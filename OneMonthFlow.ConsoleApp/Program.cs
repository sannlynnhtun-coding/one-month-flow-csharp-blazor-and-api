using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace OneMonthFlow.ConsoleApp;

class Program
{
    private static ProjectService _projectService;
    private static TeamService _teamService;
    private static TeamActivityService _teamActivityService;
    private static UserService _userService;

    static async Task Main(string[] args)
    {
        InitializeServices();

        Console.WriteLine("=== OneMonthFlow Management System ===");
        Console.WriteLine();

        while (true)
        {
            ShowMainMenu();
            var choice = GetMenuChoice(1, 5);

            switch (choice)
            {
                case 1:
                    await ManageProjects();
                    break;
                case 2:
                    await ManageTeams();
                    break;
                case 3:
                    await ManageUsers();
                    break;
                case 4:
                    await ManageActivities();
                    break;
                case 5:
                    Console.WriteLine("Exiting application...");
                    return;
            }
        }
    }

    private static ILogger<MssqlService> _logger;
    private static void InitializeServices()
    {
        // Build configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        // Create logger factory
        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .SetMinimumLevel(LogLevel.Information)
                .AddSerilog();
        });

        // Create logger
        _logger = loggerFactory.CreateLogger<MssqlService>();

        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        IDbConnectionFactory dbConnectionFactory = new MssqlConnectionFactory(connectionString);
        var sqlService = new MssqlService(dbConnectionFactory, _logger);
        _projectService = new ProjectService(sqlService);
        _teamService = new TeamService(sqlService);
        _teamActivityService = new TeamActivityService(sqlService);
        _userService = new UserService(sqlService);

        _logger.LogInformation("Services initialized successfully");
    }

    #region Main Menu
    private static void ShowMainMenu()
    {
        Console.Clear();
        Console.WriteLine("MAIN MENU");
        Console.WriteLine("1. Project Management");
        Console.WriteLine("2. Team Management");
        Console.WriteLine("3. User Management");
        Console.WriteLine("4. Activity Management");
        Console.WriteLine("5. Exit");
        Console.Write("Enter your choice (1-5): ");
    }
    #endregion

    #region Project Management
    private static async Task ManageProjects()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("PROJECT MANAGEMENT");
            Console.WriteLine("1. Create New Project");
            Console.WriteLine("2. View All Projects");
            Console.WriteLine("3. View Project Details");
            Console.WriteLine("4. Update Project");
            Console.WriteLine("5. Delete Project");
            Console.WriteLine("6. Back to Main Menu");
            Console.Write("Enter your choice (1-6): ");

            var choice = GetMenuChoice(1, 6);
            if (choice == 6) break;

            switch (choice)
            {
                case 1:
                    await CreateProject();
                    break;
                case 2:
                    await ListProjects();
                    break;
                case 3:
                    await ViewProjectDetails();
                    break;
                case 4:
                    await UpdateProject();
                    break;
                case 5:
                    await DeleteProject();
                    break;
            }

            PressAnyKeyToContinue();
        }
    }

    private static async Task CreateProject()
    {
        Console.WriteLine("\nCREATE NEW PROJECT");

        var request = new ProjectRequestModel
        {
            ProjectCode = GetInput("Project Code (required): ", required: true),
            ProjectName = GetInput("Project Name (required): ", required: true),
            RepoUrl = GetInput("Repository URL: "),
            StartDate = GetDateInput("Start Date (yyyy-mm-dd): "),
            EndDate = GetDateInput("End Date (yyyy-mm-dd): "),
            ProjectDescription = GetInput("Description: ")
        };

        Console.WriteLine("\nCreating project...");
        var result = await _projectService.CreateProjectAsync(request);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully created project!");
            Console.WriteLine($"Project ID: {result.Data.Projects[0].Project.ProjectId}");
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private static async Task ListProjects()
    {
        Console.WriteLine("\nLIST ALL PROJECTS");
        var result = await _projectService.GetProjectsAsync();

        if (result.IsSuccess && result.Data.Projects.Any())
        {
            Console.WriteLine("\nProjects:");
            foreach (var project in result.Data.Projects)
            {
                Console.WriteLine($"ID: {project.ProjectId}, Code: {project.ProjectCode}, Name: {project.ProjectName}");
            }
        }
        else
        {
            ShowError(result.Message ?? "No projects found.");
        }
    }

    private static async Task ViewProjectDetails()
    {
        Console.WriteLine("\nVIEW PROJECT DETAILS");
        var projectId = GetInput("Enter Project ID: ", required: true);

        var result = await _projectService.GetProjectByIdAsync(projectId);

        if (result.IsSuccess && result.Data.Projects.Any())
        {
            var project = result.Data.Projects[0].Project;
            Console.WriteLine("\nProject Details:");
            Console.WriteLine($"ID: {project.ProjectId}");
            Console.WriteLine($"Code: {project.ProjectCode}");
            Console.WriteLine($"Name: {project.ProjectName}");
            Console.WriteLine($"Repository URL: {project.RepoUrl}");
            Console.WriteLine($"Start Date: {project.StartDate?.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"End Date: {project.EndDate?.ToString("yyyy-MM-dd")}");
            Console.WriteLine($"Description: {project.ProjectDescription}");
        }
        else
        {
            ShowError(result.Message ?? "Project not found.");
        }
    }

    private static async Task UpdateProject()
    {
        Console.WriteLine("\nUPDATE PROJECT");
        var projectId = GetInput("Enter Project ID: ", required: true);

        var request = new ProjectRequestModel
        {
            ProjectCode = GetInput("Project Code (required): ", required: true),
            ProjectName = GetInput("Project Name (required): ", required: true),
            RepoUrl = GetInput("Repository URL: "),
            StartDate = GetDateInput("Start Date (yyyy-mm-dd): "),
            EndDate = GetDateInput("End Date (yyyy-mm-dd): "),
            ProjectDescription = GetInput("Description: ")
        };

        Console.WriteLine("\nUpdating project...");
        var result = await _projectService.UpdateProjectAsync(projectId, request);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully updated project!");
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private static async Task DeleteProject()
    {
        Console.WriteLine("\nDELETE PROJECT");
        var projectId = GetInput("Enter Project ID: ", required: true);

        Console.WriteLine("\nDeleting project...");
        var result = await _projectService.DeleteProjectAsync(projectId);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully deleted project!");
        }
        else
        {
            ShowError(result.Message);
        }
    }
    #endregion

    #region Team Management
    private static async Task ManageTeams()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("TEAM MANAGEMENT");
            Console.WriteLine("1. Create New Team");
            Console.WriteLine("2. View All Teams");
            Console.WriteLine("3. View Team Details");
            Console.WriteLine("4. Update Team");
            Console.WriteLine("5. Delete Team");
            Console.WriteLine("6. Back to Main Menu");
            Console.Write("Enter your choice (1-6): ");

            var choice = GetMenuChoice(1, 6);
            if (choice == 6) break;

            switch (choice)
            {
                case 1:
                    await CreateTeam();
                    break;
                case 2:
                    await ListTeams();
                    break;
                case 3:
                    await ViewTeamDetails();
                    break;
                case 4:
                    await UpdateTeam();
                    break;
                case 5:
                    await DeleteTeam();
                    break;
            }

            PressAnyKeyToContinue();
        }
    }

    private static async Task CreateTeam()
    {
        Console.WriteLine("\nCREATE NEW TEAM");

        var request = new TeamRequestModel
        {
            TeamCode = GetInput("Team Code (required): ", required: true),
            TeamName = GetInput("Team Name (required): ", required: true)
        };

        Console.WriteLine("\nCreating team...");
        var result = await _teamService.CreateTeamAsync(request);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully created team!");
            Console.WriteLine($"Team ID: {result.Data.Teams[0].Team.TeamId}");
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private static async Task ListTeams()
    {
        Console.WriteLine("\nLIST ALL TEAMS");
        var result = await _teamService.GetTeamsAsync();

        if (result.IsSuccess && result.Data.Teams.Any())
        {
            Console.WriteLine("\nTeams:");
            foreach (var team in result.Data.Teams)
            {
                Console.WriteLine($"ID: {team.TeamId}, Code: {team.TeamCode}, Name: {team.TeamName}");
            }
        }
        else
        {
            ShowError(result.Message ?? "No teams found.");
        }
    }

    private static async Task ViewTeamDetails()
    {
        Console.WriteLine("\nVIEW TEAM DETAILS");
        var teamId = GetInput("Enter Team ID: ", required: true);

        var result = await _teamService.GetTeamByIdAsync(teamId);

        if (result.IsSuccess && result.Data.Teams.Any())
        {
            var team = result.Data.Teams[0].Team;
            Console.WriteLine("\nTeam Details:");
            Console.WriteLine($"ID: {team.TeamId}");
            Console.WriteLine($"Code: {team.TeamCode}");
            Console.WriteLine($"Name: {team.TeamName}");
        }
        else
        {
            ShowError(result.Message ?? "Team not found.");
        }
    }

    private static async Task UpdateTeam()
    {
        Console.WriteLine("\nUPDATE TEAM");
        var teamId = GetInput("Enter Team ID: ", required: true);

        var request = new TeamRequestModel
        {
            TeamCode = GetInput("Team Code (required): ", required: true),
            TeamName = GetInput("Team Name (required): ", required: true)
        };

        Console.WriteLine("\nUpdating team...");
        var result = await _teamService.UpdateTeamAsync(teamId, request);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully updated team!");
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private static async Task DeleteTeam()
    {
        Console.WriteLine("\nDELETE TEAM");
        var teamId = GetInput("Enter Team ID: ", required: true);

        Console.WriteLine("\nDeleting team...");
        var result = await _teamService.DeleteTeamAsync(teamId);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully deleted team!");
        }
        else
        {
            ShowError(result.Message);
        }
    }
    #endregion

    #region User Management
    private static async Task ManageUsers()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("USER MANAGEMENT");
            Console.WriteLine("1. Create New User");
            Console.WriteLine("2. View All Users");
            Console.WriteLine("3. View User Details");
            Console.WriteLine("4. Update User");
            Console.WriteLine("5. Delete User");
            Console.WriteLine("6. Back to Main Menu");
            Console.Write("Enter your choice (1-6): ");

            var choice = GetMenuChoice(1, 6);
            if (choice == 6) break;

            switch (choice)
            {
                case 1:
                    await CreateUser();
                    break;
                case 2:
                    await ListUsers();
                    break;
                case 3:
                    await ViewUserDetails();
                    break;
                case 4:
                    await UpdateUser();
                    break;
                case 5:
                    await DeleteUser();
                    break;
            }

            PressAnyKeyToContinue();
        }
    }

    private static async Task CreateUser()
    {
        Console.WriteLine("\nCREATE NEW USER");

        var request = new UserRequestModel
        {
            UserCode = GetInput("User Code (required): ", required: true),
            UserName = GetInput("User Name (required): ", required: true),
            GitHubAccountName = GetInput("GitHub Username: "),
            Nrc = GetInput("NRC Number: "),
            MobileNo = GetInput("Mobile Number: ")
        };

        Console.WriteLine("\nCreating user...");
        var result = await _userService.CreateUserAsync(request);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully created user!");
            Console.WriteLine($"User ID: {result.Data.Users[0].User.UserId}");
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private static async Task ListUsers()
    {
        Console.WriteLine("\nLIST ALL USERS");
        var result = await _userService.GetUsersAsync();

        if (result.IsSuccess && result.Data.Users.Any())
        {
            Console.WriteLine("\nUsers:");
            foreach (var user in result.Data.Users)
            {
                Console.WriteLine($"ID: {user.UserId}, Code: {user.UserCode}, Name: {user.UserName}");
            }
        }
        else
        {
            ShowError(result.Message ?? "No users found.");
        }
    }

    private static async Task ViewUserDetails()
    {
        Console.WriteLine("\nVIEW USER DETAILS");
        var userId = GetInput("Enter User ID: ", required: true);

        var result = await _userService.GetUserByIdAsync(userId);

        if (result.IsSuccess && result.Data.Users.Any())
        {
            var user = result.Data.Users[0].User;
            Console.WriteLine("\nUser Details:");
            Console.WriteLine($"ID: {user.UserId}");
            Console.WriteLine($"Code: {user.UserCode}");
            Console.WriteLine($"Name: {user.UserName}");
            Console.WriteLine($"GitHub: {user.GitHubAccountName}");
            Console.WriteLine($"NRC: {user.Nrc}");
            Console.WriteLine($"Mobile: {user.MobileNo}");
        }
        else
        {
            ShowError(result.Message ?? "User not found.");
        }
    }

    private static async Task UpdateUser()
    {
        Console.WriteLine("\nUPDATE USER");
        var userId = GetInput("Enter User ID: ", required: true);

        var request = new UserRequestModel
        {
            UserCode = GetInput("User Code (required): ", required: true),
            UserName = GetInput("User Name (required): ", required: true),
            GitHubAccountName = GetInput("GitHub Username: "),
            Nrc = GetInput("NRC Number: "),
            MobileNo = GetInput("Mobile Number: ")
        };

        Console.WriteLine("\nUpdating user...");
        var result = await _userService.UpdateUserAsync(userId, request);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully updated user!");
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private static async Task DeleteUser()
    {
        Console.WriteLine("\nDELETE USER");
        var userId = GetInput("Enter User ID: ", required: true);

        Console.WriteLine("\nDeleting user...");
        var result = await _userService.DeleteUserAsync(userId);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully deleted user!");
        }
        else
        {
            ShowError(result.Message);
        }
    }
    #endregion

    #region Activity Management
    private static async Task ManageActivities()
    {
        while (true)
        {
            Console.Clear();
            Console.WriteLine("ACTIVITY MANAGEMENT");
            Console.WriteLine("1. Log New Activity");
            Console.WriteLine("2. View Team Activities");
            Console.WriteLine("3. View Activity Details");
            Console.WriteLine("4. Update Activity");
            Console.WriteLine("5. Delete Activity");
            Console.WriteLine("6. Back to Main Menu");
            Console.Write("Enter your choice (1-6): ");

            var choice = GetMenuChoice(1, 6);
            if (choice == 6) break;

            switch (choice)
            {
                case 1:
                    await LogActivity();
                    break;
                case 2:
                    await ViewTeamActivities();
                    break;
                case 3:
                    await ViewActivityDetails();
                    break;
                case 4:
                    await UpdateActivity();
                    break;
                case 5:
                    await DeleteActivity();
                    break;
            }

            PressAnyKeyToContinue();
        }
    }

    private static async Task LogActivity()
    {
        Console.WriteLine("\nLOG NEW ACTIVITY");

        var activity = new TeamActivityModel
        {
            UserCode = GetInput("User Code (required): ", required: true),
            TeamCode = GetInput("Team Code (required): ", required: true),
            ProjectCode = GetInput("Project Code (required): ", required: true),
            TechStackCode = GetInput("Tech Stack Code: "),
            ActivityDate = DateTime.Today,
            Tasks = GetInput("Tasks Completed: ")
        };

        Console.WriteLine("\nLogging activity...");
        var result = await _teamActivityService.CreateActivityAsync(activity);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully logged activity!");
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private static async Task ViewTeamActivities()
    {
        Console.WriteLine("\nVIEW TEAM ACTIVITIES");
        var teamCode = GetInput("Enter Team Code: ", required: true);

        var result = await _teamActivityService.GetActivitiesByTeamCodeAsync(teamCode);

        if (result.IsSuccess && result.Data is not null)
        {
            Console.WriteLine("\nTeam Activities:");
            foreach (var activity in result.Data)
            {
                Console.WriteLine($"ID: {activity.ProjectTeamActivityId}, User: {activity.UserCode}, Project: {activity.ProjectCode}, Date: {activity.ActivityDate:yyyy-MM-dd}");
            }
        }
        else
        {
            ShowError(result.Message ?? "No activities found for this team.");
        }
    }

    private static async Task ViewActivityDetails()
    {
        Console.WriteLine("\nVIEW ACTIVITY DETAILS");
        var activityId = GetInput("Enter Activity ID: ", required: true);

        var result = await _teamActivityService.GetActivityByIdAsync(activityId);

        if (result.IsSuccess && result.Data is not null)
        {
            var activity = result.Data;
            Console.WriteLine("\nActivity Details:");
            Console.WriteLine($"ID: {activity.ProjectTeamActivityId}");
            Console.WriteLine($"User Code: {activity.UserCode}");
            Console.WriteLine($"Team Code: {activity.TeamCode}");
            Console.WriteLine($"Project Code: {activity.ProjectCode}");
            Console.WriteLine($"Tech Stack: {activity.TechStackCode}");
            Console.WriteLine($"Date: {activity.ActivityDate:yyyy-MM-dd}");
            Console.WriteLine($"Tasks: {activity.Tasks}");
        }
        else
        {
            ShowError(result.Message ?? "Activity not found.");
        }
    }

    private static async Task UpdateActivity()
    {
        Console.WriteLine("\nUPDATE ACTIVITY");
        var activityId = GetInput("Enter Activity ID: ", required: true);

        var activity = new TeamActivityModel
        {
            ProjectTeamActivityId = activityId,
            UserCode = GetInput("User Code (required): ", required: true),
            TeamCode = GetInput("Team Code (required): ", required: true),
            ProjectCode = GetInput("Project Code (required): ", required: true),
            TechStackCode = GetInput("Tech Stack Code: "),
            ActivityDate = DateTime.Today,
            Tasks = GetInput("Tasks Completed: ")
        };

        Console.WriteLine("\nUpdating activity...");
        var result = await _teamActivityService.UpdateActivityAsync(activity);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully updated activity!");
        }
        else
        {
            ShowError(result.Message);
        }
    }

    private static async Task DeleteActivity()
    {
        Console.WriteLine("\nDELETE ACTIVITY");
        var activityId = GetInput("Enter Activity ID: ", required: true);

        Console.WriteLine("\nDeleting activity...");
        var result = await _teamActivityService.DeleteActivityAsync(activityId);

        if (result.IsSuccess)
        {
            Console.WriteLine("Successfully deleted activity!");
        }
        else
        {
            ShowError(result.Message);
        }
    }
    #endregion

    #region Helper Methods
    private static int GetMenuChoice(int min, int max)
    {
        while (true)
        {
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max)
            {
                return choice;
            }
            Console.Write($"Invalid input. Please enter a number between {min} and {max}: ");
        }
    }

    private static string GetInput(string prompt, bool required = false)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();

        if (required && string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("This field is required!");
            return GetInput(prompt, required);
        }

        return input?.Trim();
    }

    private static DateTime? GetDateInput(string prompt)
    {
        Console.Write(prompt);
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            return null;

        if (DateTime.TryParse(input, out DateTime date))
            return date;

        Console.WriteLine("Invalid date format. Please try again.");
        return GetDateInput(prompt);
    }

    private static void PressAnyKeyToContinue()
    {
        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey();
    }

    private static void ShowError(string message)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {message}");
        Console.ResetColor();
    }
    #endregion
}