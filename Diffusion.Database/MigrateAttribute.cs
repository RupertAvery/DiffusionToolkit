namespace Diffusion.Database;

public enum MigrationType
{
    /// <summary>
    /// Execute after schema changes
    /// </summary>
    Post,
    /// <summary>
    /// Execute before schema changes
    /// </summary>
    Pre
}

public class MigrateAttribute : Attribute
{
    public string? Name { get; }

    public MigrationType MigrationType { get; }

    public MigrateAttribute()
    {
        MigrationType = MigrationType.Post;
    }

    public MigrateAttribute(MigrationType migrationType)
    {
        MigrationType = migrationType;
    }

    public MigrateAttribute(string name, MigrationType migrationType)
    {
        Name = name;
        MigrationType = migrationType;
    }
}