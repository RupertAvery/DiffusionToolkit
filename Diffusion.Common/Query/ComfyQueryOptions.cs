using System.Collections.Generic;

namespace Diffusion.Common.Query;

public class ComfyQueryOptions
{
    //public bool SearchAllProperties { get; set; }
    public IEnumerable<string>? SearchProperties { get; set; }
}