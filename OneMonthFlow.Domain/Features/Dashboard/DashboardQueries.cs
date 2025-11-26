using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneMonthFlow.Domain.Features.Dashboard;

public static class DashboardQueries
{
    public const string GetTotalProjects = "SELECT COUNT(*) FROM Tbl_Project;";

    public const string GetActiveProjects = "SELECT COUNT(*) FROM Tbl_Project WHERE Status = 'Active';"; // Adjust 'Active' if needed

    public const string GetTotalUsers = "SELECT COUNT(*) FROM Tbl_User;";

    public const string GetTotalTeams = "SELECT COUNT(*) FROM Tbl_Team;";

    public const string GetProjectsEndingSoon = @"
            SELECT ProjectId, ProjectCode, ProjectName, EndDate, Status
            FROM Tbl_Project
            WHERE EndDate <= @CutoffDate;";

    public const string GetLatestActivities = @"
            SELECT ProjectTeamActivityId, ProjectCode, TeamCode, ActivityDate, Tasks
            FROM Tbl_ProjectTeamActivity
            ORDER BY ActivityDate DESC
            LIMIT @Count;"; 
}
