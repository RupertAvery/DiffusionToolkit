using System.Text.Json;

namespace Diffusion.ComfyUI;

public interface IWorkflowParser
{
    public IReadOnlyCollection<Node> Parse(string workflowId, JsonElement rootElement);
}