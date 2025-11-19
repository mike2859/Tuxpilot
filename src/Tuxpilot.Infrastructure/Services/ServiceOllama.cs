using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Tuxpilot.Core.Interfaces.Services;

namespace Tuxpilot.Infrastructure.Services;


/// <summary>
/// Service d'assistant IA utilisant Ollama en local
/// </summary>
public class ServiceOllama : IServiceAssistantIA
{
    private readonly HttpClient _httpClient;
    private readonly string _modele = "mistral"; // Modèle par défaut
    private readonly string _urlOllama = "http://localhost:11434"; // Port par défaut d'Ollama
    
    public ServiceOllama()
    {
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5) // Timeout long pour les réponses LLM
        };
    }
    
    public async Task<string> DemanderAsync(string question)
    {
        try
        {
            var requestBody = new
            {
                model = _modele,
                prompt = $"Tu es un assistant Linux expert qui aide les utilisateurs francophones. Réponds de manière claire et concise.\n\nQuestion: {question}",
                stream = false
            };
            
            var response = await _httpClient.PostAsJsonAsync(
                $"{_urlOllama}/api/generate",
                requestBody
            );
            
            response.EnsureSuccessStatusCode();
            
            var result = await response.Content.ReadFromJsonAsync<OllamaResponse>();
            
            return result?.Response ?? "Désolé, je n'ai pas pu générer une réponse.";
        }
        catch (HttpRequestException ex)
        {
            return $"❌ Erreur de connexion à Ollama : {ex.Message}\n\nAssurez-vous qu'Ollama est démarré avec : ollama serve";
        }
        catch (Exception ex)
        {
            return $"❌ Erreur : {ex.Message}";
        }
    }
    
    public async Task DemanderAvecStreamingAsync(string question, Action<string> onTokenReceived)
    {
        try
        {
            var requestBody = new
            {
                model = _modele,
                prompt = $"Tu es un assistant Linux expert qui aide les utilisateurs francophones. Réponds de manière claire et concise.\n\nQuestion: {question}",
                stream = true
            };
            
            var content = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );
            
            var request = new HttpRequestMessage(HttpMethod.Post, $"{_urlOllama}/api/generate")
            {
                Content = content
            };
            
            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            using var stream = await response.Content.ReadAsStreamAsync();
            using var reader = new StreamReader(stream);
            
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line))
                    continue;
                
                try
                {
                    var json = JsonSerializer.Deserialize<OllamaStreamResponse>(line);
                    if (json?.Response != null)
                    {
                        onTokenReceived(json.Response);
                    }
                    
                    // Si c'est le dernier message
                    if (json?.Done == true)
                        break;
                }
                catch (JsonException)
                {
                    // Ignorer les lignes mal formées
                    continue;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            onTokenReceived($"\n\n❌ Erreur de connexion à Ollama : {ex.Message}\n\nAssurez-vous qu'Ollama est démarré avec : ollama serve");
        }
        catch (Exception ex)
        {
            onTokenReceived($"\n\n❌ Erreur : {ex.Message}");
        }
    }
    
    // Classes pour désérialiser les réponses Ollama
    private class OllamaResponse
    {
        public string? Response { get; set; }
    }
    
    private class OllamaStreamResponse
    {
        public string? Response { get; set; }
        public bool Done { get; set; }
    }
}