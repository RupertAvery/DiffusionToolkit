using System.Text.Json;

namespace Diffusion.ComfyUI;

public class NodePropertyCache : INodePropertyCache
{
    private Dictionary<string, List<string>> _cache;

    public void Load(string path)
    {
        var json = File.ReadAllText(path);
        _cache = JsonSerializer.Deserialize<Dictionary<string, List<string>>>(json);
    }

    public Dictionary<string, List<string>> GetPropertiesLookup()
    {
        return _cache;
    }
}