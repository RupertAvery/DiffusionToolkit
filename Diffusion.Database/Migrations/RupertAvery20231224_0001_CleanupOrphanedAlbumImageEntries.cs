using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Diffusion.Database
{
    public partial class Migrations
    {
        [Migrate]
        private string RupertAvery20231224_0001_CleanupOrphanedAlbumImageEntries()
        {
            return "DELETE FROM AlbumImage WHERE AlbumId IN (SELECT AlbumId FROM AlbumImage WHERE AlbumId NOT IN (SELECT Id FROM Album));" +
                   "DELETE FROM AlbumImage WHERE ImageId NOT IN(SELECT Id FROM Image);";
        }
    }
}
