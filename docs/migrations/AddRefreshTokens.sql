-- =========================================================================
-- Migration: AddRefreshTokens
-- Chạy script này để tạo bảng RefreshTokens trong DB SQL Server
-- Hoặc dùng: dotnet ef database update (sau khi dừng service đang chạy)
-- =========================================================================

IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RefreshTokens]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[RefreshTokens] (
        [Id]          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWSEQUENTIALID(),
        [UserId]      UNIQUEIDENTIFIER NOT NULL,
        [Token]       NVARCHAR(512)    NOT NULL,
        [ExpiresAt]   DATETIME2        NOT NULL,
        [IsRevoked]   BIT              NOT NULL DEFAULT 0,
        [DeviceInfo]  NVARCHAR(255)    NULL,
        [CreatedAt]   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
        [UpdatedAt]   DATETIME2        NULL,
        [CreatedBy]   NVARCHAR(100)    NULL,
        [UpdatedBy]   NVARCHAR(100)    NULL,

        CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_RefreshTokens_Users] FOREIGN KEY ([UserId])
            REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX [IX_RefreshTokens_Token] ON [dbo].[RefreshTokens] ([Token]);
    CREATE INDEX [IX_RefreshTokens_UserId] ON [dbo].[RefreshTokens] ([UserId]);
    CREATE INDEX [IX_RefreshTokens_ExpiresAt] ON [dbo].[RefreshTokens] ([ExpiresAt]);

    PRINT 'Table RefreshTokens created successfully.';
END
ELSE
BEGIN
    PRINT 'Table RefreshTokens already exists.';
END
GO
