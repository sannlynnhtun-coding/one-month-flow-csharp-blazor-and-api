-- =============================================
-- OneMonthFlow Database Schema
-- MSSQL Server Script
-- =============================================

USE [OneMonthFlow]
GO

-- Drop tables if they exist (in reverse order of dependencies)
IF OBJECT_ID('Tbl_ProjectTeamActivity', 'U') IS NOT NULL DROP TABLE Tbl_ProjectTeamActivity;
IF OBJECT_ID('Tbl_ProjectTechStack', 'U') IS NOT NULL DROP TABLE Tbl_ProjectTechStack;
IF OBJECT_ID('Tbl_ProjectTeam', 'U') IS NOT NULL DROP TABLE Tbl_ProjectTeam;
IF OBJECT_ID('Tbl_UserTechStack', 'U') IS NOT NULL DROP TABLE Tbl_UserTechStack;
IF OBJECT_ID('Tbl_TeamTechStack', 'U') IS NOT NULL DROP TABLE Tbl_TeamTechStack;
IF OBJECT_ID('Tbl_TeamUser', 'U') IS NOT NULL DROP TABLE Tbl_TeamUser;
IF OBJECT_ID('Tbl_Project', 'U') IS NOT NULL DROP TABLE Tbl_Project;
IF OBJECT_ID('Tbl_Team', 'U') IS NOT NULL DROP TABLE Tbl_Team;
IF OBJECT_ID('Tbl_TechStack', 'U') IS NOT NULL DROP TABLE Tbl_TechStack;
IF OBJECT_ID('Tbl_User', 'U') IS NOT NULL DROP TABLE Tbl_User;
GO

-- =============================================
-- Create Tables
-- =============================================

-- Tbl_User
CREATE TABLE Tbl_User (
    UserId NVARCHAR(50) PRIMARY KEY,
    UserCode NVARCHAR(50) NOT NULL UNIQUE,
    UserName NVARCHAR(200) NOT NULL,
    GitHubAccountName NVARCHAR(100) NULL,
    Nrc NVARCHAR(50) NULL,
    MobileNo NVARCHAR(20) NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL
);
GO

-- Tbl_Team
CREATE TABLE Tbl_Team (
    TeamId NVARCHAR(50) PRIMARY KEY,
    TeamCode NVARCHAR(50) NOT NULL UNIQUE,
    TeamName NVARCHAR(200) NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL
);
GO

-- Tbl_TechStack
CREATE TABLE Tbl_TechStack (
    TechStackId NVARCHAR(50) PRIMARY KEY,
    TechStackCode NVARCHAR(50) NOT NULL UNIQUE,
    TechStackShortCode NVARCHAR(20) NULL,
    TechStackName NVARCHAR(200) NOT NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL
);
GO

-- Tbl_Project
CREATE TABLE Tbl_Project (
    ProjectId NVARCHAR(50) PRIMARY KEY,
    ProjectCode NVARCHAR(50) NOT NULL UNIQUE,
    ProjectName NVARCHAR(200) NOT NULL,
    RepoUrl NVARCHAR(500) NULL,
    StartDate DATE NULL,
    EndDate DATE NULL,
    ProjectDescription NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NULL
);
GO

-- Tbl_UserTechStack
CREATE TABLE Tbl_UserTechStack (
    UserTechStackId NVARCHAR(50) PRIMARY KEY,
    UserCode NVARCHAR(50) NOT NULL,
    TechStackCode NVARCHAR(50) NOT NULL,
    ProficiencyLevel INT NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    ModifiedDate DATETIME2 NULL,
    ModifiedBy NVARCHAR(50) NULL,
    FOREIGN KEY (UserCode) REFERENCES Tbl_User(UserCode),
    FOREIGN KEY (TechStackCode) REFERENCES Tbl_TechStack(TechStackCode)
);
GO

-- Tbl_TeamUser
CREATE TABLE Tbl_TeamUser (
    TeamUserId NVARCHAR(50) PRIMARY KEY,
    TeamCode NVARCHAR(50) NOT NULL,
    UserCode NVARCHAR(50) NOT NULL,
    UserRating DECIMAL(5,2) NULL,
    FOREIGN KEY (TeamCode) REFERENCES Tbl_Team(TeamCode),
    FOREIGN KEY (UserCode) REFERENCES Tbl_User(UserCode)
);
GO

