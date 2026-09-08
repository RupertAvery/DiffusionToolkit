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
        private string RupertAvery20250424_0001_MigrateWatchedRecursive()
        {
            var config = File.ReadAllText(AppInfo.SettingsPath);

            var settings = JsonDocument.Parse(config);
            var rootElement = settings.RootElement;

            Logger.Log($"Migrating watched and recursive");

            if (rootElement.TryGetProperty("WatchFolders", out var watchedProperty))
            {
                var watched = watchedProperty.GetBoolean();

                _db.Execute("UPDATE Folder SET Watched = ? WHERE IsRoot = 1", watched);
            }

            if (rootElement.TryGetProperty("RecurseFolders", out var recursiveProperty))
            {
                var recursive = recursiveProperty.GetBoolean();

                _db.Execute("UPDATE Folder SET Recursive = ? WHERE IsRoot = 1", recursive);
            }

            return ";";
        }
    }
}
