using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OneMonthFlow.Domain.Features.Dashboard;

public class DashboardService
{
    private readonly ISqlService _sqlService;

    public DashboardService(ISqlService sqlService) => _sqlService = sqlService;

    public async Task<int> GetTotalProjectsAsync()
    {
        return await _sqlService.QuerySingleAsync<int>(DashboardQueries.GetTotalProjects);
    }

    public async Task<int> GetActiveProjectsAsync()
    {
        return await _sqlService.QuerySingleAsync<int>(DashboardQueries.GetActiveProjects);
    }

    public async Task<int> GetTotalUsersAsync()
    {
        return await _sqlService.QuerySingleAsync<int>(DashboardQueries.GetTotalUsers);
    }

    public async Task<int> GetTotalTeamsAsync()
    {
        return await _sqlService.QuerySingleAsync<int>(DashboardQueries.GetTotalTeams);
    }

    public async Task<List<ProjectModel>> GetProjectsEndingSoonAsync(int days)
    {
        var cutoffDate = DateTime.Now.AddDays(days);
        return (await _sqlService.QueryAsync<ProjectModel>(DashboardQueries.GetProjectsEndingSoon, new { CutoffDate = cutoffDate })).ToList();
    }

    public async Task<List<ProjectTeamActivityModel>> GetLatestActivitiesAsync(int count)
    {
        return (await _sqlService.QueryAsync<ProjectTeamActivityModel>(DashboardQueries.GetLatestActivities, new { Count = count })).ToList();
    }
}
