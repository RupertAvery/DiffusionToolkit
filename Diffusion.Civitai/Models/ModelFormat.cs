using System.Runtime.Serialization;

namespace Diffusion.Civitai.Models;

public enum ModelFormat
{
    SafeTensor,
    PickleTensor,
    Diffusers,
    [EnumMember(Value = "Core ML")]
    CoreML,
    ONNX,
    Other,
}