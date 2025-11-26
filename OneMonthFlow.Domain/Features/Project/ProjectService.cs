namespace OneMonthFlow.Domain.Features.Project;

public class ProjectService
{
    private readonly ISqlService _sqlService;

    private const string InsertTeamTechStackQuery = @"
            INSERT INTO Tbl_TeamTechStack (TeamTechStackId, TeamCode, TechStackCode)
            VALUES (@TeamTechStackId, @TeamCode, @TechStackCode);";

    private const string CheckTeamTechStackExistsQuery = @"
            SELECT COUNT(1) FROM Tbl_TeamTechStack WHERE TeamCode = @TeamCode AND TechStackCode = @TechStackCode;";

    public ProjectService(ISqlService sqlService)
    {
        _sqlService = sqlService;
    }

    public async Task<Result<ProjectResponseModel>> CreateProjectAsync(ProjectRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.ProjectCode) || string.IsNullOrWhiteSpace(request.ProjectName))
            {
                return Result<ProjectResponseModel>.ValidationError("ProjectCode and ProjectName are required.");
            }

            if (request.Teams != null && request.Teams.Any())
            {
                var teamCodes = request.Teams.Select(t => t.TeamCode).ToList();
                var duplicateTeams = teamCodes.GroupBy(x => x)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateTeams.Any())
                {
                    return Result<ProjectResponseModel>.ValidationError(
                        $"Duplicate team(s) found in request: {string.Join(", ", duplicateTeams)}");
                }
            }

            var projectId = Guid.NewGuid().ToString();
            var projectCode = request.ProjectCode;
            var project = new ProjectModel
            {
                ProjectId = projectId,
                ProjectCode = projectCode,
                ProjectName = request.ProjectName,
                RepoUrl = request.RepoUrl,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ProjectDescription = request.ProjectDescription,
                Status = request.Status ?? "Active"
            };

            int projectRowsAffected = await _sqlService.ExecuteAsync(ProjectQueries.InsertProject, project);
            if (projectRowsAffected == 0)
            {
                return Result<ProjectResponseModel>.Failure("Failed to create project.");
            }

            var projectTeams = new List<ProjectTeamModel>();
            if (request.Teams != null && request.Teams.Any())
            {
                foreach (var teamRequest in request.Teams)
                {
                    if (string.IsNullOrWhiteSpace(teamRequest.TeamCode))
                    {
                        return Result<ProjectResponseModel>.ValidationError(
                            "TeamCode is required for all project teams.");
                    }

                    var projectTeam = new ProjectTeamModel
                    {
                        ProjectTeamId = Guid.NewGuid().ToString(),
                        ProjectCode = projectCode,
                        TeamCode = teamRequest.TeamCode,
                        ProjectTeamRating = teamRequest.ProjectTeamRating,
                        Duration = teamRequest.Duration
                    };
                    int teamRowsAffected =
                        await _sqlService.ExecuteAsync(ProjectQueries.InsertProjectTeam, projectTeam);
                    if (teamRowsAffected > 0)
                    {
                        projectTeams.Add(projectTeam);
                    }

                    if (!string.IsNullOrWhiteSpace(teamRequest.TechStackCode))
                    {
                        var existingTeamTechStackCount = await _sqlService.QuerySingleAsync<int>(
                            CheckTeamTechStackExistsQuery,
                            new { teamRequest.TeamCode, teamRequest.TechStackCode });

                        if (existingTeamTechStackCount == 0)
                        {
                            var teamTechStackEntry = new
                            {
                                TeamTechStackId = Guid.NewGuid().ToString(),
                                TeamCode = teamRequest.TeamCode,
                                TechStackCode = teamRequest.TechStackCode
                            };
                            await _sqlService.ExecuteAsync(InsertTeamTechStackQuery, teamTechStackEntry);
                        }
                    }
                }
            }

            var projectWithTeams = new ProjectWithTeamsModel
            {
                Project = project,
                Teams = projectTeams
            };

            var response = new ProjectResponseModel
            {
                Projects = new List<ProjectWithTeamsModel> { projectWithTeams },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };
            return Result<ProjectResponseModel>.Success(response, "Project and teams created successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectResponseModel>.Failure($"Error creating project: {ex.Message}");
        }
    }

    public async Task<Result<ProjectResponseModel>> GetProjectByIdAsync(string projectId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectId))
            {
                return Result<ProjectResponseModel>.ValidationError("ProjectId is required.");
            }

            var project = await _sqlService.QuerySingleAsync<ProjectModel>(
                ProjectQueries.GetProjectById,
                new { ProjectId = projectId });

            if (project == null)
            {
                return Result<ProjectResponseModel>.NotFoundError($"Project with ID {projectId} not found.");
            }

            var projectTeams = await _sqlService.QueryAsync<ProjectTeamModel>(
                ProjectQueries.GetProjectTeamsByProjectCode,
                new { ProjectCode = project.ProjectCode });

            var projectWithTeams = new ProjectWithTeamsModel
            {
                Project = project,
                Teams = projectTeams.ToList()
            };

            var response = new ProjectResponseModel
            {
                Projects = new List<ProjectWithTeamsModel> { projectWithTeams },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };
            return Result<ProjectResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<ProjectResponseModel>.Failure($"Error retrieving project: {ex.Message}");
        }
    }

    public async Task<Result<ProjectListResponseModel>> GetProjectsAsync(int page = 1, int pageSize = 10,
        string? filterColumn = null, string? filterValue = null)
    {
        try
        {
            if (page < 1 || pageSize < 1)
            {
                return Result<ProjectListResponseModel>.ValidationError("Page and PageSize must be greater than 0.");
            }

            var offset = (page - 1) * pageSize;
            var parameters = new
            {
                PageSize = pageSize,
                Offset = offset,
                FilterValue = string.IsNullOrWhiteSpace(filterValue) ? null : $"%{filterValue}%"
            };

            var projects = await _sqlService.QueryAsync<ProjectModel>(
                ProjectQueries.GetProjectsPaginated(page, pageSize, filterColumn, filterValue),
                parameters);

            var totalCountParameters = new { FilterValue = string.IsNullOrWhiteSpace(filterValue) ? null : $"%{filterValue}%" };
            var totalCount = await _sqlService.QuerySingleAsync<int>(
                ProjectQueries.GetProjectCount(filterColumn, filterValue),
                totalCountParameters);

            var response = new ProjectListResponseModel
            {
                Projects = projects.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<ProjectListResponseModel>.Success(response, "Projects retrieved successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectListResponseModel>.Failure($"Error retrieving projects: {ex.Message}");
        }
    }

    public async Task<Result<ProjectListResponseModel>> GetProjectsWithPaginationAsync(
        int page,
        int pageSize,
        string? sortBy,
        bool sortDescending,
        string? filterTerm)
    {
        try
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;

            var offset = (page - 1) * pageSize;

            var queryBuilder = new StringBuilder("SELECT * FROM Tbl_Project ");
            var countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM Tbl_Project ");

            object queryParameters;
            var filterCondition = "";
            if (!string.IsNullOrWhiteSpace(filterTerm))
            {
                filterCondition = "WHERE (ProjectCode LIKE @FilterTerm OR ProjectName LIKE @FilterTerm OR Status LIKE @FilterTerm) ";
                queryBuilder.Append(filterCondition);
                countQueryBuilder.Append(filterCondition);
            }

            string orderByClause = "ORDER BY ProjectName ";
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                var allowedSortColumns = new List<string> { "ProjectCode", "ProjectName", "Status", "StartDate", "EndDate" };
                if (allowedSortColumns.Contains(sortBy, StringComparer.OrdinalIgnoreCase))
                {
                    orderByClause = $"ORDER BY {sortBy} {(sortDescending ? "DESC" : "ASC")} ";
                }
            }
            queryBuilder.Append(orderByClause);

            queryBuilder.Append("OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY");

            if (!string.IsNullOrWhiteSpace(filterTerm))
            {
                queryParameters = new { PageSize = pageSize, Offset = offset, FilterTerm = $"%{filterTerm}%" };
            }
            else
            {
                queryParameters = new { PageSize = pageSize, Offset = offset };
            }

            var projects = await _sqlService.QueryAsync<ProjectModel>(queryBuilder.ToString(), queryParameters);
            var totalCount = await _sqlService.QuerySingleAsync<int>(countQueryBuilder.ToString(),
                !string.IsNullOrWhiteSpace(filterTerm) ? new { FilterTerm = $"%{filterTerm}%" } : null);

            var response = new ProjectListResponseModel
            {
                Projects = projects.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<ProjectListResponseModel>.Success(response, "Projects retrieved successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectListResponseModel>.Failure($"Error retrieving paginated projects: {ex.Message}");
        }
    }

    public async Task<Result<ProjectResponseModel>> UpdateProjectAsync(string projectId, ProjectRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectId))
            {
                return Result<ProjectResponseModel>.ValidationError("ProjectId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.ProjectCode) || string.IsNullOrWhiteSpace(request.ProjectName))
            {
                return Result<ProjectResponseModel>.ValidationError("ProjectCode and ProjectName are required.");
            }

            var project = new ProjectModel
            {
                ProjectId = projectId,
                ProjectCode = request.ProjectCode,
                ProjectName = request.ProjectName,
                RepoUrl = request.RepoUrl,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                ProjectDescription = request.ProjectDescription
            };

            int projectRowsAffected = await _sqlService.ExecuteAsync(ProjectQueries.UpdateProject, project);
            if (projectRowsAffected == 0)
            {
                return Result<ProjectResponseModel>.NotFoundError($"Project with ID {projectId} not found.");
            }

            await _sqlService.ExecuteAsync(ProjectQueries.DeleteProjectTeams,
                new { ProjectCode = request.ProjectCode });

            var projectTeams = new List<ProjectTeamModel>();
            if (request.Teams != null && request.Teams.Any())
            {
                foreach (var teamRequest in request.Teams)
                {
                    if (string.IsNullOrWhiteSpace(teamRequest.TeamCode))
                    {
                        return Result<ProjectResponseModel>.ValidationError(
                            "TeamCode is required for all project teams.");
                    }

                    var projectTeam = new ProjectTeamModel
                    {
                        ProjectTeamId = Guid.NewGuid().ToString(),
                        ProjectCode = request.ProjectCode,
                        TeamCode = teamRequest.TeamCode,
                        ProjectTeamRating = teamRequest.ProjectTeamRating,
                        Duration = teamRequest.Duration
                    };
                    int teamRowsAffected =
                        await _sqlService.ExecuteAsync(ProjectQueries.InsertProjectTeam, projectTeam);
                    if (teamRowsAffected > 0)
                    {
                        projectTeams.Add(projectTeam);
                    }

                    if (!string.IsNullOrWhiteSpace(teamRequest.TechStackCode))
                    {
                        var existingTeamTechStackCount = await _sqlService.QuerySingleAsync<int>(
                            CheckTeamTechStackExistsQuery,
                            new { teamRequest.TeamCode, teamRequest.TechStackCode });

                        if (existingTeamTechStackCount == 0)
                        {
                            var teamTechStackEntry = new
                            {
                                TeamTechStackId = Guid.NewGuid().ToString(),
                                TeamCode = teamRequest.TeamCode,
                                TechStackCode = teamRequest.TechStackCode
                            };
                            await _sqlService.ExecuteAsync(InsertTeamTechStackQuery, teamTechStackEntry);
                        }
                    }
                }
            }

            var projectWithTeams = new ProjectWithTeamsModel
            {
                Project = project,
                Teams = projectTeams
            };

            var response = new ProjectResponseModel
            {
                Projects = new List<ProjectWithTeamsModel> { projectWithTeams },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };
            return Result<ProjectResponseModel>.Success(response, "Project and teams updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectResponseModel>.Failure($"Error updating project: {ex.Message}");
        }
    }

    public async Task<Result<ProjectResponseModel>> DeleteProjectAsync(string projectId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectId))
            {
                return Result<ProjectResponseModel>.ValidationError("ProjectId is required.");
            }

            var projectToDelete = await _sqlService.QuerySingleAsync<ProjectModel>(
                ProjectQueries.GetProjectById,
                new { ProjectId = projectId });

            if (projectToDelete == null)
            {
                return Result<ProjectResponseModel>.NotFoundError($"Project with ID {projectId} not found.");
            }

            await _sqlService.ExecuteAsync(ProjectQueries.DeleteProjectTeams,
                new { ProjectCode = projectToDelete.ProjectCode });

            int rowsAffected =
                await _sqlService.ExecuteAsync(ProjectQueries.DeleteProject, new { ProjectId = projectId });

            if (rowsAffected > 0)
            {
                return Result<ProjectResponseModel>.Success(
                    new ProjectResponseModel { Projects = new List<ProjectWithTeamsModel>() },
                    "Project and associated teams deleted successfully.");
            }
            else
            {
                return Result<ProjectResponseModel>.NotFoundError(
                    $"Project with ID {projectId} could not be deleted as it was not found post dependency check.");
            }
        }
        catch (Exception ex)
        {
            return Result<ProjectResponseModel>.Failure($"Error deleting project: {ex.Message}");
        }
    }

    public async Task<Result<List<TeamModel>>> GetAllTeamsAsync()
    {
        try
        {
            var teams = await _sqlService.QueryAsync<TeamModel>("SELECT * FROM Tbl_Team ORDER BY TeamCode");
            return Result<List<TeamModel>>.Success(teams.ToList(), "Teams retrieved successfully.");
        }
        catch (Exception ex)
        {
            return Result<List<TeamModel>>.Failure($"Error retrieving teams: {ex.Message}");
        }
    }

    public async Task<Result<List<ProjectModel>>> SearchProjectsAsync(string keyword)
    {
        try
        {
            var query = @"SELECT * FROM Tbl_Project
                                 WHERE (@Keyword IS NULL OR @Keyword = '' OR ProjectCode LIKE @KeywordPattern OR ProjectName LIKE @KeywordPattern)
                                 ORDER BY ProjectName";
            var projects = await _sqlService.QueryAsync<ProjectModel>(query,
                new { Keyword = keyword, KeywordPattern = $"%{keyword}%" });

            return Result<List<ProjectModel>>.Success(projects.ToList(), "Search results returned.");
        }
        catch (Exception ex)
        {
            return Result<List<ProjectModel>>.Failure($"Error searching projects: {ex.Message}");
        }
    }

    public async Task<Result<bool>> AddTeamsToProjectAsync(string projectCode, List<ProjectTeamRequestModel> teams)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectCode))
                return Result<bool>.ValidationError("ProjectCode is required.");

            if (teams == null || !teams.Any())
                return Result<bool>.ValidationError("At least one team is required.");

            var teamCodesInRequest = teams.Select(t => t.TeamCode).ToList();
            var duplicateTeamsInRequest = teamCodesInRequest.GroupBy(x => x)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key)
                .ToList();

            if (duplicateTeamsInRequest.Any())
            {
                return Result<bool>.ValidationError(
                    $"Duplicate team(s) found in request: {string.Join(", ", duplicateTeamsInRequest)}");
            }

            foreach (var teamRequest in teams)
            {
                if (string.IsNullOrWhiteSpace(teamRequest.TeamCode))
                    return Result<bool>.ValidationError("TeamCode is required for all teams.");

                if (await IsTeamAlreadyInProject(projectCode, teamRequest.TeamCode))
                {
                    return Result<bool>.ValidationError(
                        $"Team {teamRequest.TeamCode} is already assigned to this project.");
                }
            }

            foreach (var teamRequest in teams)
            {
                var projectTeamLink = new ProjectTeamModel
                {
                    ProjectTeamId = Guid.NewGuid().ToString(),
                    ProjectCode = projectCode,
                    TeamCode = teamRequest.TeamCode,
                    ProjectTeamRating = teamRequest.ProjectTeamRating,
                    Duration = teamRequest.Duration
                };
                await _sqlService.ExecuteAsync(ProjectQueries.InsertProjectTeam, projectTeamLink);

                if (!string.IsNullOrWhiteSpace(teamRequest.TechStackCode))
                {
                    var existingTeamTechStackCount = await _sqlService.QuerySingleAsync<int>(
                        CheckTeamTechStackExistsQuery,
                        new { teamRequest.TeamCode, teamRequest.TechStackCode });

                    if (existingTeamTechStackCount == 0)
                    {
                        var teamTechStackEntry = new
                        {
                            TeamTechStackId = Guid.NewGuid().ToString(),
                            TeamCode = teamRequest.TeamCode,
                            TechStackCode = teamRequest.TechStackCode
                        };
                        await _sqlService.ExecuteAsync(InsertTeamTechStackQuery, teamTechStackEntry);
                    }
                }
            }

            return Result<bool>.Success(true, "Teams added to project successfully.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error adding teams to project: {ex.Message}");
        }
    }

    public async Task<Result<ProjectResponseModel>> AddProjectAsync(ProjectModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.ProjectCode) || string.IsNullOrWhiteSpace(model.ProjectName))
            {
                return Result<ProjectResponseModel>.ValidationError("ProjectCode and ProjectName are required.");
            }

            model.ProjectId ??= Guid.NewGuid().ToString();

            var rowsAffected = await _sqlService.ExecuteAsync(ProjectQueries.InsertProject, model);
            if (rowsAffected == 0)
            {
                return Result<ProjectResponseModel>.Failure("Failed to add project.");
            }

            var createdProject = await _sqlService.QuerySingleAsync<ProjectModel>(
                ProjectQueries.GetProjectById, new { ProjectId = model.ProjectId });

            var response = new ProjectResponseModel
            {
                Projects = new List<ProjectWithTeamsModel>
                    {
                        new ProjectWithTeamsModel { Project = createdProject ?? model, Teams = new List<ProjectTeamModel>() }
                    },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };

            return Result<ProjectResponseModel>.Success(response, "Project added successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectResponseModel>.Failure($"Error adding project: {ex.Message}");
        }
    }

    public async Task<Result<ProjectResponseModel>> UpdateProjectAsync(ProjectModel model)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(model.ProjectId) ||
                string.IsNullOrWhiteSpace(model.ProjectCode) ||
                string.IsNullOrWhiteSpace(model.ProjectName))
            {
                return Result<ProjectResponseModel>.ValidationError(
                    "ProjectId, ProjectCode, and ProjectName are required.");
            }

            var rowsAffected = await _sqlService.ExecuteAsync(ProjectQueries.UpdateProject, model);
            if (rowsAffected == 0)
            {
                return Result<ProjectResponseModel>.NotFoundError("Project not found or update failed.");
            }

            var updatedProject = await _sqlService.QuerySingleAsync<ProjectModel>(
                ProjectQueries.GetProjectById, new { ProjectId = model.ProjectId });

            var projectTeams = await _sqlService.QueryAsync<ProjectTeamModel>(
                ProjectQueries.GetProjectTeamsByProjectCode, new { ProjectCode = updatedProject?.ProjectCode ?? model.ProjectCode });

            var response = new ProjectResponseModel
            {
                Projects = new List<ProjectWithTeamsModel>
                    {
                        new ProjectWithTeamsModel { Project = updatedProject ?? model, Teams = projectTeams.ToList() }
                    },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };

            return Result<ProjectResponseModel>.Success(response, "Project updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<ProjectResponseModel>.Failure($"Error updating project: {ex.Message}");
        }
    }

    public async Task<Result<ProjectResponseModel>> DeleteProjectAsync(Guid projectId)
    {
        return await DeleteProjectAsync(projectId.ToString());
    }

    public async Task<Result<List<TeamModel>>> GetTeamsByProjectIdAsync(string projectCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectCode))
            {
                return Result<List<TeamModel>>.ValidationError("ProjectCode cannot be null or whitespace.");
            }

            var teams = await _sqlService.QueryAsync<TeamModel>(
                ProjectQueries.GetTeamsByProjectId,
                new { ProjectCode = projectCode });

            return Result<List<TeamModel>>.Success(teams.ToList(), "Teams retrieved successfully.");
        }
        catch (Exception ex)
        {
            return Result<List<TeamModel>>.Failure(
                $"An error occurred while retrieving teams for project {projectCode}: {ex.Message}");
        }
    }

    public async Task<Result<bool>> RemoveTeamFromProjectAsync(string projectCode, string teamCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(projectCode) || string.IsNullOrWhiteSpace(teamCode))
            {
                return Result<bool>.ValidationError("ProjectCode and TeamCode are required.");
            }
            const string removeQuery =
                "DELETE FROM Tbl_ProjectTeam WHERE ProjectCode = @ProjectCode AND TeamCode = @TeamCode;";
            int rowsAffected =
                await _sqlService.ExecuteAsync(removeQuery, new { ProjectCode = projectCode, TeamCode = teamCode });

            if (rowsAffected > 0) return Result<bool>.Success(true, "Team removed successfully from project.");
            return Result<bool>.NotFoundError("Failed to remove team: Team not found in project or project does not exist.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error removing team from project: {ex.Message}");
        }
    }

    private async Task<bool> IsTeamAlreadyInProject(string projectCode, string teamCode)
    {
        const string query =
            "SELECT COUNT(*) FROM Tbl_ProjectTeam WHERE ProjectCode = @ProjectCode AND TeamCode = @TeamCode";
        var count = await _sqlService.QuerySingleAsync<int>(query,
            new { ProjectCode = projectCode, TeamCode = teamCode });
        return count > 0;
    }
}