using System.ComponentModel.DataAnnotations;
using WebhookServiceSample.Dtos;

namespace SampleWebApiAspNetCore.Dtos
{
    public class WebhookCallbackDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public HttpMethod Method { get; set; }

        public DeviceDTO[] Callbackdata { get; set; }
    }
}
