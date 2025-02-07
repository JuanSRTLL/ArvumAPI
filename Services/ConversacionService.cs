using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProyectoAPI.Models;

namespace ProyectoAPI.Services
{
    public class ConversacionService
    {
        private readonly HttpClient _httpClient;
        private const string ApiUrl = "https://api.cohere.ai/v1/chat";

        public ConversacionService(IConfiguration configuration)
        {
            _httpClient = new HttpClient();
            var apiKey = configuration["CohereApiKey"];
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
        }

        public async Task<CohereResponseModel> ManejarConversacionAsync(string mensaje, string conversationId)
        {
            if (string.IsNullOrWhiteSpace(mensaje))
            {
                throw new ArgumentException("El mensaje no puede estar vacío");
            }

            if (string.IsNullOrWhiteSpace(conversationId))
            {
                conversationId = Guid.NewGuid().ToString();
            }

            var request = new
            {
                model = "command-r-plus-08-2024",
                message = mensaje,
                max_tokens = 100,
                conversation_id = conversationId
            };

            var content = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(ApiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Error: {response.StatusCode}, Details: {errorResponse}");
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var cohereResponse = JsonConvert.DeserializeObject<CohereResponseModel>(responseString);
            cohereResponse.ConversationId = conversationId;
            return cohereResponse;
        }
    }
}