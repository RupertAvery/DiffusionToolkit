using Diffusion.Common;
using Diffusion.Database.Models;
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
        private class FolderTemp
        {
            public int Id { get; set; }
            public int ParentId { get; set; }
            public string Path { get; set; }
            public bool IsRoot { get; set; }
            public string ParentPath { get; set; }
        }



        [Migrate(MigrationType.Post)]
        private string RupertAvery20250405_0001_FixFolders()
        {
            var config = File.ReadAllText(AppInfo.SettingsPath);

            var settings = JsonDocument.Parse(config);
            var rootElement = settings.RootElement;

            Logger.Log($"Migrating root paths");

            if (rootElement.TryGetProperty("ImagePaths", out var rootProperty))
            {
                var rootPaths = rootProperty.EnumerateArray().Select(d => d.GetString()).ToList();

                if (rootPaths.Any())
                {
                    var query =
                        "INSERT INTO Folder (ParentId, Path, Unavailable, Archived, IsRoot) VALUES (0, ?, 0, 0, 1) ON CONFLICT (Path) DO UPDATE SET IsRoot = 1 RETURNING Id";

                    foreach (var path in rootPaths)
                    {
                        _db.ExecuteScalar<int>(query, path);
                    }


                    Logger.Log($"{rootPaths.Count} root paths added from config.json");
                }

            }

            Logger.Log("Updating Folder ParentIds");

            var folders = _db.Query<FolderTemp>("SELECT Id, ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, IsRoot FROM Folder");

            foreach (var folder in folders)
            {
                folder.ParentPath = Path.GetDirectoryName(folder.Path);
            }

            foreach (var folder in folders)
            {
                var children = folders.Where(d => d.ParentPath == folder.Path);
                foreach (var child in children)
                {
                    child.ParentId = folder.Id;
                }
            }

            foreach (var folder in folders)
            {
                _db.Execute("UPDATE Folder SET ParentId = ?, IsRoot = ?, Unavailable = 0, Archived = 0 WHERE Id = ?", folder.ParentId, folder.IsRoot, folder.Id);
            }

            var orphanedFolders = _db.Query<FolderTemp>("SELECT Id, ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, IsRoot FROM Folder f WHERE ParentId = 0 AND IsRoot = 0");

            Logger.Log($"Found {orphanedFolders.Count} orphaned folders");

            foreach (var folder in orphanedFolders)
            {
                folder.ParentPath = Path.GetDirectoryName(folder.Path);
            }

            var parents = orphanedFolders.Select(d => d.ParentPath).Distinct();

            var folderCache = _db.Query<Folder>("SELECT Id, ParentId, Path, ImageCount, ScannedDate, Unavailable, Archived, IsRoot FROM Folder").ToDictionary(d => d.Path);

            Logger.Log($"Creating parent folders");

            foreach (var folder in parents)
            {
                Logger.Log($"Creating {folder}");

                try
                {
                    if (DataStore.EnsureFolderExistsExt(_db, folder, folderCache, out var folderId))
                    {
                        var children = orphanedFolders.Where(d => d.ParentPath == folder);

                        foreach (var child in children)
                        {
                            _db.Execute("UPDATE Folder SET ParentId = ? WHERE Id = ?", folderId, child.Id);
                        }
                    }
                    else
                    {
                        Logger.Log($"Root folder not found for {folder}");
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(e.Message);
                }

            }

            var cleanup = @"DELETE FROM Folder WHERE Id IN 
(
     SELECT f.Id FROM Folder f 
     LEFT JOIN Image i ON f.Id = i.FolderId
     WHERE ParentId = 0 AND IsRoot = 0
     GROUP BY f.Id
     HAVING COUNT(i.Id) = 0
)";

            var count = _db.Execute(cleanup);

            Logger.Log($"Removed {count} empty orphaned folders");

            Logger.Log($"Migrating excluded paths");

            _db.Execute("UPDATE Folder SET Excluded = 0");

            if (rootElement.TryGetProperty("ExcludePaths", out var excludeProperty))
            {
                var excludePaths = excludeProperty.EnumerateArray().Select(d => d.GetString()).ToList();

                foreach (var folder in excludePaths)
                {
                    Logger.Log($"Excluding {folder}");

                    try
                    {
                        if (DataStore.EnsureFolderExistsExt(_db, folder, folderCache, out var folderId))
                        {
                            Logger.Log($"Setting {folder} as Excluded");
                            _db.Execute("UPDATE Folder SET Excluded = 1 WHERE Id = ?", folderId);
                        }
                        else
                        {
                            Logger.Log($"Root folder not found for {folder}");
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log(e.Message);
                    }

                }

                Logger.Log($"{excludePaths.Count} paths excluded from config.json");

            }

            // Return empty statement so that nothing happens
            return ";";
        }
    }
}
