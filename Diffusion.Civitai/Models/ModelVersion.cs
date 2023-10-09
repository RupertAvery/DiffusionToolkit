namespace Diffusion.Civitai.Models;

public class ModelVersion
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string DownloadUrl { get; set; }
    public List<string> TrainedWords { get; set; }
    public List<ModelFile> Files { get; set; }
    public List<ModelImage> Images { get; set; }
    public Stats Stats { get; set; }
    public string BaseModel { get; set; }

    // Additional properties
    //"trainingStatus": null,
    //"trainingDetails": null,
    //"baseModel": "SD 1.5",
    //"baseModelType": "Standard",
    //"earlyAccessTimeFrame": 0,
    //"description": null,
    //"vaeId": null,
}

/// <summary>
/// Used by model-versions api
/// </summary>
public class ModelVersion2 : ModelVersion
{
    public ModelVersionModel? Model { get; set; }
}


public class ModelVersionModel
{
    public string Name { get; set; }
    public ModelType Type { get; set; }
    public bool Nsfw { get; set; }
    public bool Poi { get; set; }
    public ModelMode? Mode { get; set; }
}
