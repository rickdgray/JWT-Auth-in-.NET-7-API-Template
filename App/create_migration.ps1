param(
	[Parameter(Mandatory=$true)]
	[string] $MigrationName
)

dotnet ef migrations add $MigrationName