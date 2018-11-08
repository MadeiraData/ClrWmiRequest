/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/
IF NOT EXISTS (SELECT * FROM sys.assemblies WHERE name = 'System.Management')
BEGIN
	CREATE ASSEMBLY [System.Management]
	AUTHORIZATION [dbo]
	FROM 'C:\windows\Microsoft.NET\Framework64\v4.0.30319\System.Management.dll'
	WITH PERMISSION_SET = UNSAFE
END
