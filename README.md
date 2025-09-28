# AllTheBeans

- start and configure PostgreSQL
```docker compose -f .\deploy\local\docker-compose.yml up```

- configure User Secrets for AllTheBeans.API and set there the connection string to the db, e.g.;
```
{
  "ConnectionStrings": {
    "BeansDbConnectionString": "Server=127.0.0.1;Port=5432;Database=beans-db;User Id=<user>;Password=<password>;"
  }
}
```

- apply migrations
```
dotnet ef database update -p .\src\AllTheBeans.Infrastructure --connection '<connection string>'
```