using System;

namespace Diffusion.Toolkit.Classes;

public interface IErrorHandler
{
    void HandleError(Exception ex);
}