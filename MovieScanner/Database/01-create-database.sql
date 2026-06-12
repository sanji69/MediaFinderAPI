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

