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
CREATE TABLE [Users] (
    [Id] uniqueidentifier NOT NULL,
    [Username] nvarchar(50) NOT NULL,
    [Email] nvarchar(255) NOT NULL,
    [PasswordHash] nvarchar(500) NOT NULL,
    [AvatarPath] nvarchar(500) NULL,
    [IsEmailConfirmed] bit NOT NULL,
    [EmailConfirmationToken] nvarchar(200) NULL,
    [EmailConfirmationTokenExpiresAt] datetime2 NULL,
    [IsDeleted] bit NOT NULL,
    [DeletedAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Comments] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [MediaId] int NOT NULL,
    [MediaType] nvarchar(20) NOT NULL,
    [Content] nvarchar(2000) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Comments_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Favorites] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [MediaId] int NOT NULL,
    [MediaType] nvarchar(20) NOT NULL,
    [Title] nvarchar(300) NOT NULL,
    [PosterPath] nvarchar(500) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Favorites] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Favorites_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE TABLE [Ratings] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] uniqueidentifier NOT NULL,
    [MediaId] int NOT NULL,
    [MediaType] nvarchar(20) NOT NULL,
    [Score] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_Ratings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Ratings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Comments_UserId] ON [Comments] ([UserId]);

CREATE UNIQUE INDEX [IX_Favorites_UserId_MediaId_MediaType] ON [Favorites] ([UserId], [MediaId], [MediaType]);

CREATE UNIQUE INDEX [IX_Ratings_UserId_MediaId_MediaType] ON [Ratings] ([UserId], [MediaId], [MediaType]);

CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);

CREATE UNIQUE INDEX [IX_Users_Username] ON [Users] ([Username]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260612185407_InitialCreate', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var nvarchar(max);
SELECT @var = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Ratings]') AND [c].[name] = N'Score');
IF @var IS NOT NULL EXEC(N'ALTER TABLE [Ratings] DROP CONSTRAINT ' + @var + ';');
ALTER TABLE [Ratings] ALTER COLUMN [Score] decimal(18,2) NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260614125040_ChangeRatingScoreToDecimal', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
DECLARE @var1 nvarchar(max);
SELECT @var1 = QUOTENAME([d].[name])
FROM [sys].[default_constraints] [d]
INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Users]') AND [c].[name] = N'IsDeleted');
IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [Users] DROP CONSTRAINT ' + @var1 + ';');
ALTER TABLE [Users] DROP COLUMN [IsDeleted];

ALTER TABLE [Users] ADD [AccountStatus] int NOT NULL DEFAULT 1;

ALTER TABLE [Users] ADD [Role] int NOT NULL DEFAULT 1;

ALTER TABLE [Users] ADD [WarningCount] smallint NOT NULL DEFAULT CAST(0 AS smallint);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260617115325_AddUserStatusRoleAndWarnings', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Comments] ADD [DeletedAt] datetime2 NULL;

ALTER TABLE [Comments] ADD [HiddenAt] datetime2 NULL;

ALTER TABLE [Comments] ADD [MediaPosterPath] nvarchar(500) NULL;

ALTER TABLE [Comments] ADD [MediaTitle] nvarchar(255) NOT NULL DEFAULT N'';

ALTER TABLE [Comments] ADD [ModeratedByUserId] uniqueidentifier NULL;

ALTER TABLE [Comments] ADD [Status] int NOT NULL DEFAULT 1;

CREATE TABLE [CommentReports] (
    [Id] uniqueidentifier NOT NULL,
    [CommentId] uniqueidentifier NOT NULL,
    [ReporterUserId] uniqueidentifier NOT NULL,
    [Reason] nvarchar(1000) NOT NULL,
    [Status] int NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL,
    [ReviewedAt] datetime2 NULL,
    [ReviewedByUserId] uniqueidentifier NULL,
    CONSTRAINT [PK_CommentReports] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_CommentReports_Comments_CommentId] FOREIGN KEY ([CommentId]) REFERENCES [Comments] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_CommentReports_Users_ReporterUserId] FOREIGN KEY ([ReporterUserId]) REFERENCES [Users] ([Id]) ON DELETE NO ACTION
);

CREATE INDEX [IX_Comments_MediaType_MediaId] ON [Comments] ([MediaType], [MediaId]);

CREATE INDEX [IX_Comments_Status] ON [Comments] ([Status]);

CREATE UNIQUE INDEX [IX_CommentReports_CommentId_ReporterUserId] ON [CommentReports] ([CommentId], [ReporterUserId]);

CREATE INDEX [IX_CommentReports_CreatedAt] ON [CommentReports] ([CreatedAt]);

CREATE INDEX [IX_CommentReports_ReporterUserId] ON [CommentReports] ([ReporterUserId]);

CREATE INDEX [IX_CommentReports_Status] ON [CommentReports] ([Status]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260617161035_UpdateCommentAndAddReporting', N'10.0.9');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260617172340_UpdateReportingBolean', N'10.0.9');

COMMIT;
GO

