using System;

namespace Diffusion.Toolkit;

public class Job<T, TOut>
{
    public T Data { get; set; }
    public Action<TOut> Completion { get; set; }
}