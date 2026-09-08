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
        private string RupertAvery20240102_0001_LoadFileNamesFromPaths()
        {
            return "UPDATE Image SET FileName = path_basename(replace(Path, '\\','/'))";
        }
    }
}
