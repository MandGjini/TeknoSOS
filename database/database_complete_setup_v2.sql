-- Complete database setup script for TeknoSOS
-- This script creates all necessary tables for the application

USE TeknoSOS;

-- ============================================================================
-- 1. CREATE ProfessionalSpecialties TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ProfessionalSpecialties')
BEGIN
    CREATE TABLE [dbo].[ProfessionalSpecialties] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [ProfessionalId] nvarchar(450) NOT NULL,
        [Category] int NOT NULL,
        [HourlyRate] numeric(10, 2) NOT NULL,
        [YearsOfExperience] int NULL,
        [IsVerified] bit NOT NULL DEFAULT 0,
        PRIMARY KEY ([Id]),
        FOREIGN KEY ([ProfessionalId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE
    );
    PRINT 'ProfessionalSpecialties table created.';
END

-- ============================================================================
-- 2. CREATE ServiceRequests TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'ServiceRequests')
BEGIN
    CREATE TABLE [dbo].[ServiceRequests] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [CitizenId] nvarchar(450) NOT NULL,
        [ProfessionalId] nvarchar(450) NULL,
        [Title] nvarchar(max) NOT NULL,
        [Description] nvarchar(max) NOT NULL,
        [Category] int NOT NULL,
        [Status] int NOT NULL DEFAULT 0,
        [Priority] int NOT NULL DEFAULT 0,
        [CasePriority] int NOT NULL DEFAULT 1,
        [CreatedDate] datetime2 NOT NULL,
        [ScheduledDate] datetime2 NULL,
        [CompletedDate] datetime2 NULL,
        [Location] nvarchar(max) NULL,
        [EstimatedCost] numeric(10, 2) NULL,
        [FinalCost] numeric(10, 2) NULL,
        [PhotoUrl] nvarchar(max) NULL,
        [ClientContactNumber] nvarchar(max) NULL,
        [ClientLatitude] numeric(10, 8) NULL,
        [ClientLongitude] numeric(10, 8) NULL,
        [NearestTechnicianDistance] numeric(18, 2) NULL,
        PRIMARY KEY ([Id]),
        FOREIGN KEY ([CitizenId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([ProfessionalId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_ServiceRequests_CitizenId] ON [dbo].[ServiceRequests] ([CitizenId]);
    CREATE INDEX [IX_ServiceRequests_ProfessionalId] ON [dbo].[ServiceRequests] ([ProfessionalId]);
    PRINT 'ServiceRequests table created.';
END

-- ============================================================================
-- 3. CREATE Reviews TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Reviews')
BEGIN
    CREATE TABLE [dbo].[Reviews] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [ServiceRequestId] int NOT NULL,
        [ReviewerId] nvarchar(450) NOT NULL,
        [RevieweeId] nvarchar(450) NOT NULL,
        [Rating] int NOT NULL,
        [Comment] nvarchar(max) NULL,
        [CreatedDate] datetime2 NOT NULL,
        PRIMARY KEY ([Id]),
        FOREIGN KEY ([ServiceRequestId]) REFERENCES [dbo].[ServiceRequests] ([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([ReviewerId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        FOREIGN KEY ([RevieweeId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        UNIQUE ([ServiceRequestId])
    );
    CREATE INDEX [IX_Reviews_ReviewerId] ON [dbo].[Reviews] ([ReviewerId]);
    CREATE INDEX [IX_Reviews_RevieweeId] ON [dbo].[Reviews] ([RevieweeId]);
    PRINT 'Reviews table created.';
END

-- ============================================================================
-- 4. CREATE Notifications TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Notifications')
BEGIN
    CREATE TABLE [dbo].[Notifications] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [RecipientId] nvarchar(450) NOT NULL,
        [ServiceRequestId] int NOT NULL,
        [Type] int NOT NULL,
        [Title] nvarchar(max) NOT NULL,
        [Message] nvarchar(max) NOT NULL,
        [IsRead] bit NOT NULL DEFAULT 0,
        [CreatedDate] datetime2 NOT NULL,
        [ReadDate] datetime2 NULL,
        [IsPushSent] bit NOT NULL DEFAULT 0,
        [IsEmailSent] bit NOT NULL DEFAULT 0,
        PRIMARY KEY ([Id]),
        FOREIGN KEY ([RecipientId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        FOREIGN KEY ([ServiceRequestId]) REFERENCES [dbo].[ServiceRequests] ([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_Notifications_RecipientId] ON [dbo].[Notifications] ([RecipientId]);
    CREATE INDEX [IX_Notifications_ServiceRequestId] ON [dbo].[Notifications] ([ServiceRequestId]);
    PRINT 'Notifications table created.';
END

-- ============================================================================
-- 5. CREATE Messages TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'Messages')
BEGIN
    CREATE TABLE [dbo].[Messages] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [SenderId] nvarchar(450) NOT NULL,
        [ReceiverId] nvarchar(450) NOT NULL,
        [ServiceRequestId] int NOT NULL,
        [Content] nvarchar(max) NOT NULL,
        [Status] int NOT NULL,
        [CreatedDate] datetime2 NOT NULL,
        [DeliveredDate] datetime2 NULL,
        [ReadDate] datetime2 NULL,
        [IsArchived] bit NOT NULL DEFAULT 0,
        PRIMARY KEY ([Id]),
        FOREIGN KEY ([SenderId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        FOREIGN KEY ([ReceiverId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE NO ACTION,
        FOREIGN KEY ([ServiceRequestId]) REFERENCES [dbo].[ServiceRequests] ([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_Messages_SenderId] ON [dbo].[Messages] ([SenderId]);
    CREATE INDEX [IX_Messages_ReceiverId] ON [dbo].[Messages] ([ReceiverId]);
    CREATE INDEX [IX_Messages_ServiceRequestId] ON [dbo].[Messages] ([ServiceRequestId]);
    PRINT 'Messages table created.';
END

-- ============================================================================
-- 6. CREATE TechnicianInterests TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'TechnicianInterests')
BEGIN
    CREATE TABLE [dbo].[TechnicianInterests] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [TechnicianId] nvarchar(450) NOT NULL,
        [ServiceRequestId] int NOT NULL,
        [Status] int NOT NULL,
        [PreventiveOffer] nvarchar(max) NULL,
        [EstimatedCost] numeric(10, 2) NULL,
        [EstimatedTimeInHours] int NULL,
        [CreatedDate] datetime2 NOT NULL,
        [ResponseDate] datetime2 NULL,
        PRIMARY KEY ([Id]),
        FOREIGN KEY ([TechnicianId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE CASCADE,
        FOREIGN KEY ([ServiceRequestId]) REFERENCES [dbo].[ServiceRequests] ([Id]) ON DELETE NO ACTION
    );
    CREATE INDEX [IX_TechnicianInterests_TechnicianId] ON [dbo].[TechnicianInterests] ([TechnicianId]);
    CREATE INDEX [IX_TechnicianInterests_ServiceRequestId] ON [dbo].[TechnicianInterests] ([ServiceRequestId]);
    PRINT 'TechnicianInterests table created.';
END

-- ============================================================================
-- 7. CREATE AuditLogs TABLE
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE [dbo].[AuditLogs] (
        [Id] int NOT NULL IDENTITY(1, 1),
        [UserId] nvarchar(450) NULL,
        [Action] nvarchar(max) NOT NULL,
        [Entity] nvarchar(max) NOT NULL,
        [EntityId] int NULL,
        [OldValue] nvarchar(max) NULL,
        [NewValue] nvarchar(max) NULL,
        [CreatedDate] datetime2 NOT NULL,
        [IpAddress] nvarchar(max) NULL,
        [UserAgent] nvarchar(max) NULL,
        PRIMARY KEY ([Id]),
        FOREIGN KEY ([UserId]) REFERENCES [dbo].[AspNetUsers] ([Id]) ON DELETE SET NULL
    );
    CREATE INDEX [IX_AuditLogs_UserId] ON [dbo].[AuditLogs] ([UserId]);
    PRINT 'AuditLogs table created.';
END

-- ============================================================================
-- 8. UPDATE AspNetUsers TABLE - Add Missing Columns
-- ============================================================================
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'FirstName')
    ALTER TABLE [dbo].[AspNetUsers] ADD [FirstName] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'LastName')
    ALTER TABLE [dbo].[AspNetUsers] ADD [LastName] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'Role')
    ALTER TABLE [dbo].[AspNetUsers] ADD [Role] int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'RegistrationDate')
    ALTER TABLE [dbo].[AspNetUsers] ADD [RegistrationDate] datetime2 NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'IsActive')
    ALTER TABLE [dbo].[AspNetUsers] ADD [IsActive] bit NOT NULL DEFAULT 1;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'ProfileImageUrl')
    ALTER TABLE [dbo].[AspNetUsers] ADD [ProfileImageUrl] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'Address')
    ALTER TABLE [dbo].[AspNetUsers] ADD [Address] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'City')
    ALTER TABLE [dbo].[AspNetUsers] ADD [City] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'PostalCode')
    ALTER TABLE [dbo].[AspNetUsers] ADD [PostalCode] nvarchar(max) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'PreferredLanguage')
    ALTER TABLE [dbo].[AspNetUsers] ADD [PreferredLanguage] int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'Latitude')
    ALTER TABLE [dbo].[AspNetUsers] ADD [Latitude] numeric(10, 8) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'Longitude')
    ALTER TABLE [dbo].[AspNetUsers] ADD [Longitude] numeric(10, 8) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'AverageRating')
    ALTER TABLE [dbo].[AspNetUsers] ADD [AverageRating] float NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'TotalReviews')
    ALTER TABLE [dbo].[AspNetUsers] ADD [TotalReviews] int NOT NULL DEFAULT 0;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'NotificationsEnabled')
    ALTER TABLE [dbo].[AspNetUsers] ADD [NotificationsEnabled] bit NOT NULL DEFAULT 1;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'EmailNotificationsEnabled')
    ALTER TABLE [dbo].[AspNetUsers] ADD [EmailNotificationsEnabled] bit NOT NULL DEFAULT 1;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'PushNotificationsEnabled')
    ALTER TABLE [dbo].[AspNetUsers] ADD [PushNotificationsEnabled] bit NOT NULL DEFAULT 1;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.AspNetUsers') AND name = 'LastLoginDate')
    ALTER TABLE [dbo].[AspNetUsers] ADD [LastLoginDate] datetime2 NULL;

PRINT 'AspNetUsers columns updated.';

PRINT '';
PRINT '========================================';
PRINT 'Database setup completed successfully!';
PRINT '========================================';
