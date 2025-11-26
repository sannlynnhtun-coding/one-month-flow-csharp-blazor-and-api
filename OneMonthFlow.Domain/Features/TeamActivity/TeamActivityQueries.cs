// TeamActivityQueries.cs

namespace OneMonthFlow.Domain.Features.TeamActivity;

public static class TeamActivityQueries
{
    public const string InsertTeamActivity = @"
        INSERT INTO Tbl_ProjectTeamActivity (ProjectTeamActivityId, UserId, TeamId, ProjectId, TechStackId, ActivityDate, Tasks, CreatedDate, CreatedBy)
        SELECT NEWID(), u.UserId, t.TeamId, p.ProjectId, ts.TechStackId, @ActivityDate, @Tasks, GETDATE(), @CreatedBy
        FROM Tbl_User u
        JOIN Tbl_Team t ON u.UserId = t.TeamId AND t.TeamCode = @TeamCode
        JOIN Tbl_Project p ON p.ProjectCode = @ProjectCode
        LEFT JOIN Tbl_TechStack ts ON ts.TechStackCode = @TechStackCode;
        -- Assumes CreatedBy is passed from UI";

    public const string GetActivitiesByTeamCode = @"
        SELECT a.*, u.UserName, t.TeamName, p.ProjectName, ts.TechStackName
        FROM Tbl_ProjectTeamActivity a
        JOIN Tbl_Team t ON t.TeamId = a.TeamId
        JOIN Tbl_Project p ON p.ProjectId = a.ProjectId
        JOIN Tbl_User u ON u.UserId = a.UserId
        LEFT JOIN Tbl_TechStack ts ON ts.TechStackId = a.TechStackId
        WHERE t.TeamCode = @TeamCode;";

    public const string GetActivityById = @"
        SELECT a.*, u.UserName, t.TeamName, p.ProjectName, ts.TechStackName
        FROM Tbl_ProjectTeamActivity a
        JOIN Tbl_Team t ON t.TeamId = a.TeamId
        JOIN Tbl_Project p ON p.ProjectId = a.ProjectId
        JOIN Tbl_User u ON u.UserId = a.UserId
        LEFT JOIN Tbl_TechStack ts ON ts.TechStackId = a.TechStackId
        WHERE a.ProjectTeamActivityId = @ProjectTeamActivityId;";

    public const string UpdateActivity = @"
        UPDATE Tbl_ProjectTeamActivity
        SET Tasks = @Tasks, ActivityDate = @ActivityDate
        WHERE ProjectTeamActivityId = @ProjectTeamActivityId;";

    public const string DeleteActivity = "DELETE FROM Tbl_ProjectTeamActivity WHERE ProjectTeamActivityId = @ProjectTeamActivityId;";
}
