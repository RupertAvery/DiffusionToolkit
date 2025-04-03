using System.Text.Json;

namespace Diffusion.IO
{
    public class Node
    {
        public int RefId { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Input> Inputs { get; set; }
        public object ImageRef { get; set; }

        public override int GetHashCode()
        {
            var hash = Id.GetHashCode();

            if(Name != null)
            {
                hash = (hash * 397) ^ Name.GetHashCode();
            }

            if (Inputs != null)
            {
                foreach (var input in Inputs)
                {
                    hash = (hash * 397) ^ input.Name.GetHashCode();
                }
            }

            return hash;
        }
    }

    public class Input
    {
        public string WorkflowId { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

        public Input(string workflowId, string path, string name, object value)
        {
            WorkflowId = workflowId;
            Path = path;
            Name = name;
            Value = value;
        }
    }

    public class ComfyUIParser
    {
        public IReadOnlyCollection<Node>? Parse(string workflowId, string? workflow)
        {
            if (workflow == null) return null;
            var root = JsonDocument.Parse(workflow);
            var rootElements = root.RootElement.EnumerateObject().ToDictionary(n => n.Name, n => n.Value);

            var nodes = new List<Node>();

            foreach (var element in rootElements)
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
}
