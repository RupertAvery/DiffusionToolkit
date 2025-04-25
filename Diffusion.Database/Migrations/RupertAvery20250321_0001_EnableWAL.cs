namespace Diffusion.Database
{
    public partial class Migrations
    {
        [Migrate(MigrationType.Pre, true)]
        private string RupertAvery20250321_0001_EnableWAL()
        {
            return "PRAGMA journal_mode=WAL";
        }
    }
}
