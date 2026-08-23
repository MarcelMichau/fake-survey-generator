$ErrorActionPreference = 'Stop'

$sqlServerFqdn = $env:DBSERVER
$sqlDatabaseName = $env:DBNAME
$principalName = $env:PRINCIPALNAME
$principalId = [Guid]$env:ID
$pipelineIdentityName = $env:PIPELINEIDENTITYNAME
$pipelineIdentityClientId = [Guid]$env:PIPELINEIDENTITYCLIENTID

$sqlAccessToken = Get-AzAccessToken -ResourceUrl 'https://database.windows.net/'
$tokenBstr = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlAccessToken.Token)

try {
    $accessToken = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($tokenBstr)
}
finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($tokenBstr)
}

$connection = [System.Data.SqlClient.SqlConnection]::new(
    "Server=tcp:${sqlServerFqdn},1433;Initial Catalog=${sqlDatabaseName};Encrypt=True;TrustServerCertificate=False;"
)
$connection.AccessToken = $accessToken

$command = $connection.CreateCommand()
$command.CommandText = @'
DECLARE @principalSid NVARCHAR(MAX) = CONVERT(VARCHAR(MAX), CONVERT(VARBINARY(16), @principalId), 1);
DECLARE @pipelineIdentitySid NVARCHAR(MAX) = CONVERT(VARCHAR(MAX), CONVERT(VARBINARY(16), @pipelineIdentityClientId), 1);
DECLARE @sql NVARCHAR(MAX);

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @principalName)
BEGIN
    SET @sql = N'CREATE USER ' + QUOTENAME(@principalName) + N' WITH SID = ' + @principalSid + N', TYPE = E;';
    EXEC sys.sp_executesql @sql;
END;

IF IS_ROLEMEMBER(N'db_owner', @principalName) <> 1
BEGIN
    SET @sql = N'ALTER ROLE [db_owner] ADD MEMBER ' + QUOTENAME(@principalName) + N';';
    EXEC sys.sp_executesql @sql;
END;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @pipelineIdentityName)
BEGIN
    SET @sql = N'CREATE USER ' + QUOTENAME(@pipelineIdentityName) + N' WITH SID = ' + @pipelineIdentitySid + N', TYPE = E;';
    EXEC sys.sp_executesql @sql;
END;

IF IS_ROLEMEMBER(N'db_owner', @pipelineIdentityName) <> 1
BEGIN
    SET @sql = N'ALTER ROLE [db_owner] ADD MEMBER ' + QUOTENAME(@pipelineIdentityName) + N';';
    EXEC sys.sp_executesql @sql;
END;
'@

$null = $command.Parameters.Add('@principalName', [System.Data.SqlDbType]::NVarChar, 128)
$command.Parameters['@principalName'].Value = $principalName
$null = $command.Parameters.Add('@principalId', [System.Data.SqlDbType]::UniqueIdentifier)
$command.Parameters['@principalId'].Value = $principalId
$null = $command.Parameters.Add('@pipelineIdentityName', [System.Data.SqlDbType]::NVarChar, 128)
$command.Parameters['@pipelineIdentityName'].Value = $pipelineIdentityName
$null = $command.Parameters.Add('@pipelineIdentityClientId', [System.Data.SqlDbType]::UniqueIdentifier)
$command.Parameters['@pipelineIdentityClientId'].Value = $pipelineIdentityClientId

try {
    $connection.Open()
    $null = $command.ExecuteNonQuery()
}
finally {
    $command.Dispose()
    $connection.Dispose()
}
