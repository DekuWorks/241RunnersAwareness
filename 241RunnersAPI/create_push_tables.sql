-- Create Devices table for push notifications
CREATE TABLE [Devices] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [Platform] nvarchar(10) NOT NULL,
    [FcmToken] nvarchar(500) NOT NULL,
    [AppVersion] nvarchar(20) NULL,
    [LastSeenAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [TopicsJson] nvarchar(2000) NULL,
    [IsActive] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    [DeviceModel] nvarchar(100) NULL,
    [OsVersion] nvarchar(50) NULL,
    [AppBuildNumber] nvarchar(100) NULL,
    CONSTRAINT [PK_Devices] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Devices_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

-- Create unique index for UserId + Platform
CREATE UNIQUE INDEX [IX_Devices_UserId_Platform] ON [Devices] ([UserId], [Platform]);

-- Create TopicSubscriptions table
CREATE TABLE [TopicSubscriptions] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [Topic] nvarchar(100) NOT NULL,
    [IsSubscribed] bit NOT NULL DEFAULT 1,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] datetime2 NULL,
    [SubscriptionReason] nvarchar(200) NULL,
    [LastNotificationSent] datetime2 NULL,
    [NotificationCount] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_TopicSubscriptions] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_TopicSubscriptions_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

-- Create unique index for UserId + Topic
CREATE UNIQUE INDEX [IX_TopicSubscriptions_UserId_Topic] ON [TopicSubscriptions] ([UserId], [Topic]);

-- Create Notifications table
CREATE TABLE [Notifications] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserId] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Body] nvarchar(1000) NOT NULL,
    [Type] nvarchar(50) NOT NULL,
    [Topic] nvarchar(100) NULL,
    [DataJson] nvarchar(2000) NULL,
    [IsSent] bit NOT NULL DEFAULT 0,
    [SentAt] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL DEFAULT GETUTCDATE(),
    [IsDelivered] bit NOT NULL DEFAULT 0,
    [DeliveredAt] datetime2 NULL,
    [IsOpened] bit NOT NULL DEFAULT 0,
    [OpenedAt] datetime2 NULL,
    [ErrorMessage] nvarchar(500) NULL,
    [RetryCount] int NOT NULL DEFAULT 0,
    [RelatedCaseId] int NULL,
    [RelatedUserId] int NULL,
    [Priority] nvarchar(100) NOT NULL DEFAULT 'normal',
    [ExpiresAt] datetime2 NULL,
    CONSTRAINT [PK_Notifications] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Notifications_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

-- Create indexes for Notifications
CREATE INDEX [IX_Notifications_UserId] ON [Notifications] ([UserId]);
CREATE INDEX [IX_Notifications_Type] ON [Notifications] ([Type]);
CREATE INDEX [IX_Notifications_Topic] ON [Notifications] ([Topic]);
CREATE INDEX [IX_Notifications_CreatedAt] ON [Notifications] ([CreatedAt]);
CREATE INDEX [IX_Notifications_IsSent] ON [Notifications] ([IsSent]);

-- Mark migrations as applied
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
VALUES ('20250913232725_AddPerformanceIndexes', '8.0.8');

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion]) 
VALUES ('20250919173347_AddPushNotificationsAndTopics', '8.0.8');
