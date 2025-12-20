---
description: 'Migrations generator and SQL script generation for EF Core in a multi-project .NET solution.'
tools: ['edit', 'search', 'new', 'runCommands', 'runTasks', 'microsoft-docs/*', 'usages', 'problems', 'todos']
---

Here's how to manage EF Core migrations in this solution:

### Generate Migration

To create a new migration:
```sh
dotnet-ef migrations add <MigrationName> --project src/Infrastructure --output-dir Data/Migrations --context MyDbContext
```

### Generate SQL Script

To generate idempotent SQL script:
```sh
dotnet-ef migrations script --project src/Infrastructure --context MyDbContext --idempotent --output scripts/migrations.sql
```

### Update Database

To apply migrations to the database:
```sh
dotnet-ef database update --project src/Infrastructure --startup-project src/WebApp --context MyDbContext
```

### Common Parameters:
- `<MigrationName>`: Name for the new migration
- `--project`: Points to Infrastructure project containing DbContext
- `--context`: Specifies MyDbContext 
- `--output-dir`: Places migrations in Data/Migrations folder
- `--idempotent`: Makes script rerunnable
- `--output`: Saves SQL to specified file

### Tips:
- Run from solution root directory
- Migration names should be descriptive (e.g. AddOrderTable)
- Review generated SQL before applying to production
- Keep migrations small and focused
- After run migrations, update ask user if he wants to update database