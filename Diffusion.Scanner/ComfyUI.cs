using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Diffusion.IO
{
    public class Node
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Input> Inputs { get; set; }

        public override int GetHashCode()
        {
            var hash = Id.GetHashCode();

            hash = (hash * 397) ^ Name.GetHashCode();

            foreach (var input in Inputs)
            {
                hash = (hash * 397) ^ input.Name.GetHashCode();
            }
            
            return hash;
        }
    }

    public class Input
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public Input(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    public class ComfyUIParser
    {
        public IReadOnlyCollection<Node> Parse(string workflow)
        {
            if (workflow == null) return null;
            var root = System.Text.Json.JsonDocument.Parse(workflow);
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
                            switch (prop2.Value.ValueKind)
                            {
                                case JsonValueKind.Undefined:
                                    break;
                                case JsonValueKind.Object:
                                    break;
                                case JsonValueKind.Array:
                                    break;
                                case JsonValueKind.String:
                                    node.Inputs.Add(new Input(prop2.Name.Replace("_", "__"), prop2.Value.GetString()));
                                    break;
                                case JsonValueKind.Number:
                                    node.Inputs.Add(new Input(prop2.Name.Replace("_", "__"), prop2.Value.GetDouble()));
                                    break;
                                case JsonValueKind.True:
                                    node.Inputs.Add(new Input(prop2.Name.Replace("_", "__"), prop2.Value.GetBoolean()));
                                    break;
                                case JsonValueKind.False:
                                    node.Inputs.Add(new Input(prop2.Name.Replace("_", "__"), prop2.Value.GetBoolean()));
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
                        node.Name = props.Value.GetString().Replace("_", "__");
                    }
                }
                nodes.Add(node);
            }

            return nodes;
        }
    }
}
