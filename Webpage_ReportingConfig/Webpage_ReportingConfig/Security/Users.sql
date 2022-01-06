
-------------------------------------------------------------------
--- Create BIReporting_Configurator_user for 
---------------------------------------------------------------------

IF NOT EXISTS (
		SELECT name
		FROM master.sys.server_principals
		WHERE name = 'BIReporting_Configurator_user'
		)
BEGIN
	CREATE LOGIN [BIReporting_Configurator_user]
		WITH PASSWORD = N'password-dev', 
		SID = 0xAA1FA13502DFAA499521360180A63AA1,
		DEFAULT_DATABASE = [BI_Reporting], 
		DEFAULT_LANGUAGE = [us_english], 
		CHECK_EXPIRATION = OFF, 
		CHECK_POLICY = OFF
END
GO

IF NOT EXISTS (
		SELECT name
		FROM sys.database_principals
		WHERE name = 'BIReporting_Configurator_user'
		)
BEGIN
	CREATE USER [BIReporting_Configurator_user]
	FOR LOGIN [BIReporting_Configurator_user]
	WITH DEFAULT_SCHEMA = [dbo]
END

GO