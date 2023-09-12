using System.ComponentModel.DataAnnotations;

namespace SampleWebApiAspNetCore.Dtos
{
    public class WebhookCallbackDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public HttpMethod Method { get; set; }

        public string Callbackdata { get; set; }
    }
}
