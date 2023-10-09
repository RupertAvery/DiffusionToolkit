namespace Diffusion.Civitai.Models;

public class ModelSearchParameters
{
    /// <summary>
    /// The number of results to be returned per page. This can be a number between 1 and 100. By default, each page will return 100 results
    /// </summary>
    public int? Limit { get; set; }
    /// <summary>
    /// The page from which to start fetching models
    /// </summary>
    public int? Page { get; set; }
    /// <summary>
    /// Search query to filter models by name
    /// </summary>
    public string Query { get; set; }
    /// <summary>
    /// Search query to filter models by tag
    /// </summary>
    public string Tag { get; set; }
    /// <summary>
    /// Search query to filter models by user
    /// </summary>
    public string Username { get; set; }
    /// <summary>
    /// The type of model you want to filter with. If none is specified, it will return all types
    /// </summary>
    public List<ModelType> Types { get; set; }
    /// <summary>
    /// The order in which you wish to sort the results
    /// </summary>
    public SortOrder? Sort { get; set; }
    /// <summary>
    /// The time frame in which the models will be sorted
    /// </summary>
    public TimePeriod? Period { get; set; }
    /// <summary>
    /// The rating you wish to filter the models with. If none is specified, it will return models with any rating
    /// </summary>
    public double? Rating { get; set; }
    /// <summary>
    /// Filter to favorites of the authenticated user (this requires an API token or session cookie)
    /// </summary>
    public bool? Favorites { get; set; }
    /// <summary>
    /// Filter to hidden models of the authenticated user (this requires an API token or session cookie)
    /// </summary>
    public bool? Hidden { get; set; }
    /// <summary>
    /// Only include the primary file for each model (This will use your preferred format options if you use an API token or session cookie)
    /// </summary>
    public bool? PrimaryFileOnly { get; set; }
    /// <summary>
    /// Filter to models that require or don't require crediting the creator
    /// </summary>
    public bool? AllowNoCredit { get; set; }
    /// <summary>
    /// Filter to models that allow or don't allow creating derivatives
    /// </summary>
    public bool? AllowDerivatives { get; set; }
    /// <summary>
    /// Filter to models that allow or don't allow derivatives to have a different license
    /// </summary>
    public bool? AllowDifferentLicenses { get; set; }
    /// <summary>
    /// Filter to models based on their commercial permissions
    /// </summary>
    public CommercialUse? AllowCommercialUse { get; set; }
    /// <summary>
    /// If false, will return safer images and hide models that don't have safe images
    /// </summary>
    public bool? Nsfw { get; set; }
}