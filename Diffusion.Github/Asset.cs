namespace Diffusion.Github;

public class Asset
{
    public string url { get; set; }
    public string browser_download_url { get; set; }
    public int id { get; set; }
    public string node_id { get; set; }
    public string name { get; set; }
    public string label { get; set; }
    public string state { get; set; }
    public string content_type { get; set; }
    public int size { get; set; }
    public int download_count { get; set; }
    public DateTime created_at { get; set; }
    public DateTime updated_at { get; set; }
    public Uploader uploader { get; set; }
}