namespace Diffusion.ComfyUI
{
    public class Node
    {
        private string _name;
        public int RefId { get; set; }
        public string Id { get; set; }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Label => _name.Replace("_", "__");

        public List<Input> Inputs { get; set; }
        public object ImageRef { get; set; }

        public override int GetHashCode()
        {
            var hash = Id.GetHashCode();

            if (Name != null)
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
}
