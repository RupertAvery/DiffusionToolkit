namespace Diffusion.Database;

public class MigrateAttribute : Attribute
{
    public string? Name { get; }

    public MigrateAttribute() { }

    public MigrateAttribute(string name)
    {
        Name = name;
    }
}