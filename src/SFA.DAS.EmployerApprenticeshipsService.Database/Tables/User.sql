﻿CREATE TABLE [account].[User]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [PireanKey] UNIQUEIDENTIFIER NOT NULL, 
    [Email] NVARCHAR(255) NOT NULL, 
    [FirstName] NVARCHAR(MAX) NULL, 
    [LastName] NVARCHAR(MAX) NULL
)