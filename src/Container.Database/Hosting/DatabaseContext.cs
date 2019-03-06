namespace TestContainers.Container.Database.Hosting
{
    public class DatabaseContext : IDatabaseContext
    {
        public string DatabaseName { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }
    }
}