using System.Collections;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Diffusion.Civitai.Models;

namespace Diffusion.Civitai;

public class CivitaiClient : IDisposable
{
    private readonly string _baseUrl = "https://civitai.com/api/v1";

    private readonly HttpClient _httpClient;

    public string BaseUrl => _baseUrl;

    public CivitaiClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<Results<LiteModel>?> GetLiteModelsAsync(ModelSearchParameters searchParameters, CancellationToken token)
    {
        string queryString = GetQueryString(searchParameters);

        string apiUrl = $"{_baseUrl}/models{queryString}";

        return await GetResponseResults<Results<LiteModel>>(_httpClient, apiUrl, token);
    }

    public async Task<Results<LiteModel>?> GetLiteModels(string url, CancellationToken token)
    {
        return await GetResponseResults<Results<LiteModel>>(_httpClient, url, token);
    }

    public async Task<Results<Model>?> GetModels(string url, CancellationToken token)
    {
        return await GetResponseResults<Results<Model>>(_httpClient, url, token);
    }


    public async Task<Results<Model>?> GetModelsAsync(ModelSearchParameters searchParameters, CancellationToken token)
    {
        string queryString = GetQueryString(searchParameters);

        string apiUrl = $"{_baseUrl}/models{queryString}";

        return await GetResponseResults<Results<Model>>(_httpClient, apiUrl, token);
    }

    public async Task<ModelVersion2> GetModelVersionsByHashAsync(string hash, CancellationToken token)
    {
        string apiUrl = $"{_baseUrl}/model-versions/by-hash/{hash}";

        return await GetResponseResults<ModelVersion2>(_httpClient, apiUrl, token);
    }

    private async Task<T> GetResponseResults<T>(HttpClient client, string url, CancellationToken token) where T: class
    {
        T? results = null;
        try
        {
            var response = await client.GetAsync(url, token);

            if (response.IsSuccessStatusCode)
            {
                //var options = new JsonSerializerOptions
                //{
                //    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                //    Converters =
                //    {
                //        new JsonStringEnumConverter()
                //    }
                //};

                var options = new JsonSerializerOptions();
                options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                options.Converters.Add(new JsonStringEnumConverterWithAttributeSupport());

                using (var responseStream = await response.Content.ReadAsStreamAsync(token))
                {
                    //results = await JsonSerializer.DeserializeAsync<T>(responseStream, options);

                    using var buffer = new MemoryStream();
                    await responseStream.CopyToAsync(buffer);
                    responseStream.Flush();
                    responseStream.Close();

                    buffer.Position = 0;
                    using var fs = new FileStream($"civitai-{DateTime.Now:yyyyMMddhhmmss}.json", FileMode.Create, FileAccess.Write);
                    await buffer.CopyToAsync(fs);
                    fs.Flush();
                    fs.Close();

                    buffer.Position = 0;
                    results = await JsonSerializer.DeserializeAsync<T>(buffer, options);

                    buffer.Close();

                }
            }
            else
            {
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var document = JsonDocument.Parse(body);

                    string message = "Failed to retrieve results";

                    if (document.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        var arr = document.RootElement.EnumerateArray();
                        var err = arr.First();
                        string path = null;

                        if (err.TryGetProperty("message", out var messageElement))
                        {
                            message = messageElement.GetString();
                        }

                        if (err.TryGetProperty("path", out var pathElement))
                        {
                            path = string.Join("/", pathElement.EnumerateArray().Select(p => p.GetString()));
                        }

                        throw new CivitaiRequestException(message, path, body, response.StatusCode);
                    }
                    else
                    {
                        throw new CivitaiRequestException(message, body, response.StatusCode);
                    }


                }

                throw new CivitaiRequestException("Failed to retrieve results", response.StatusCode);
            }

        }
        catch (TaskCanceledException)
        {
        }

        return results;
    }

    static string GetQueryString<T>(T searchParameters)
    {
        var queryString = new StringBuilder("?");

        // Use reflection to get properties and values from ModelSearchParameters
        var properties = searchParameters.GetType().GetProperties();

        foreach (var property in properties)
        {
            var value = property.GetValue(searchParameters);

            if (value != null)
            {
                var propertyName = ToCamelCase(property.Name);

                if (value is IEnumerable listValue)
                {
                    var objectList = listValue.Cast<object>();

                    if (objectList.First() is Enum)
                    {
                        var enumList = listValue.Cast<Enum>().Select(enumValue => $"{propertyName}={EnumToString(enumValue)}");
                        queryString.Append($"{string.Join("&", enumList)}&");
                    }
                    else
                    {
                        var list = objectList.Select(value => $"{propertyName}={value}&");
                        queryString.Append($"{string.Join("&", list)}&");
                    }


                }
                else if (value is Enum enumValue)
                {
                    queryString.Append($"{propertyName}={EnumToString(enumValue)}&");
                }
                else
                {
                    queryString.Append($"{propertyName}={value}&");
                }
            }
        }

        // Remove the trailing "&" if there are any parameters
        if (queryString.Length > 1)
        {
            queryString.Length--; // Remove the last character
        }

        return queryString.ToString();
    }

    public static string ToCamelCase(string name)
    {
        return name[..1].ToLower() + name[1..];
    }

    static string EnumToString(Enum value)
    {
        switch (value)
        {
            case SortOrder sortOrder:
                return sortOrder switch
                {
                    SortOrder.HighestRated => "Highest Rated",
                    SortOrder.MostDownloaded => "Most Downloaded",
                    SortOrder.MostLiked => "Most Liked",
                    SortOrder.MostDiscussed => "Most Discussed",
                    SortOrder.MostCollected => "Most Collected",
                    SortOrder.Newest => "Newest",
                    _ => throw new ArgumentOutOfRangeException()
                };
            default:
                return value.ToString("G").Replace("_", " "); // Replace underscores with spaces
        }

    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }

}