namespace Diffusion.ComfyUI;

public class Input
{
    public string WorkflowId { get; set; }
    public string Path { get; set; }
    public string Name { get; set; }
    public string Label { get; set; }
    public object Value { get; set; }

    public Input(string workflowId, string path, string name, object value)
    {
        WorkflowId = workflowId;
        Path = path;
        Name = name;
        Label = name.Replace("_", "__");
        Value = value;
    }
}