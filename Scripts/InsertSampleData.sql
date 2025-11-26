-- =============================================
-- OneMonthFlow Database Sample Data
-- MSSQL Server Script
-- =============================================

USE [OneMonthFlow]
GO

-- Clear existing data
DELETE FROM Tbl_ProjectTeamActivity;
DELETE FROM Tbl_ProjectTechStack;
DELETE FROM Tbl_ProjectTeam;
DELETE FROM Tbl_UserTechStack;
DELETE FROM Tbl_TeamTechStack;
DELETE FROM Tbl_TeamUser;
DELETE FROM Tbl_Project;
DELETE FROM Tbl_Team;
DELETE FROM Tbl_TechStack;
DELETE FROM Tbl_User;
GO

-- =============================================
-- Insert Sample Users
-- =============================================
INSERT INTO Tbl_User (UserId, UserCode, UserName, GitHubAccountName, Nrc, MobileNo, CreatedDate, CreatedBy)
VALUES
    (NEWID(), 'USR001', 'John Doe', 'johndoe', '12/ABC123456', '09123456789', GETDATE(), 'SYSTEM'),
    (NEWID(), 'USR002', 'Jane Smith', 'janesmith', '13/DEF789012', '09234567890', GETDATE(), 'SYSTEM'),
    (NEWID(), 'USR003', 'Bob Johnson', 'bobjohnson', '14/GHI345678', '09345678901', GETDATE(), 'SYSTEM'),
    (NEWID(), 'USR004', 'Alice Williams', 'alicewilliams', '15/JKL901234', '09456789012', GETDATE(), 'SYSTEM'),
    (NEWID(), 'USR005', 'Charlie Brown', 'charliebrown', '16/MNO567890', '09567890123', GETDATE(), 'SYSTEM');
GO

