﻿CREATE TABLE [levy].[LevyDeclaration]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
	[AccountId] BIGINT NOT NULL DEFAULT 0,
    [empRef] NVARCHAR(50) NOT NULL, 
    [LevyDueYTD] DECIMAL(18, 4) NULL DEFAULT 0, 
    [LevyAllowanceForYear] DECIMAL(18, 4) NULL DEFAULT 0, 
    [SubmissionDate] DATETIME NULL, 
    [SubmissionId] BIGINT NOT NULL DEFAULT 0,
	[PayrollYear] NVARCHAR(10) NULL,
	[PayrollMonth] TINYINT NULL
)
