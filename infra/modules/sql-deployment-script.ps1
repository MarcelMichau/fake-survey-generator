$sqlServerFqdn = "$env:DBSERVER"
$sqlDatabaseName = "$env:DBNAME"
$principalName = "$env:PRINCIPALNAME"
$id = "$env:ID"
$pipelineIdentityName = "$env:PIPELINEIDENTITYNAME"
$pipelineIdentityClientId = "$env:PIPELINEIDENTITYCLIENTID"

# Install SqlServer module - using specific version to avoid breaking changes in 22.4.5.1 (see https://github.com/dotnet/aspire/issues/9926)
Install-Module -Name SqlServer -RequiredVersion 22.3.0 -Force -AllowClobber -Scope CurrentUser
Import-Module SqlServer

$sqlCmd = @"
DECLARE @name SYSNAME = '$principalName';
DECLARE @id UNIQUEIDENTIFIER = '$id';
DECLARE @pipelineName SYSNAME = '$pipelineIdentityName';
DECLARE @pipelineId UNIQUEIDENTIFIER = '$pipelineIdentityClientId';

-- Convert the guid to the right type
DECLARE @castId NVARCHAR(MAX) = CONVERT(VARCHAR(MAX), CONVERT (VARBINARY(16), @id), 1);
DECLARE @pipelineCastId NVARCHAR(MAX) = CONVERT(VARCHAR(MAX), CONVERT (VARBINARY(16), @pipelineId), 1);

-- Construct command: CREATE USER [@name] WITH SID = @castId, TYPE = E;
DECLARE @cmd NVARCHAR(MAX) = N'CREATE USER [' + @name + '] WITH SID = ' + @castId + ', TYPE = E;'
EXEC (@cmd);

-- Assign roles to the new user
DECLARE @role1 NVARCHAR(MAX) = N'ALTER ROLE db_owner ADD MEMBER [' + @name + ']';
EXEC (@role1);

-- Create pipeline identity user and assign role
DECLARE @pipelineCmd NVARCHAR(MAX) = N'CREATE USER [' + @pipelineName + '] WITH SID = ' + @pipelineCastId + ', TYPE = E;'
EXEC (@pipelineCmd);

DECLARE @pipelineRole NVARCHAR(MAX) = N'ALTER ROLE db_owner ADD MEMBER [' + @pipelineName + ']';
EXEC (@pipelineRole);

"@
# Note: the string terminator must not have whitespace before it, therefore it is not indented.

Write-Host $sqlCmd

$connectionString = "Server=tcp:${sqlServerFqdn},1433;Initial Catalog=${sqlDatabaseName};Authentication=Active Directory Default;"

Invoke-Sqlcmd -ConnectionString $connectionString -Query $sqlCmd
