using System.Text.Json;
using System.Text.Json.Serialization;

namespace Diffusion.ComfyUI;

public interface INodePropertyCache
{
    Dictionary<string, List<string>> GetPropertiesLookup();
}

public class ComfyUIParser
{
    private INodePropertyCache _cache;

    public ComfyUIParser(INodePropertyCache cache)
    {
        _cache = cache;
    }

    public IReadOnlyCollection<Node>? Parse(string workflowId, string? workflow)
    {
        if (workflow == null) return null;
        var root = JsonDocument.Parse(workflow);

        JsonElement rootElement = root.RootElement;

        if (rootElement.TryGetProperty("prompt", out var tempElement))
        {
            rootElement = tempElement;
        }

        if (rootElement.ValueKind == JsonValueKind.String)
        {
            var tempRoot = JsonDocument.Parse(rootElement.GetString());
            rootElement = tempRoot.RootElement;
        }

        IWorkflowParser parser;

        if (rootElement.TryGetProperty("nodes", out var nodesProperty) && nodesProperty.ValueKind == JsonValueKind.Array)
        {
            parser = new AnimatedWebPWorkflowParser(_cache);
        }
        else
        {
            parser = new SimpleWorkflowParser();
        }


        return parser.Parse(workflowId, rootElement);
    }
}