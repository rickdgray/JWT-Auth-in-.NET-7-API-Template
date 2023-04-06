param(
	[string] $TargetMigration
)

# TODO print names of last 5 migrations to allow user to input

if ([string]::IsNullOrEmpty($TargetMigration)) {
	dotnet ef database update
}
else {
	dotnet ef database update $TargetMigration
}
