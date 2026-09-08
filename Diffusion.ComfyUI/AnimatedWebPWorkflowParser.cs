using System.Text.Json;

namespace Diffusion.ComfyUI;

public class AnimatedWebPWorkflowParser : IWorkflowParser
{
    private readonly Dictionary<string, List<string>> _propertiesLookup;

    public AnimatedWebPWorkflowParser(INodePropertyCache nodePropertyCache)
    {
        _propertiesLookup = nodePropertyCache.GetPropertiesLookup();
    }

    public IReadOnlyCollection<Node> Parse(string workflowId, JsonElement rootElement)
    {
        var nodes = rootElement.GetProperty("nodes").EnumerateArray();

        var result = new List<Node>();

        foreach (var node in nodes)
        {
            var id = node.GetProperty("id").GetInt32().ToString();
            var name = node.GetProperty("type").GetString();

            if (_propertiesLookup.TryGetValue(name, out var properties))
            {
                var values = node.GetProperty("widgets_values").EnumerateArray()
                    .Select(d =>
                    {
                        switch (d.ValueKind)
                        {
                            case JsonValueKind.String:
                                return (object)d.GetString();
                            case JsonValueKind.Number:
                                try
                                {
                                    return (object)d.GetInt32();
                                }
                                catch (Exception e)
                                {
                                    return (object)d.GetDouble();
                                }
                            default:
                                return (object)null;
                        }
                    });


                var inputs = properties.Zip(values)
                    .Where(d => d.Second != null)
                    .Select(d => new Input(workflowId, $"[{id}].inputs[\"{d.First}\"]", d.First, d.Second))
                    .ToList();

                var outputNode = new Node()
                {
                    Id = id,
                    Name = name,
                    Inputs = inputs,
                };

                result.Add(outputNode);

            }

        }
        return result;
    }
}