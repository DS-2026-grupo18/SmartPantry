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
        var response = await _httpClient.GetAsync($"api/v3/product/{barcode}.json");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<OpenFoodFactsApiResponse>();

        if (result == null || result.Status != 1 || result.Product == null)
        {
            return null;
        }

        return new ExternalProductDto
        {
            Barcode = barcode,
            Name = result.Product.ProductName,
            Brand = result.Product.Brands,
            ImageUrl = result.Product.ImageFrontUrl,
            Ingredients = result.Product.IngredientsText,
            NutriScore = result.Product.NutriscoreGrade?.ToUpper()
        };
    }

    internal class OpenFoodFactsApiResponse
    {
        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("product")]
        public OpenFoodFactsProductData? Product { get; set; }
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