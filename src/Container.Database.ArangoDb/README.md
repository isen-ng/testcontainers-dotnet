# ArangoDB Container

Container to start a [ArangoDb container](https://hub.docker.com/_/arangodb). 

### Example code

For more detailed examples, see our [integration tests](../../test/Container.Database.ArangoDb.Integration.Tests)

```
/*
 * username is always `root`
 * password will be used for the `root` user
 * container does not allow automatic creation of an initial database
 * 
 * default image is arangodb:3.4  
 */
 
var container = new ContainerBuilder<ArangoDbContainer>()
    .ConfigureDatabase("not-used", myRootPassword, "not-used")
    .Build();

// .. create your db or use init scripts to create db    

var settings = new DatabaseSharedSetting
{
    Url = arangoUrl,
    Database = existingDatabaseName,
    Credential = new NetworkCredential("root", myRootPassword)
};

using (var db = new ArangoDatabase(settings))
{
    return await Record.ExceptionAsync(async () =>
        await db.CreateStatement<int>("RETURN 1").ToListAsync());
}
```