-- =============================================
-- Insert Sample Tech Stacks
-- =============================================
INSERT INTO Tbl_TechStack (TechStackId, TechStackCode, TechStackShortCode, TechStackName, CreatedDate, CreatedBy)
VALUES
    (NEWID(), 'TS001', 'C#', 'C# .NET', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TS002', 'JS', 'JavaScript', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TS003', 'PY', 'Python', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TS004', 'JAVA', 'Java', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TS005', 'REACT', 'React', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TS006', 'ANG', 'Angular', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TS007', 'SQL', 'SQL Server', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TS008', 'NODE', 'Node.js', GETDATE(), 'SYSTEM');
GO

-- =============================================
-- Insert Sample Teams
-- =============================================
INSERT INTO Tbl_Team (TeamId, TeamCode, TeamName, CreatedDate, CreatedBy)
VALUES
    (NEWID(), 'TEAM001', 'Frontend Team', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TEAM002', 'Backend Team', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TEAM003', 'Full Stack Team', GETDATE(), 'SYSTEM'),
    (NEWID(), 'TEAM004', 'DevOps Team', GETDATE(), 'SYSTEM');
GO

-- =============================================
-- Insert Sample Projects
-- =============================================
INSERT INTO Tbl_Project (ProjectId, ProjectCode, ProjectName, RepoUrl, StartDate, EndDate, ProjectDescription, Status)
VALUES
    (NEWID(), 'PROJ001', 'E-Commerce Platform', 'https://github.com/company/ecommerce', '2024-01-01', '2024-06-30', 'A modern e-commerce platform with payment integration', 'Active'),
    (NEWID(), 'PROJ002', 'Mobile App', 'https://github.com/company/mobileapp', '2024-02-01', '2024-08-31', 'Cross-platform mobile application', 'Active'),
    (NEWID(), 'PROJ003', 'Dashboard System', 'https://github.com/company/dashboard', '2024-03-01', '2024-09-30', 'Analytics and reporting dashboard', 'In Progress'),
    (NEWID(), 'PROJ004', 'API Gateway', 'https://github.com/company/apigateway', '2023-11-01', '2024-05-31', 'Microservices API gateway', 'Completed');
GO

-- =============================================
-- Insert Sample User Tech Stacks
-- =============================================
DECLARE @UserCode1 NVARCHAR(50) = 'USR001';
DECLARE @UserCode2 NVARCHAR(50) = 'USR002';
DECLARE @UserCode3 NVARCHAR(50) = 'USR003';
DECLARE @UserCode4 NVARCHAR(50) = 'USR004';
DECLARE @UserCode5 NVARCHAR(50) = 'USR005';

INSERT INTO Tbl_UserTechStack (UserTechStackId, UserCode, TechStackCode, ProficiencyLevel, CreatedDate, CreatedBy)
VALUES
    (NEWID(), @UserCode1, 'TS001', 8, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode1, 'TS007', 7, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode2, 'TS002', 9, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode2, 'TS005', 8, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode3, 'TS003', 7, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode3, 'TS008', 6, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode4, 'TS004', 8, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode4, 'TS007', 7, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode5, 'TS002', 9, GETDATE(), 'SYSTEM'),
    (NEWID(), @UserCode5, 'TS006', 8, GETDATE(), 'SYSTEM');
GO

-- =============================================
-- Insert Sample Team Users
-- =============================================
DECLARE @TeamCode1 NVARCHAR(50) = 'TEAM001';
DECLARE @TeamCode2 NVARCHAR(50) = 'TEAM002';
DECLARE @TeamCode3 NVARCHAR(50) = 'TEAM003';

INSERT INTO Tbl_TeamUser (TeamUserId, TeamCode, UserCode, UserRating)
VALUES
    (NEWID(), @TeamCode1, 'USR002', 4.5),
    (NEWID(), @TeamCode1, 'USR005', 4.8),
    (NEWID(), @TeamCode2, 'USR001', 4.7),
    (NEWID(), @TeamCode2, 'USR003', 4.3),
    (NEWID(), @TeamCode3, 'USR004', 4.6),
    (NEWID(), @TeamCode3, 'USR001', 4.5),
    (NEWID(), @TeamCode3, 'USR002', 4.4);
GO

-- =============================================
-- Insert Sample Team Tech Stacks
-- =============================================
INSERT INTO Tbl_TeamTechStack (TeamTechStackId, TeamCode, TechStackCode)
VALUES
    (NEWID(), 'TEAM001', 'TS002'),
    (NEWID(), 'TEAM001', 'TS005'),
    (NEWID(), 'TEAM001', 'TS006'),
    (NEWID(), 'TEAM002', 'TS001'),
    (NEWID(), 'TEAM002', 'TS003'),
    (NEWID(), 'TEAM002', 'TS004'),
    (NEWID(), 'TEAM002', 'TS007'),
    (NEWID(), 'TEAM003', 'TS001'),
    (NEWID(), 'TEAM003', 'TS002'),
    (NEWID(), 'TEAM003', 'TS005'),
    (NEWID(), 'TEAM003', 'TS007');
GO

-- =============================================
-- Insert Sample Project Teams
-- =============================================
INSERT INTO Tbl_ProjectTeam (ProjectTeamId, ProjectCode, TeamCode, ProjectTeamRating, Duration)
VALUES
    (NEWID(), 'PROJ001', 'TEAM003', 4.5, 180),
    (NEWID(), 'PROJ002', 'TEAM001', 4.7, 210),
    (NEWID(), 'PROJ002', 'TEAM002', 4.6, 210),
    (NEWID(), 'PROJ003', 'TEAM003', 4.4, 180),
    (NEWID(), 'PROJ004', 'TEAM002', 4.8, 210);
GO

-- =============================================
-- Insert Sample Project Tech Stacks
-- =============================================
INSERT INTO Tbl_ProjectTechStack (ProjectTechStackId, ProjectCode, TechStackCode)
VALUES
    (NEWID(), 'PROJ001', 'TS001'),
    (NEWID(), 'PROJ001', 'TS002'),
    (NEWID(), 'PROJ001', 'TS005'),
    (NEWID(), 'PROJ001', 'TS007'),
    (NEWID(), 'PROJ002', 'TS002'),
    (NEWID(), 'PROJ002', 'TS005'),
    (NEWID(), 'PROJ002', 'TS008'),
    (NEWID(), 'PROJ003', 'TS001'),
    (NEWID(), 'PROJ003', 'TS002'),
    (NEWID(), 'PROJ003', 'TS007'),
    (NEWID(), 'PROJ004', 'TS001'),
    (NEWID(), 'PROJ004', 'TS003'),
    (NEWID(), 'PROJ004', 'TS007');
GO

-- =============================================
-- Insert Sample Project Team Activities
-- =============================================
INSERT INTO Tbl_ProjectTeamActivity (ProjectTeamActivityId, ProjectCode, TeamCode, ActivityDate, Tasks, CreatedDate, CreatedBy)
VALUES
    (NEWID(), 'PROJ001', 'TEAM003', '2024-01-15', 'Implemented user authentication module', GETDATE(), 'SYSTEM'),
    (NEWID(), 'PROJ001', 'TEAM003', '2024-01-20', 'Created product catalog API endpoints', GETDATE(), 'SYSTEM'),
    (NEWID(), 'PROJ002', 'TEAM001', '2024-02-10', 'Developed mobile app UI components', GETDATE(), 'SYSTEM'),
    (NEWID(), 'PROJ002', 'TEAM002', '2024-02-12', 'Implemented backend API for mobile app', GETDATE(), 'SYSTEM'),
    (NEWID(), 'PROJ003', 'TEAM003', '2024-03-05', 'Created dashboard data visualization components', GETDATE(), 'SYSTEM'),
    (NEWID(), 'PROJ003', 'TEAM003', '2024-03-10', 'Implemented real-time data updates', GETDATE(), 'SYSTEM'),
    (NEWID(), 'PROJ004', 'TEAM002', '2024-01-20', 'Configured API gateway routing', GETDATE(), 'SYSTEM'),
    (NEWID(), 'PROJ004', 'TEAM002', '2024-02-15', 'Implemented rate limiting and authentication', GETDATE(), 'SYSTEM');
GO

PRINT 'Sample data inserted successfully!';
GO

-- =============================================
-- Verify Data
-- =============================================
SELECT 'Users' AS TableName, COUNT(*) AS RecordCount FROM Tbl_User
UNION ALL
SELECT 'Teams', COUNT(*) FROM Tbl_Team
UNION ALL
SELECT 'Tech Stacks', COUNT(*) FROM Tbl_TechStack
UNION ALL
SELECT 'Projects', COUNT(*) FROM Tbl_Project
UNION ALL
SELECT 'User Tech Stacks', COUNT(*) FROM Tbl_UserTechStack
UNION ALL
SELECT 'Team Users', COUNT(*) FROM Tbl_TeamUser
UNION ALL
SELECT 'Team Tech Stacks', COUNT(*) FROM Tbl_TeamTechStack
UNION ALL
SELECT 'Project Teams', COUNT(*) FROM Tbl_ProjectTeam
UNION ALL
SELECT 'Project Tech Stacks', COUNT(*) FROM Tbl_ProjectTechStack
UNION ALL
SELECT 'Project Team Activities', COUNT(*) FROM Tbl_ProjectTeamActivity;
GO

