namespace Diffusion.Database
{
    public partial class Migrations
    {
        [Migrate]
        private string RupertAvery20231224_0002_AlbumImageForeignKeys()
        {
            return @"DROP TABLE IF EXISTS ""AlbumImageTemp"";
CREATE TABLE IF NOT EXISTS ""AlbumImageTemp""(
    ""AlbumId""   integer,
    ""ImageId""   integer,
    CONSTRAINT ""FK_AlbumImage_AlbumId"" FOREIGN KEY(""AlbumId"") REFERENCES Album(""Id""),
    CONSTRAINT ""FK_AlbumImage_ImageId"" FOREIGN KEY(""ImageId"") REFERENCES Image(""Id"")
);
INSERT INTO AlbumImageTemp SELECT AlbumId, ImageId FROM AlbumImage;
DROP TABLE ""AlbumImage"";
ALTER TABLE ""AlbumImageTemp"" RENAME TO ""AlbumImage"";";
        }
    }
}
