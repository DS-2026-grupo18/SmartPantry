using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SmartPantry.Products;

namespace SmartPantry.ExternalServices;

public class OpenFoodFactsProductCatalogClient : IExternalProductCatalogClient
{
    private readonly HttpClient _httpClient;

    public OpenFoodFactsProductCatalogClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ExternalProductDto?> GetByBarcodeAsync(string barcode)
    {
        HttpResponseMessage response;
        try
        {
            var url = $"api/v3/product/{barcode}.json?fields=product_name,brands,image_front_url,ingredients_text,nutriscore_grade";
            response = await _httpClient.GetAsync(url);
        }
        catch (HttpRequestException ex)
        {
            throw new ExternalCatalogUnavailableException("Error de conexión al consultar Open Food Facts.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new ExternalCatalogUnavailableException("Tiempo de espera agotado al consultar Open Food Facts.", ex);
        }

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            throw new ExternalCatalogRateLimitException();
        }

        if (!response.IsSuccessStatusCode)
        {
            throw new ExternalCatalogUnavailableException($"Open Food Facts respondió con código {(int)response.StatusCode}.");
        }

        OpenFoodFactsApiResponse? result;
        try
        {
            result = await response.Content.ReadFromJsonAsync<OpenFoodFactsApiResponse>();
        }
        catch (Exception ex)
        {
            throw new ExternalCatalogUnavailableException("Error al procesar la respuesta de Open Food Facts.", ex);
        }

        if (result == null || result.Product == null || result.Result?.Id == "product_not_found" || result.Status == "failure")
        {
            return null;
        }

        return new ExternalProductDto
        {
            Barcode = barcode,
            Name = string.IsNullOrWhiteSpace(result.Product.ProductName) ? null : result.Product.ProductName,
            Brand = string.IsNullOrWhiteSpace(result.Product.Brands) ? null : result.Product.Brands,
            ImageUrl = string.IsNullOrWhiteSpace(result.Product.ImageFrontUrl) ? null : result.Product.ImageFrontUrl,
            Ingredients = string.IsNullOrWhiteSpace(result.Product.IngredientsText) ? null : result.Product.IngredientsText,
            NutriScore = string.IsNullOrWhiteSpace(result.Product.NutriscoreGrade) ? null : result.Product.NutriscoreGrade.ToUpper()
        };
    }

    internal class OpenFoodFactsApiResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("result")]
        public OpenFoodFactsResultInfo? Result { get; set; }

        [JsonPropertyName("product")]
        public OpenFoodFactsProductData? Product { get; set; }
    }

    internal class OpenFoodFactsResultInfo
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }

    internal class OpenFoodFactsProductData
    {
        [JsonPropertyName("product_name")]
        public string? ProductName { get; set; }

        [JsonPropertyName("brands")]
        public string? Brands { get; set; }

        [JsonPropertyName("image_front_url")]
        public string? ImageFrontUrl { get; set; }

        [JsonPropertyName("ingredients_text")]
        public string? IngredientsText { get; set; }

        [JsonPropertyName("nutriscore_grade")]
        public string? NutriscoreGrade { get; set; }
    }
}
