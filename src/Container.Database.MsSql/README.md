# MsSql Container

Container to start a [Microsoft SQL Server container](https://hub.docker.com/_/microsoft-mssql-server). 

### Example code

For more detailed examples, see our [integration tests](../../test/Container.Database.MsSql.Integration.Tests).

```
/*
 * username is always `sa`
 * password will be used for the `sa` user
 *  - at least 8 characters in length
 *  - has at least 3 out of 4 categories of
 *    - has upper case alphabet
 *    - has lower case alphabet
 *    - has digit
 *    - has non-alphanumeric character
 * container does not allow automatic creation of an initial database
 * 
 * default image is mcr.microsoft.com/mssql/server:2017-latest-ubuntu 
 */
 
var container = new ContainerBuilder<MsSqlContainer>()
    .ConfigureDatabase("not-used", mySaPassword, "not-used")
    .Build();

// .. create your db or use init scripts to create db    

using (var connection = new SqlConnection(container.GetConnectionString(existingDatabaseName)))
{
    await connection.OpenAsync();

    using (var command = connection.CreateCommand())
    {
        command.CommandText = "SELECT 1";
        await command.ExecuteScalarAsync();
    }
}
```