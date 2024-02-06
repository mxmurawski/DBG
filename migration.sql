CREATE DATABASE DBG;
GO

USE DBG;

IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF SCHEMA_ID(N'db_states') IS NULL EXEC(N'CREATE SCHEMA [db_states];');
GO

IF SCHEMA_ID(N'core') IS NULL EXEC(N'CREATE SCHEMA [core];');
GO

IF SCHEMA_ID(N'os_states') IS NULL EXEC(N'CREATE SCHEMA [os_states];');
GO

CREATE TABLE [db_states].[db_dynamic_states] (
    [Id] uniqueidentifier NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [DbSystemEntryId] uniqueidentifier NOT NULL,
    [ConnectionsCount] int NOT NULL,
    [DbAndTableSizesAsJson] nvarchar(max) NULL,
    CONSTRAINT [PK_db_dynamic_states] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [db_states].[db_static_states] (
    [Id] uniqueidentifier NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [DbSystemEntryId] uniqueidentifier NOT NULL,
    [Version] nvarchar(max) NULL,
    [MaxConnectionsCount] int NOT NULL,
    CONSTRAINT [PK_db_static_states] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [core].[db_system_entries] (
    [Id] uniqueidentifier NOT NULL,
    [DbType] int NOT NULL,
    [Address] nvarchar(max) NOT NULL,
    [Port] int NOT NULL,
    CONSTRAINT [PK_db_system_entries] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [os_states].[os_dynamic_states] (
    [Id] uniqueidentifier NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [OsSystemEntryId] uniqueidentifier NOT NULL,
    [CpuUsage] float NOT NULL,
    [RamUsage] float NOT NULL,
    [DiskUsageAsJson] nvarchar(max) NULL,
    CONSTRAINT [PK_os_dynamic_states] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [os_states].[os_static_states] (
    [Id] uniqueidentifier NOT NULL,
    [Timestamp] datetime2 NOT NULL,
    [OsSystemEntryId] uniqueidentifier NOT NULL,
    [Version] nvarchar(max) NULL,
    [CpuCount] int NOT NULL,
    [RamCount] int NOT NULL,
    [Uptime] nvarchar(max) NULL,
    CONSTRAINT [PK_os_static_states] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [core].[os_system_entries] (
    [Id] uniqueidentifier NOT NULL,
    [OsType] int NOT NULL,
    [Address] nvarchar(max) NOT NULL,
    [Port] int NOT NULL,
    CONSTRAINT [PK_os_system_entries] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [core].[users] (
    [Id] uniqueidentifier NOT NULL,
    [Email] nvarchar(450) NOT NULL,
    [FirstName] nvarchar(max) NULL,
    [LastName] nvarchar(max) NULL,
    [Password] nvarchar(max) NOT NULL,
    [Role] int NOT NULL,
    [RefreshToken] nvarchar(max) NULL,
    [RefreshTokenExpiryTime] datetime2 NULL,
    [CreatedOn] datetime2 NOT NULL,
    [UpdatedOn] datetime2 NULL,
    CONSTRAINT [PK_users] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [core].[system_entries] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(max) NULL,
    [DbEntryId] uniqueidentifier NOT NULL,
    [OsEntryId] uniqueidentifier NOT NULL,
    [CreatedOn] datetime2 NOT NULL,
    [UpdatedOn] datetime2 NULL,
    CONSTRAINT [PK_system_entries] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_system_entries_db_system_entries_DbEntryId] FOREIGN KEY ([DbEntryId]) REFERENCES [core].[db_system_entries] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_system_entries_os_system_entries_OsEntryId] FOREIGN KEY ([OsEntryId]) REFERENCES [core].[os_system_entries] ([Id]) ON DELETE CASCADE
);
GO

IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedOn', N'Email', N'FirstName', N'LastName', N'Password', N'RefreshToken', N'RefreshTokenExpiryTime', N'Role', N'UpdatedOn') AND [object_id] = OBJECT_ID(N'[core].[users]'))
    SET IDENTITY_INSERT [core].[users] ON;
INSERT INTO [core].[users] ([Id], [CreatedOn], [Email], [FirstName], [LastName], [Password], [RefreshToken], [RefreshTokenExpiryTime], [Role], [UpdatedOn])
VALUES ('05988d8d-4ce8-48b9-b524-713d1c0758c0', '0001-01-01T00:00:00.0000000', N'admin@admin.com', NULL, NULL, N'C7AD44CBAD762A5DA0A452F9E854FDC1E0E7A52A38015F23F3EAB1D80B931DD472634DFAC71CD34EBC35D16AB7FB8A90C81F975113D6C7538DC69DD8DE9077EC', NULL, NULL, 0, NULL),
('4b65c84c-933a-47e2-87bb-cc85642e3e1e', '0001-01-01T00:00:00.0000000', N'viewer@admin.com', NULL, NULL, N'A8D73E712D9257A75BCE54754E0AD3074894E29FEEEC1709F9E47B761DC38D7AB923A62F1B4883A19569115E8B68850CC86B27FDA81A0DAA5305538E4D910168', NULL, NULL, 1, NULL);
IF EXISTS (SELECT * FROM [sys].[identity_columns] WHERE [name] IN (N'Id', N'CreatedOn', N'Email', N'FirstName', N'LastName', N'Password', N'RefreshToken', N'RefreshTokenExpiryTime', N'Role', N'UpdatedOn') AND [object_id] = OBJECT_ID(N'[core].[users]'))
    SET IDENTITY_INSERT [core].[users] OFF;
GO

CREATE INDEX [IX_db_dynamic_states_DbSystemEntryId] ON [db_states].[db_dynamic_states] ([DbSystemEntryId]);
GO

CREATE INDEX [IX_db_static_states_DbSystemEntryId] ON [db_states].[db_static_states] ([DbSystemEntryId]);
GO

CREATE INDEX [IX_os_dynamic_states_OsSystemEntryId] ON [os_states].[os_dynamic_states] ([OsSystemEntryId]);
GO

CREATE INDEX [IX_os_static_states_OsSystemEntryId] ON [os_states].[os_static_states] ([OsSystemEntryId]);
GO

CREATE INDEX [IX_system_entries_DbEntryId] ON [core].[system_entries] ([DbEntryId]);
GO

CREATE INDEX [IX_system_entries_OsEntryId] ON [core].[system_entries] ([OsEntryId]);
GO

CREATE UNIQUE INDEX [IX_users_Email] ON [core].[users] ([Email]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20240104120716_InitialMigration', N'7.0.0');
GO

COMMIT;
GO

