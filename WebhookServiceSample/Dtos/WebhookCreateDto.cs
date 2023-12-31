using System.ComponentModel.DataAnnotations;

namespace SampleWebApiAspNetCore.Dtos
{
    public class WebhookCreateDto
    {
        [Required]
        public string CallBackUrl { get; set; }
        public string WebhookType { get; set; }
        public string Pivot { get; set; }
        public DateTime Created { get; set; }
        public string Key { get; set; }
    }
}
