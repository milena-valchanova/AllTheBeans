# AllTheBeans

- using docker
    - ensure you have docker running, i.e. for Windows machine can run [DockerDesktop](https://docs.docker.com/desktop/setup/install/windows-install/)
    - build latest version of the API
    ```docker build --no-cache -f .\src\AllTheBeans.API\Dockerfile -t allthebeansapi .```
    - configure PostgreSQL
        - open [docker-compose.yml](/deploy/local/docker-compose.yml)
        - replace '<userId>' with user name for your database, i.e. my-user
        - replace '<password>' with password for your database, i.e. my-super-secret-password
    - run PostgreSQL and the API
    ```docker compose -f .\deploy\local\docker-compose.yml up```
    - apply migrations, where '<connection string>' is the connection string to the database, i.e. 'Server=127.0.0.1;Port=5432;Database=beans-db;User Id=<userId>;Password=<password>;'
    ```dotnet ef database update -p .\src\AllTheBeans.Infrastructure --connection '<connection string>'```

- to run from VisualStudio without using docker
    - run an instance of PostgreSQL
    - apply migrations, where '<connection string>' is the connection string to the database, i.e. 'Server=127.0.0.1;Port=5432;Database=beans-db;User Id=<userId>;Password=<password>;'
    ```dotnet ef database update -p .\src\AllTheBeans.Infrastructure --connection '<connection string>'```
    - configure the connection string to the database:
        - if running in Debug mode - add User Secrets for AllTheBeans.API
        ```
        {
            "ConnectionStrings": {
                "BeansDbConnectionString": "Server=127.0.0.1;Port=5432;Database=beans-db;User Id=<userId>;Password=<password>;"
            }
        }
        ```
        - if running in Release mode - configure BeansDbConnectionString in appsettings.json by replacing '<userId>' and '<password>'

- available endpoints could be found in this [Postman collection](AllTheBeans.postman_collection.json)

- ensure docker is running in order to run Integration Tests