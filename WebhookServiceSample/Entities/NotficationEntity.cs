namespace SampleWebApiAspNetCore.Entities
{
    public class NotificationEntity
    {
        public int Id { get; set; }

        public string SubscriptionId { get; set; }

        public string ResourceId { get; set; }

        public string Resource { get; set; }

        public string Content { get; set; }
    }
}
