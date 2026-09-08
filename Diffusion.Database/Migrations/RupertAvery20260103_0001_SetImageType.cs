using Diffusion.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Diffusion.Database
{

    public partial class Migrations
    {
        [Migrate(MigrationType.Post)]
        private string RupertAvery20260103_0001_SetImageType()
        {
            Logger.Log($"Setting Image Type");

            return "UPDATE Image SET Type = 0";

        }
    }
}
