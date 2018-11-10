IF NOT EXISTS (SELECT * FROM sys.assemblies WHERE name = 'System.Management')
BEGIN
	CREATE ASSEMBLY [System.Management]
	AUTHORIZATION [dbo]
	FROM 'C:\windows\Microsoft.NET\Framework64\v4.0.30319\System.Management.dll'
	WITH PERMISSION_SET = UNSAFE
END
