namespace Diffusion.Civitai.Models
{
    public class Model
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ModelType Type { get; set; }
        public bool Nsfw { get; set; }
        public List<string> Tags { get; set; }
        public ModelMode? Mode { get; set; }
        public Creator Creator { get; set; }
        public Stats Stats { get; set; }
        public List<ModelVersion> ModelVersions { get; set; }
    }
}