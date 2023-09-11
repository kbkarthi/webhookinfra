namespace SampleWebApiAspNetCore.Entities
{
    public class WebhookEntity
    {
        public int Id { get; set; }
        public string CallBackUrl { get; set; }
        public string WebhookType { get; set; }
        public DateTime Created { get; set; }
    }
}
