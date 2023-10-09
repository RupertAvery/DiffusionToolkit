using System;
using System.Collections;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Diffusion.Civitai.Models;

namespace Diffusion.Civitai;

public class CivitaiClient : IDisposable
{
    private readonly string _baseUrl = "https://civitai.com/api/v1";

    private readonly HttpClient _httpClient;

    public CivitaiClient()
    {
        _httpClient = new HttpClient();
    }

    public async Task<Results<LiteModel>?> GetLiteModelsAsync(ModelSearchParameters searchParameters, CancellationToken token)
    {
        string queryString = GetQueryString(searchParameters);

        string apiUrl = $"{_baseUrl}/models{queryString}";

        return await GetResponseResults<LiteModel>(_httpClient, apiUrl, token);
    }

    public async Task<Results<Model>?> GetModelsAsync(ModelSearchParameters searchParameters, CancellationToken token)
    {
        string queryString = GetQueryString(searchParameters);

        string apiUrl = $"{_baseUrl}/models{queryString}";

        return await GetResponseResults<Model>(_httpClient, apiUrl, token);
    }

    private async Task<Results<T>?> GetResponseResults<T>(HttpClient client, string url, CancellationToken token)
    {
        Results<T>? results = null;
        try
        {
            var response = await client.GetAsync(url, token);

            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    Converters =
                    {
                        new JsonStringEnumConverter()
                    }
                };

                using (var responseStream = await response.Content.ReadAsStreamAsync(token))
                {
                    results = await JsonSerializer.DeserializeAsync<Results<T>>(responseStream, options);
                }
            }
            else
            {
                if (response.Content.Headers.ContentType?.MediaType == "application/json")
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var document = JsonDocument.Parse(body);
                    string message = "Failed to retrieve results";
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
        return name.Substring(0, 1).ToLower() + name.Substring(1);
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