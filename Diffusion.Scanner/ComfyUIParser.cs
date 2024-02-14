using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Diffusion.IO
{
    public class ComfyUINode
    {
        public string Id { get; }
        public JsonElement Inputs { get; }
        public string ClassType { get; }

        public ComfyUINode(string id, JsonElement inputs, string classType)
        {
            Id = id;
            Inputs = inputs;
            ClassType = classType;
        }

        public Dictionary<string, ComfyUINode> InputNodes { get; set; }

    }

    public class ComfyUIParser
    {
        private Dictionary<string, ComfyUINode> _nodes = new Dictionary<string, ComfyUINode>();
        private List<ComfyUINode> _allNodes = new List<ComfyUINode>();
        private List<ComfyUINode> _rootNodes = new List<ComfyUINode>();
        private List<ComfyUINode> _leafNodes = new List<ComfyUINode>();

        public void LoadJson(string description)
        {
            var json = description.Substring("prompt: ".Length);

            // fix for errant nodes
            json = json.Replace("NaN", "null");

            var root = JsonDocument.Parse(json);
            var jsonNodes = root.RootElement.EnumerateObject().ToDictionary(o => o.Name, o => o.Value);

            foreach (var node in jsonNodes)
            {
                var id = node.Key;
                var inputs = node.Value.GetProperty("inputs");
                var classType = node.Value.GetProperty("class_type").GetString();

                var comfyUINode = new ComfyUINode(id, inputs, classType);

                _allNodes.Add(comfyUINode);
            }

            _nodes = _allNodes.ToDictionary(n => n.Id);

            BuildTree();

            Console.WriteLine($"{ComputeHash():x8}");
        }

        private void BuildTree()
        {
            var linkedNodes = new List<ComfyUINode>();

            foreach (var node in _nodes)
            {
                var links = new Dictionary<string, ComfyUINode>();

                foreach (var prop in node.Value.Inputs.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        var id = prop.Value.EnumerateArray().First().GetString();
                        linkedNodes.Add(_nodes[id]);
                        links.Add(prop.Name, _nodes[id]);
                    }
                }

                node.Value.InputNodes = links;
            }

            _rootNodes = _allNodes.Where(n => n.InputNodes.Count == 0).ToList();
            _leafNodes = _allNodes.Except(linkedNodes).Where(n => n.InputNodes.Count > 0).ToList();

        }


        private int ComputeHash()
        {
            int hashcode = 1430287;
            unchecked // Allow arithmetic overflow, numbers will just "wrap around"
            {
                foreach (var node in _leafNodes)
                {
                    hashcode = hashcode * 7302013 ^ ComputeHash("", node);
                }
            }
            return hashcode;
        }

        static int GetDeterministicHashCode(string str)
        {
            unchecked
            {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }


        private int ComputeHash(string key, ComfyUINode node)
        {
            int hashcode = 1430287;
            unchecked // Allow arithmetic overflow, numbers will just "wrap around"
            {
                hashcode = hashcode * 7302013 ^ GetDeterministicHashCode(key);
                hashcode = hashcode * 7302013 ^ GetDeterministicHashCode(node.ClassType);

                foreach (var inputNode in node.InputNodes.OrderBy(n => n.Key))
                {
                    hashcode = hashcode * 7302013 ^ ComputeHash(inputNode.Key, inputNode.Value);
                }

            }

            return hashcode;
        }

        public FileParameters GetParameters()
        {
            var fp = new FileParameters();
            return fp;
        }
    }
}
