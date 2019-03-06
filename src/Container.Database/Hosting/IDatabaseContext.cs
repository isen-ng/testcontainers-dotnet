namespace Container.Database.Hosting
{
    public interface IDatabaseContext
    {
        string DatabaseName { get; }
        
        string Username { get; }
        
        string Password { get; }
    }
}