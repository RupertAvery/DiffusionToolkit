using System.Collections.Generic;

namespace Diffusion.Common;

public class ComfyQueryOptions
{
    //public bool SearchAllProperties { get; set; }
    public IEnumerable<string>? SearchProperties { get; set; }
}