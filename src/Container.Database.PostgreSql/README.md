# MsSql Container

Container to start a [PostgreSQL Server container](https://hub.docker.com/_/postgres). 

### Example code

For more detailed examples, see our [integration tests](../../test/Container.Database.PostgreSql.Integration.Tests).

```
/*
 * default image is postgres:11-alpine 
 */
 
var container = new ContainerBuilder<GenericContainer>()
    .ConfigureDatabase(myUsername, mySaPassword, myDatabaseName)
    .Build();

using (var connection = new NpgsqlConnection(container.GetConnectionString()))
{
    await connection.OpenAsync();

    return await Record.ExceptionAsync(async () =>
    {
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync();
        }
    });
}
```