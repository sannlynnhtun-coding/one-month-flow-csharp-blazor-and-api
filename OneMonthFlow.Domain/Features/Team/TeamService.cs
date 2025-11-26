using OneMonthFlow.Domain.Shared; // For Result, TeamModel, TeamRequestModel, TechStackModel
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OneMonthFlow.Domain.Features.Team;

public class TeamService
{
    private readonly ISqlService _sqlService;

    public TeamService(ISqlService sqlService) => _sqlService = sqlService;

    public async Task<Result<List<string>>> GetTechStackCodesByTeamCodeAsync(string teamCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamCode))
            {
                return Result<List<string>>.ValidationError("TeamCode is required.");
            }
            var techStackCodes = await _sqlService.QueryAsync<string>(TeamQueries.GetTechStackCodesByTeamCode, new { TeamCode = teamCode });
            return Result<List<string>>.Success(techStackCodes.ToList(), "Tech stacks retrieved successfully for team.");
        }
        catch (Exception ex)
        {
            return Result<List<string>>.Failure($"Error retrieving tech stacks for team {teamCode}: {ex.Message}");
        }
    }


    public async Task<Result<TeamResponseModel>> CreateTeamAsync(TeamRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.TeamCode) || string.IsNullOrWhiteSpace(request.TeamName))
            {
                return Result<TeamResponseModel>.ValidationError("TeamCode and TeamName are required.");
            }

            // Check if TeamCode already exists
            var existingTeamByCode = await _sqlService.QuerySingleAsync<TeamModel>(TeamQueries.GetTeamByCode, new { request.TeamCode });
            if (existingTeamByCode != null)
            {
                return Result<TeamResponseModel>.ValidationError($"Team with Code '{request.TeamCode}' already exists.");
            }

            var teamId = Guid.NewGuid().ToString();
            var team = new TeamModel
            {
                TeamId = teamId,
                TeamCode = request.TeamCode,
                TeamName = request.TeamName
            };

            int rowsAffected = await _sqlService.ExecuteAsync(TeamQueries.InsertTeam, team);
            if (rowsAffected == 0)
            {
                return Result<TeamResponseModel>.Failure("Failed to create team.");
            }

            // Add Tech Stacks
            if (request.TechStackCodes != null && request.TechStackCodes.Any())
            {
                foreach (var techStackCode in request.TechStackCodes)
                {
                    await _sqlService.ExecuteAsync(TeamQueries.InsertTeamTechStack, new
                    {
                        TeamTechStackId = Guid.NewGuid().ToString(),
                        TeamCode = request.TeamCode,
                        TechStackCode = techStackCode
                    });
                }
            }

            var teamWithUsers = new TeamWithUsersModel // Assuming TeamWithUsersModel exists
            {
                Team = team,
                Users = new List<TeamUserModel>() // Assuming TeamUserModel exists
            };

            var response = new TeamResponseModel // Assuming TeamResponseModel exists
            {
                Teams = new List<TeamWithUsersModel> { teamWithUsers },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };

            return Result<TeamResponseModel>.Success(response, "Team and associated tech stacks created successfully.");
        }
        catch (Exception ex)
        {
            // Consider more specific exception handling, e.g., for unique constraint violations if TeamCode should be unique in DB
            return Result<TeamResponseModel>.Failure($"Error creating team: {ex.Message}");
        }
    }

    public async Task<Result<TeamResponseModel>> GetTeamByIdAsync(string teamId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamId))
            {
                return Result<TeamResponseModel>.ValidationError("TeamId is required.");
            }

            var team = await _sqlService.QuerySingleAsync<TeamModel>(TeamQueries.GetTeamById, new { TeamId = teamId });
            if (team == null)
            {
                return Result<TeamResponseModel>.NotFoundError($"Team with ID {teamId} not found.");
            }

            // Optionally, load associated tech stacks here if TeamResponseModel should include them.
            // For this example, TeamResponseModel is kept as is.

            var teamWithUsers = new TeamWithUsersModel
            {
                Team = team,
                Users = new List<TeamUserModel>()
            };

            var response = new TeamResponseModel
            {
                Teams = new List<TeamWithUsersModel> { teamWithUsers },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };

            return Result<TeamResponseModel>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<TeamResponseModel>.Failure($"Error retrieving team: {ex.Message}");
        }
    }

    public async Task<Result<TeamListResponseModel>> GetTeamsAsync(int page = 1, int pageSize = 10, string filterColumn = null, string filterValue = null)
    {
        try
        {
            if (page < 1 || pageSize < 1)
            {
                return Result<TeamListResponseModel>.ValidationError("Page and PageSize must be greater than 0.");
            }

            var offset = (page - 1) * pageSize;
            var parameters = new
            {
                PageSize = pageSize,
                Offset = offset,
                FilterValue = $"%{filterValue}%" // Ensure filterValue is handled if null
            };

            var teams = await _sqlService.QueryAsync<TeamModel>(TeamQueries.GetTeamsPaginated, parameters);
            var totalCount = await _sqlService.QuerySingleAsync<int>(TeamQueries.GetTeamCount, new { FilterValue = $"%{filterValue}%" });

            var response = new TeamListResponseModel // Assuming TeamListResponseModel exists
            {
                Teams = teams.ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };

            return Result<TeamListResponseModel>.Success(response, "Teams retrieved successfully.");
        }
        catch (Exception ex)
        {
            return Result<TeamListResponseModel>.Failure($"Error retrieving teams: {ex.Message}");
        }
    }

    public async Task<Result<TeamResponseModel>> UpdateTeamAsync(string teamId, TeamRequestModel request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamId))
            {
                return Result<TeamResponseModel>.ValidationError("TeamId is required.");
            }

            if (string.IsNullOrWhiteSpace(request.TeamCode) || string.IsNullOrWhiteSpace(request.TeamName))
            {
                return Result<TeamResponseModel>.ValidationError("TeamCode and TeamName are required.");
            }

            // Fetch the existing team to get its current TeamCode, as TeamCode is not updated by TeamQueries.UpdateTeam
            var existingTeam = await _sqlService.QuerySingleAsync<TeamModel>(TeamQueries.GetTeamById, new { TeamId = teamId });
            if (existingTeam == null)
            {
                return Result<TeamResponseModel>.NotFoundError($"Team with ID {teamId} not found for update.");
            }

            // Note: TeamQueries.UpdateTeam only updates TeamName. TeamCode is not changed.
            // If TeamCode could be updated, logic for tech stacks would need the old and new TeamCode.
            var teamToUpdate = new TeamModel
            {
                TeamId = teamId,
                TeamCode = existingTeam.TeamCode, // Use existing TeamCode as it's not being updated
                TeamName = request.TeamName
            };

            int rowsAffected = await _sqlService.ExecuteAsync(TeamQueries.UpdateTeam, teamToUpdate);
            // No need to check rowsAffected here for NotFound, as we checked for existingTeam above.
            // If rowsAffected is 0 despite existingTeam being found, it might indicate a concurrency issue or other problem.

            // Update Tech Stacks
            // 1. Delete existing tech stacks for this team (using the original/current TeamCode)
            await _sqlService.ExecuteAsync(TeamQueries.DeleteTeamTechStacksByTeamCode, new { TeamCode = existingTeam.TeamCode });

            // 2. Add new selected tech stacks
            if (request.TechStackCodes != null && request.TechStackCodes.Any())
            {
                foreach (var techStackCode in request.TechStackCodes)
                {
                    await _sqlService.ExecuteAsync(TeamQueries.InsertTeamTechStack, new
                    {
                        TeamTechStackId = Guid.NewGuid().ToString(),
                        TeamCode = existingTeam.TeamCode, // Use existing TeamCode
                        TechStackCode = techStackCode
                    });
                }
            }

            // Construct response based on the potentially updated 'teamToUpdate' which only has TeamName changed from request
            var teamWithUsers = new TeamWithUsersModel
            {
                Team = teamToUpdate, // This model has the updated TeamName and original TeamCode
                Users = new List<TeamUserModel>()
            };

            var response = new TeamResponseModel
            {
                Teams = new List<TeamWithUsersModel> { teamWithUsers },
                TotalCount = 1,
                Page = 1,
                PageSize = 1
            };

            // The original code returned NotFound if rowsAffected was 0.
            // We've already confirmed the team exists. If update itself affected 0 rows for some reason
            // (e.g. data didn't change), it's still logically a success in terms of the operation attempting an update.
            // For consistency with user's original code for Update:
            if (rowsAffected == 0 && existingTeam.TeamName == request.TeamName && (request.TechStackCodes == null || !request.TechStackCodes.Any()))
            {
                // No actual change to team name, and no tech stacks to add (old ones deleted if any)
                // This could be considered a "no operation" success.
            }
            else if (rowsAffected == 0 && existingTeam.TeamName != request.TeamName)
            {
                // This would be an unexpected failure if team name was meant to change but didn't.
                return Result<TeamResponseModel>.Failure($"Failed to update team details for ID {teamId}, though tech stacks might have been processed.");
            }


            return Result<TeamResponseModel>.Success(response, "Team and associated tech stacks updated successfully.");
        }
        catch (Exception ex)
        {
            return Result<TeamResponseModel>.Failure($"Error updating team: {ex.Message}");
        }
    }

    public async Task<Result<bool>> DeleteTeamAsync(string teamId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(teamId))
            {
                return Result<bool>.ValidationError("TeamId is required.");
            }

            var team = await _sqlService.QuerySingleAsync<TeamModel>(TeamQueries.GetTeamById, new { TeamId = teamId });
            if (team == null)
            {
                return Result<bool>.NotFoundError($"Team with ID {teamId} not found.");
            }

            // Delete associated tech stacks first
            await _sqlService.ExecuteAsync(TeamQueries.DeleteTeamTechStacksByTeamCode, new { team.TeamCode });

            // Then delete the team
            int rowsAffected = await _sqlService.ExecuteAsync(TeamQueries.DeleteTeam, new { TeamId = teamId });

            return rowsAffected > 0
                ? Result<bool>.Success(true, "Team and associated tech stacks deleted successfully.")
                : Result<bool>.Failure($"Failed to delete team with ID {teamId}. It might have been deleted by another process.");
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure($"Error deleting team: {ex.Message}");
        }
    }
}