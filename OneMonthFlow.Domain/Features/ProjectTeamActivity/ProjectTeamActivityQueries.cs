namespace OneMonthFlow.Domain.Features.ProjectTeamActivity;

public static class ProjectTeamActivityQueries
{
    public const string GetUserByCode = @"
            SELECT UserId, UserCode, UserName, GitHubAccountName, Nrc, MobileNo
            FROM Tbl_User WHERE UserCode = @UserCode;";

    public const string SearchUsers = @"
            SELECT UserId, UserCode, UserName, GitHubAccountName, Nrc, MobileNo
            FROM Tbl_User WHERE LOWER(UserName) LIKE @SearchPattern OR UserCode = @SearchTerm;";

    public const string GetTechStackByCode = @"
            SELECT TechStackId, TechStackCode, TechStackShortCode, TechStackName
            FROM Tbl_TechStack WHERE TechStackCode = @TechStackCode;";

    public const string SearchTechStacks = @"
            SELECT TechStackId, TechStackCode, TechStackShortCode, TechStackName
            FROM Tbl_TechStack WHERE LOWER(TechStackName) LIKE @SearchPattern OR TechStackCode = @SearchTerm;";

    public const string InsertProjectTeamActivity = @"
            INSERT INTO Tbl_ProjectTeamActivity (ProjectTeamActivityId, ProjectCode, TeamCode, ActivityDate, Tasks)
            VALUES (@ProjectTeamActivityId, @ProjectCode, @TeamCode, @ActivityDate, @Tasks);";

    public const string UpdateProjectTeamActivity = @"
            UPDATE Tbl_ProjectTeamActivity
            SET ProjectCode = @ProjectCode, TeamCode = @TeamCode, ActivityDate = @ActivityDate, Tasks = @Tasks
            WHERE ProjectTeamActivityId = @ProjectTeamActivityId;";

    public const string GetProjectTeamActivitiesByProject = @"
            SELECT pta.ProjectTeamActivityId, pta.ProjectCode, pta.TeamCode, pta.ActivityDate, pta.Tasks,
                   p.ProjectName, t.TeamName
            FROM Tbl_ProjectTeamActivity pta
            INNER JOIN Tbl_Project p ON pta.ProjectCode = p.ProjectCode
            INNER JOIN Tbl_Team t ON pta.TeamCode = t.TeamCode
            WHERE pta.ProjectCode = @ProjectCode
            ORDER BY pta.ActivityDate DESC;";

    public const string GetActivitiesBase = @"
            SELECT pta.ProjectTeamActivityId, pta.ProjectCode, pta.TeamCode, pta.ActivityDate, pta.Tasks,
                   p.ProjectName, t.TeamName
            FROM Tbl_ProjectTeamActivity pta
            INNER JOIN Tbl_Project p ON pta.ProjectCode = p.ProjectCode
            INNER JOIN Tbl_Team t ON pta.TeamCode = t.TeamCode";

    public const string GetProjectTeamActivityById = @"
            SELECT pta.ProjectTeamActivityId, pta.ProjectCode, pta.TeamCode, pta.ActivityDate, pta.Tasks,
                   p.ProjectName, t.TeamName
            FROM Tbl_ProjectTeamActivity pta
            INNER JOIN Tbl_Project p ON pta.ProjectCode = p.ProjectCode
            INNER JOIN Tbl_Team t ON pta.TeamCode = t.TeamCode
            WHERE pta.ProjectTeamActivityId = @ProjectTeamActivityId;";

    public const string DeleteProjectTeamActivity = @"
            DELETE FROM Tbl_ProjectTeamActivity WHERE ProjectTeamActivityId = @ProjectTeamActivityId;";

    public const string GetTeamByCodeQuery = @"
            SELECT TeamId, TeamCode, TeamName FROM Tbl_Team WHERE TeamCode = @TeamCode;";

    public const string GetProjectsByTeamCodeQuery = @"
            SELECT p.ProjectId, p.ProjectCode, p.ProjectName, p.RepoUrl, p.StartDate, p.EndDate, p.ProjectDescription, p.Status
            FROM Tbl_Project p
            INNER JOIN Tbl_ProjectTeam pt ON p.ProjectCode = pt.ProjectCode
            WHERE pt.TeamCode = @TeamCode;";

    public const string SearchTeamsQuery = @"
            SELECT TeamId, TeamCode, TeamName FROM Tbl_Team
            WHERE LOWER(TeamName) LIKE @Query OR LOWER(TeamCode) LIKE @Query;";
}