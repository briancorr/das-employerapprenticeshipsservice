﻿/*
 Pre-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be executed before the build script.	
 Use SQLCMD syntax to include a file in the pre-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the pre-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/


--Update for PAYE

IF EXISTS(select 1 from INFORMATION_SCHEMA.TABLE_CONSTRAINTS where CONSTRAINT_NAME = 'FK_Paye_LegalEntity')
BEGIN
	alter table account.paye drop constraint FK_Paye_LegalEntity	
END
GO

IF EXISTS(select 1 from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'PAYE' and COLUMN_NAME = 'legalentityid')
BEGIN 
	alter table account.paye drop column legalentityid
END
GO

IF EXISTS(select 1  from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'User' and COLUMN_NAME='Email' and CHARACTER_MAXIMUM_LENGTH = 50)
BEGIN
	ALTER TABLE [account].[User]
	ALTER COLUMN Email NVARCHAR(255) NOT NULL
END

IF EXISTS(select 1  from INFORMATION_SCHEMA.COLUMNS where TABLE_NAME = 'Invitation' and COLUMN_NAME='Email' and CHARACTER_MAXIMUM_LENGTH = 100)
BEGIN
	ALTER TABLE [account].[Invitation]
	ALTER COLUMN Email NVARCHAR(255) NOT NULL
END