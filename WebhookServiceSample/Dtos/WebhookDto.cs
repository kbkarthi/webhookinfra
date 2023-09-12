namespace SampleWebApiAspNetCore.Dtos
{
    public class WebhookDto
    {
        public int Id { get; set; }
        public string CallBackUrl { get; set; }
        public string WebhookType { get; set; }
        public string Pivot { get; set; }
        public IDictionary<string, string> PivotParsed { get; set; }
        public DateTime Created { get; set; }
    }
}
