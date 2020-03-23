# MySql Container

Container to start a [MySql Server container](https://hub.docker.com/_/mysql).

Also compatible with [MariaDB](https://hub.docker.com/_/mariadb).

### Example code

For more detailed examples, see our [integration tests](../../test/Container.Database.MySql.Integration.Tests).

```
/*
 * default image is mysql:8
 */

var container = new ContainerBuilder<MySqlContainer>()
    .ConfigureDatabase(myUsername, mySaPassword, myDatabaseName)
    .Build();

using (var connection = new MySqlConnection(container.GetConnectionString()))
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
