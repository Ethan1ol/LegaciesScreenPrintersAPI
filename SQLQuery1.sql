-- Database schema, generated from the EF Core migrations (dotnet ef migrations script).
-- The API creates the database itself on startup, so this file is just for reference.

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
CREATE TABLE [materials] (
    [material_id] int NOT NULL IDENTITY,
    [material_name] nvarchar(100) NOT NULL,
    [color] nvarchar(50) NOT NULL,
    [unit_price] decimal(10,2) NOT NULL,
    [stock_quantity] int NOT NULL,
    [unit_measure] nvarchar(20) NOT NULL DEFAULT N'meters',
    [low_stock_threshold] int NOT NULL DEFAULT 15,
    [updated_at] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_materials] PRIMARY KEY ([material_id]),
    CONSTRAINT [CK_materials_stock] CHECK ([stock_quantity] >= 0)
);

CREATE TABLE [users] (
    [user_id] int NOT NULL IDENTITY,
    [full_name] nvarchar(100) NOT NULL,
    [email] nvarchar(120) NOT NULL,
    [phone_number] nvarchar(20) NULL,
    [password_hash] nvarchar(255) NULL,
    [google_sub] nvarchar(64) NULL,
    [role] nvarchar(20) NOT NULL DEFAULT N'customer',
    [created_at] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_users] PRIMARY KEY ([user_id]),
    CONSTRAINT [CK_users_role] CHECK ([role] IN ('customer', 'admin'))
);

CREATE TABLE [orders] (
    [order_id] int NOT NULL IDENTITY,
    [order_ref] nvarchar(20) NOT NULL,
    [user_id] int NOT NULL,
    [order_type] nvarchar(20) NOT NULL DEFAULT N'mobile',
    [total_amount] decimal(10,2) NOT NULL,
    [status] nvarchar(50) NOT NULL DEFAULT N'Order Received & Verified',
    [created_at] datetime2 NOT NULL DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_orders] PRIMARY KEY ([order_id]),
    CONSTRAINT [CK_orders_order_type] CHECK ([order_type] IN ('mobile', 'walk_in')),
    CONSTRAINT [CK_orders_status] CHECK ([status] IN ('Order Received & Verified', 'Vinyl Cutting & Screen Prep', 'Screen Printing', 'Ready for Collection', 'Completed')),
    CONSTRAINT [FK_orders_users_user_id] FOREIGN KEY ([user_id]) REFERENCES [users] ([user_id]) ON DELETE CASCADE
);

CREATE TABLE [order_items] (
    [item_id] int NOT NULL IDENTITY,
    [order_id] int NOT NULL,
    [design_name] nvarchar(100) NOT NULL,
    [artwork_file_url] nvarchar(255) NULL,
    [material_id] int NOT NULL,
    [quantity] int NOT NULL,
    [unit_price] decimal(10,2) NOT NULL,
    [subtotal] decimal(10,2) NOT NULL,
    CONSTRAINT [PK_order_items] PRIMARY KEY ([item_id]),
    CONSTRAINT [CK_order_items_quantity] CHECK ([quantity] > 0),
    CONSTRAINT [FK_order_items_materials_material_id] FOREIGN KEY ([material_id]) REFERENCES [materials] ([material_id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_order_items_orders_order_id] FOREIGN KEY ([order_id]) REFERENCES [orders] ([order_id]) ON DELETE CASCADE
);

CREATE INDEX [IX_order_items_material_id] ON [order_items] ([material_id]);

CREATE INDEX [IX_order_items_order_id] ON [order_items] ([order_id]);

CREATE UNIQUE INDEX [IX_orders_order_ref] ON [orders] ([order_ref]);

CREATE INDEX [IX_orders_user_id] ON [orders] ([user_id]);

CREATE UNIQUE INDEX [IX_users_email] ON [users] ([email]);

CREATE UNIQUE INDEX [IX_users_google_sub] ON [users] ([google_sub]) WHERE [google_sub] IS NOT NULL;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261115174246_InitialCreate', N'10.0.15');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [orders] DROP CONSTRAINT [CK_orders_order_type];

ALTER TABLE [orders] ADD CONSTRAINT [CK_orders_order_type] CHECK ([order_type] IN ('mobile', 'web', 'walk_in'));

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20261115190421_AddWebOrderType', N'10.0.15');

COMMIT;
GO

