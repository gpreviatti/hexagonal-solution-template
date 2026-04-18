# EF Migration Commands

```bash
dotnet ef migrations add <MigrationName> --project src/Infrastructure --startup-project src/WebApp --output-dir Data/Migrations

dotnet ef database update --project src/Infrastructure --startup-project src/WebApp

dotnet ef migrations script --idempotent --project src/Infrastructure --startup-project src/WebApp --output scripts/sql/migrations.sql
```
