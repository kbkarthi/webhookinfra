using System.ComponentModel.DataAnnotations.Schema;

namespace SampleWebApiAspNetCore.Entities
{
    public class WebhookEntity
    {
        public int Id { get; set; }
        public string CallBackUrl { get; set; }
        public string WebhookType { get; set; }
        public string Pivot { get; set; }

        [NotMapped]
        public IDictionary<string, string> PivotParsed {  get; set; }

        [NotMapped]
        public string Key { get; set; }
        public DateTime Created { get; set; }
    }
}
