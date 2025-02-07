namespace ProyectoAPI.Models
{
    public class CohereResponseModel
    {
        public string Text { get; set; } // Respuesta del modelo
        public string ConversationId { get; set; } // ID de la conversación
    }
}