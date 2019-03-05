namespace TestContainers.Container.Abstractions.Models
{
    public class AccessMode
    {
        public static readonly AccessMode ReadOnly = new AccessMode("ro");
        public static readonly AccessMode ReadWrite = new AccessMode("rw");

        public string Value { get; }

        private AccessMode(string value)
        {
            Value = value;
        }
    }
}