-- Tbl_TeamTechStack
CREATE TABLE Tbl_TeamTechStack (
    TeamTechStackId NVARCHAR(50) PRIMARY KEY,
    TeamCode NVARCHAR(50) NOT NULL,
    TechStackCode NVARCHAR(50) NOT NULL,
    FOREIGN KEY (TeamCode) REFERENCES Tbl_Team(TeamCode),
    FOREIGN KEY (TechStackCode) REFERENCES Tbl_TechStack(TechStackCode)
);
GO

-- Tbl_ProjectTeam
CREATE TABLE Tbl_ProjectTeam (
    ProjectTeamId NVARCHAR(50) PRIMARY KEY,
    ProjectCode NVARCHAR(50) NOT NULL,
    TeamCode NVARCHAR(50) NOT NULL,
    ProjectTeamRating DECIMAL(5,2) NULL,
    Duration INT NULL,
    FOREIGN KEY (ProjectCode) REFERENCES Tbl_Project(ProjectCode),
    FOREIGN KEY (TeamCode) REFERENCES Tbl_Team(TeamCode)
);
GO

-- Tbl_ProjectTechStack
CREATE TABLE Tbl_ProjectTechStack (
    ProjectTechStackId NVARCHAR(50) PRIMARY KEY,
    ProjectCode NVARCHAR(50) NOT NULL,
    TechStackCode NVARCHAR(50) NOT NULL,
    FOREIGN KEY (ProjectCode) REFERENCES Tbl_Project(ProjectCode),
    FOREIGN KEY (TechStackCode) REFERENCES Tbl_TechStack(TechStackCode)
);
GO

-- Tbl_ProjectTeamActivity
CREATE TABLE Tbl_ProjectTeamActivity (
    ProjectTeamActivityId NVARCHAR(50) PRIMARY KEY,
    ProjectCode NVARCHAR(50) NOT NULL,
    TeamCode NVARCHAR(50) NOT NULL,
    ActivityDate DATE NULL,
    Tasks NVARCHAR(MAX) NULL,
    CreatedDate DATETIME2 DEFAULT GETDATE(),
    CreatedBy NVARCHAR(50) NULL,
    FOREIGN KEY (ProjectCode) REFERENCES Tbl_Project(ProjectCode),
    FOREIGN KEY (TeamCode) REFERENCES Tbl_Team(TeamCode)
);
GO

-- =============================================
-- Create Indexes
-- =============================================

CREATE INDEX IX_Tbl_User_UserCode ON Tbl_User(UserCode);
CREATE INDEX IX_Tbl_Team_TeamCode ON Tbl_Team(TeamCode);
CREATE INDEX IX_Tbl_TechStack_TechStackCode ON Tbl_TechStack(TechStackCode);
CREATE INDEX IX_Tbl_Project_ProjectCode ON Tbl_Project(ProjectCode);
CREATE INDEX IX_Tbl_UserTechStack_UserCode ON Tbl_UserTechStack(UserCode);
CREATE INDEX IX_Tbl_UserTechStack_TechStackCode ON Tbl_UserTechStack(TechStackCode);
CREATE INDEX IX_Tbl_TeamUser_TeamCode ON Tbl_TeamUser(TeamCode);
CREATE INDEX IX_Tbl_TeamUser_UserCode ON Tbl_TeamUser(UserCode);
CREATE INDEX IX_Tbl_TeamTechStack_TeamCode ON Tbl_TeamTechStack(TeamCode);
CREATE INDEX IX_Tbl_ProjectTeam_ProjectCode ON Tbl_ProjectTeam(ProjectCode);
CREATE INDEX IX_Tbl_ProjectTeam_TeamCode ON Tbl_ProjectTeam(TeamCode);
CREATE INDEX IX_Tbl_ProjectTechStack_ProjectCode ON Tbl_ProjectTechStack(ProjectCode);
CREATE INDEX IX_Tbl_ProjectTeamActivity_ProjectCode ON Tbl_ProjectTeamActivity(ProjectCode);
CREATE INDEX IX_Tbl_ProjectTeamActivity_TeamCode ON Tbl_ProjectTeamActivity(TeamCode);
GO

PRINT 'Tables created successfully!';
GO

