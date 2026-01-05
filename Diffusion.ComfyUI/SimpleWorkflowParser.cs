using System.Text.Json;

namespace Diffusion.ComfyUI;

public class SimpleWorkflowParser : IWorkflowParser
{
    public IReadOnlyCollection<Node> Parse(string workflowId, JsonElement rootElement)
    {
        var rootProperties = rootElement
            .EnumerateObject().ToDictionary(n => n.Name, n => n.Value);


        var nodes = new List<Node>();

        foreach (var element in rootProperties)
        {
            var node = new Node();
            node.Id = element.Key;

            foreach (var props in element.Value.EnumerateObject())
            {
                if (props.Name == "inputs")
                {
                    node.Inputs = new List<Input>();

                    foreach (var prop2 in props.Value.EnumerateObject())
                    {
                        var name = prop2.Name;

                        string path = $"[{node.Id}].inputs[\"{prop2.Name}\"]";

                        switch (prop2.Value.ValueKind)
                        {
                            case JsonValueKind.Undefined:
                                break;
                            case JsonValueKind.Object:
                                break;
                            case JsonValueKind.Array:
                                break;
                            case JsonValueKind.String:
                                node.Inputs.Add(new Input(workflowId, path, name, prop2.Value.GetString()));
                                break;
                            case JsonValueKind.Number:
                                node.Inputs.Add(new Input(workflowId, path, name, prop2.Value.GetDouble()));
                                break;
                            case JsonValueKind.True:
                                node.Inputs.Add(new Input(workflowId, path, name, prop2.Value.GetBoolean()));
                                break;
                            case JsonValueKind.False:
                                node.Inputs.Add(new Input(workflowId, path, name, prop2.Value.GetBoolean()));
                                break;
                            case JsonValueKind.Null:
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }
                else if (props.Name == "class_type")
                {
                    node.Name = props.Value.GetString();
                }
            }
            nodes.Add(node);
        }

        return nodes;
    }
}