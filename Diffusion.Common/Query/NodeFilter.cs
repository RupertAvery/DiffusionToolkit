namespace Diffusion.Common.Query;

public class NodeFilter
{
    public bool IsActive { get; set; }
    public NodeOperation Operation { get; set; }
    public string Node { get; set; }
    public string Property { get; set; }
    public NodeComparison Comparison { get; set; }
    public string Value { get; set; }

}