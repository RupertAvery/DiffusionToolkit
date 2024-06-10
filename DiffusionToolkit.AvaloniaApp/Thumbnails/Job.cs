using System;

namespace DiffusionToolkit.AvaloniaApp.Thumbnails;

public class Job<T, TOut>
{
    public T Data { get; set; }
    public Action<TOut> Completion { get; set; }
}