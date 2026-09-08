namespace Diffusion.Database
{
    public partial class Migrations
    {

        [Migrate(MigrationType.Post)]
        private string RupertAvery20240818_0001_SetUnavailable()
        {
            return "UPDATE Image SET Unavailable = 0";
        }
    }
}
