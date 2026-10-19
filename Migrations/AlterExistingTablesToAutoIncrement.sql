/*
  In-place migration for an existing WezDB database.
  SQL Server cannot ALTER an existing column to IDENTITY, so this script adds
  an identity column to each existing table, redirects foreign keys, removes
  the old ID column, and renames the new column to ID.

  No tables are created or replaced. Existing rows and relationships remain,
  but ID values are regenerated as 1, 2, 3, ... for each table.
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;

DECLARE @ExpectedTableCount int = 9;

IF (
    SELECT COUNT(*)
    FROM sys.tables AS t
    INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
    INNER JOIN sys.columns AS c ON c.object_id = t.object_id
    WHERE s.name = N'dbo'
      AND t.name IN (N'AssignedTasks', N'Member', N'Priority', N'ProjectRoles', N'Projects', N'Status', N'SystemRoles', N'Tasks', N'User')
      AND c.name = N'ID'
) <> @ExpectedTableCount
BEGIN
    ;THROW 50000, 'The expected WezDB tables were not found. No changes were applied.', 1;
END;

IF EXISTS (
    SELECT 1
    FROM sys.tables AS t
    INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
    INNER JOIN sys.columns AS c ON c.object_id = t.object_id
    WHERE s.name = N'dbo'
      AND t.name IN (N'AssignedTasks', N'Member', N'Priority', N'ProjectRoles', N'Projects', N'Status', N'SystemRoles', N'Tasks', N'User')
      AND c.name = N'ID'
      AND c.is_identity = 1
)
BEGIN
    IF EXISTS (
        SELECT 1
        FROM sys.tables AS t
        INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
        INNER JOIN sys.columns AS c ON c.object_id = t.object_id
        WHERE s.name = N'dbo'
          AND t.name IN (N'AssignedTasks', N'Member', N'Priority', N'ProjectRoles', N'Projects', N'Status', N'SystemRoles', N'Tasks', N'User')
          AND c.name = N'ID'
          AND c.is_identity = 0
    )
        ;THROW 50001, 'Some ID columns are already identity columns. The schema is in a mixed state, so no changes were applied.', 1;

    PRINT 'All ID columns are already IDENTITY columns. Nothing to do.';
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    -- Remove current foreign keys before changing the key columns.
    ALTER TABLE [dbo].[AssignedTasks] DROP CONSTRAINT [FK_AssignedTasks_Member];
    ALTER TABLE [dbo].[AssignedTasks] DROP CONSTRAINT [FK_AssignedTasks_Tasks];
    ALTER TABLE [dbo].[Member] DROP CONSTRAINT [FK_Member_ProjectRoles];
    ALTER TABLE [dbo].[Member] DROP CONSTRAINT [FK_Member_Projects];
    ALTER TABLE [dbo].[Member] DROP CONSTRAINT [FK_Member_User];
    ALTER TABLE [dbo].[Projects] DROP CONSTRAINT [FK_Projects_User];
    ALTER TABLE [dbo].[Tasks] DROP CONSTRAINT [FK_Tasks_Priority];
    ALTER TABLE [dbo].[Tasks] DROP CONSTRAINT [FK_Tasks_Projects];
    ALTER TABLE [dbo].[Tasks] DROP CONSTRAINT [FK_Tasks_Status];
    ALTER TABLE [dbo].[User] DROP CONSTRAINT [FK_User_SystemRoles];

    -- Add auto-increment columns to the existing tables.
    ALTER TABLE [dbo].[AssignedTasks] ADD [NewID] int IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[Member] ADD [NewID] int IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[Priority] ADD [NewID] int IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[ProjectRoles] ADD [NewID] int IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[Projects] ADD [NewID] int IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[Status] ADD [NewID] int IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[SystemRoles] ADD [NewID] int IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[Tasks] ADD [NewID] int IDENTITY(1,1) NOT NULL;
    ALTER TABLE [dbo].[User] ADD [NewID] int IDENTITY(1,1) NOT NULL;

    -- Point each relationship at the new identity values before old IDs are removed.
    UPDATE [dbo].[User]
    SET [Role] = [SystemRoles].[NewID]
    FROM [dbo].[User]
    INNER JOIN [dbo].[SystemRoles] ON [SystemRoles].[ID] = [User].[Role];

    UPDATE [dbo].[Projects]
    SET [CreatedBy] = [User].[NewID]
    FROM [dbo].[Projects]
    INNER JOIN [dbo].[User] ON [User].[ID] = [Projects].[CreatedBy];

    UPDATE [dbo].[Member]
    SET [ProjectID] = [Projects].[NewID]
    FROM [dbo].[Member]
    INNER JOIN [dbo].[Projects] ON [Projects].[ID] = [Member].[ProjectID];

    UPDATE [dbo].[Member]
    SET [UserID] = [User].[NewID]
    FROM [dbo].[Member]
    INNER JOIN [dbo].[User] ON [User].[ID] = [Member].[UserID];

    UPDATE [dbo].[Member]
    SET [Role] = [ProjectRoles].[NewID]
    FROM [dbo].[Member]
    INNER JOIN [dbo].[ProjectRoles] ON [ProjectRoles].[ID] = [Member].[Role];

    UPDATE [dbo].[Tasks]
    SET [Priority] = [Priority].[NewID]
    FROM [dbo].[Tasks]
    INNER JOIN [dbo].[Priority] ON [Priority].[ID] = [Tasks].[Priority];

    UPDATE [dbo].[Tasks]
    SET [Status] = [Status].[NewID]
    FROM [dbo].[Tasks]
    INNER JOIN [dbo].[Status] ON [Status].[ID] = [Tasks].[Status];

    UPDATE [dbo].[Tasks]
    SET [ProjectID] = [Projects].[NewID]
    FROM [dbo].[Tasks]
    INNER JOIN [dbo].[Projects] ON [Projects].[ID] = [Tasks].[ProjectID];

    UPDATE [dbo].[AssignedTasks]
    SET [TaskID] = [Tasks].[NewID]
    FROM [dbo].[AssignedTasks]
    INNER JOIN [dbo].[Tasks] ON [Tasks].[ID] = [AssignedTasks].[TaskID];

    UPDATE [dbo].[AssignedTasks]
    SET [MemberID] = [Member].[NewID]
    FROM [dbo].[AssignedTasks]
    INNER JOIN [dbo].[Member] ON [Member].[ID] = [AssignedTasks].[MemberID];

    -- Remove the old non-identity primary keys and columns.
    ALTER TABLE [dbo].[AssignedTasks] DROP CONSTRAINT [PK_AssignedTasks];
    ALTER TABLE [dbo].[Member] DROP CONSTRAINT [PK_Member];
    ALTER TABLE [dbo].[Priority] DROP CONSTRAINT [PK_Priority];
    ALTER TABLE [dbo].[ProjectRoles] DROP CONSTRAINT [PK_ProjectRoles];
    ALTER TABLE [dbo].[Projects] DROP CONSTRAINT [PK_Projects];
    ALTER TABLE [dbo].[Status] DROP CONSTRAINT [PK_Status];
    ALTER TABLE [dbo].[SystemRoles] DROP CONSTRAINT [PK_SystemRoles];
    ALTER TABLE [dbo].[Tasks] DROP CONSTRAINT [PK_Tasks];
    ALTER TABLE [dbo].[User] DROP CONSTRAINT [PK_User];

    ALTER TABLE [dbo].[AssignedTasks] DROP COLUMN [ID];
    ALTER TABLE [dbo].[Member] DROP COLUMN [ID];
    ALTER TABLE [dbo].[Priority] DROP COLUMN [ID];
    ALTER TABLE [dbo].[ProjectRoles] DROP COLUMN [ID];
    ALTER TABLE [dbo].[Projects] DROP COLUMN [ID];
    ALTER TABLE [dbo].[Status] DROP COLUMN [ID];
    ALTER TABLE [dbo].[SystemRoles] DROP COLUMN [ID];
    ALTER TABLE [dbo].[Tasks] DROP COLUMN [ID];
    ALTER TABLE [dbo].[User] DROP COLUMN [ID];

    EXEC sys.sp_rename N'dbo.AssignedTasks.NewID', N'ID', N'COLUMN';
    EXEC sys.sp_rename N'dbo.Member.NewID', N'ID', N'COLUMN';
    EXEC sys.sp_rename N'dbo.Priority.NewID', N'ID', N'COLUMN';
    EXEC sys.sp_rename N'dbo.ProjectRoles.NewID', N'ID', N'COLUMN';
    EXEC sys.sp_rename N'dbo.Projects.NewID', N'ID', N'COLUMN';
    EXEC sys.sp_rename N'dbo.Status.NewID', N'ID', N'COLUMN';
    EXEC sys.sp_rename N'dbo.SystemRoles.NewID', N'ID', N'COLUMN';
    EXEC sys.sp_rename N'dbo.Tasks.NewID', N'ID', N'COLUMN';
    EXEC sys.sp_rename N'dbo.[User].NewID', N'ID', N'COLUMN';

    -- Restore the original primary-key and foreign-key constraints.
    ALTER TABLE [dbo].[AssignedTasks] ADD CONSTRAINT [PK_AssignedTasks] PRIMARY KEY CLUSTERED ([ID]);
    ALTER TABLE [dbo].[Member] ADD CONSTRAINT [PK_Member] PRIMARY KEY CLUSTERED ([ID]);
    ALTER TABLE [dbo].[Priority] ADD CONSTRAINT [PK_Priority] PRIMARY KEY CLUSTERED ([ID]);
    ALTER TABLE [dbo].[ProjectRoles] ADD CONSTRAINT [PK_ProjectRoles] PRIMARY KEY CLUSTERED ([ID]);
    ALTER TABLE [dbo].[Projects] ADD CONSTRAINT [PK_Projects] PRIMARY KEY CLUSTERED ([ID]);
    ALTER TABLE [dbo].[Status] ADD CONSTRAINT [PK_Status] PRIMARY KEY CLUSTERED ([ID]);
    ALTER TABLE [dbo].[SystemRoles] ADD CONSTRAINT [PK_SystemRoles] PRIMARY KEY CLUSTERED ([ID]);
    ALTER TABLE [dbo].[Tasks] ADD CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED ([ID]);
    ALTER TABLE [dbo].[User] ADD CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED ([ID]);

    ALTER TABLE [dbo].[AssignedTasks] WITH CHECK ADD CONSTRAINT [FK_AssignedTasks_Member] FOREIGN KEY ([MemberID]) REFERENCES [dbo].[Member] ([ID]);
    ALTER TABLE [dbo].[AssignedTasks] WITH CHECK ADD CONSTRAINT [FK_AssignedTasks_Tasks] FOREIGN KEY ([TaskID]) REFERENCES [dbo].[Tasks] ([ID]);
    ALTER TABLE [dbo].[Member] WITH CHECK ADD CONSTRAINT [FK_Member_ProjectRoles] FOREIGN KEY ([Role]) REFERENCES [dbo].[ProjectRoles] ([ID]);
    ALTER TABLE [dbo].[Member] WITH CHECK ADD CONSTRAINT [FK_Member_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects] ([ID]);
    ALTER TABLE [dbo].[Member] WITH CHECK ADD CONSTRAINT [FK_Member_User] FOREIGN KEY ([UserID]) REFERENCES [dbo].[User] ([ID]);
    ALTER TABLE [dbo].[Projects] WITH CHECK ADD CONSTRAINT [FK_Projects_User] FOREIGN KEY ([CreatedBy]) REFERENCES [dbo].[User] ([ID]);
    ALTER TABLE [dbo].[Tasks] WITH CHECK ADD CONSTRAINT [FK_Tasks_Priority] FOREIGN KEY ([Priority]) REFERENCES [dbo].[Priority] ([ID]);
    ALTER TABLE [dbo].[Tasks] WITH CHECK ADD CONSTRAINT [FK_Tasks_Projects] FOREIGN KEY ([ProjectID]) REFERENCES [dbo].[Projects] ([ID]);
    ALTER TABLE [dbo].[Tasks] WITH CHECK ADD CONSTRAINT [FK_Tasks_Status] FOREIGN KEY ([Status]) REFERENCES [dbo].[Status] ([ID]);
    ALTER TABLE [dbo].[User] WITH CHECK ADD CONSTRAINT [FK_User_SystemRoles] FOREIGN KEY ([Role]) REFERENCES [dbo].[SystemRoles] ([ID]);

    COMMIT TRANSACTION;
    PRINT 'All existing table IDs are now IDENTITY(1,1).';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    ;THROW;
END CATCH;
