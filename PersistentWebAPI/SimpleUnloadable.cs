namespace PersistentWebAPI
{
    public class SimpleUnloadable
    {
        public SimpleUnloadableAssemblyLoadContext Context { get; set; } = new SimpleUnloadableAssemblyLoadContext();
    }
}
