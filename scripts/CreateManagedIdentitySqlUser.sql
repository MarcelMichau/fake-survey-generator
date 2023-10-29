DECLARE @managedIdentityName nvarchar(128) = '$(ManagedIdentityName)';

IF NOT EXISTS (SELECT [name] FROM [sys].[database_principals] WHERE [name] = @managedIdentityName)
CREATE USER [$(ManagedIdentityName)] FROM EXTERNAL PROVIDER;
GO
ALTER ROLE [db_datareader] ADD MEMBER [$(ManagedIdentityName)];
ALTER ROLE [db_datawriter] ADD MEMBER [$(ManagedIdentityName)];
GO