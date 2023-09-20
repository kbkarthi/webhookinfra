using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Models;

namespace SampleWebApiAspNetCore.Repositories
{
    public interface IWebhookRepository
    {
        WebhookEntity GetSingle(int id);
        NotificationEntity GetSingle(String id);
        void Add(WebhookEntity item);
        void Add(NotificationEntity item);
        void Delete(int id);
        IQueryable<WebhookEntity> GetAll(QueryParameters queryParameters);
        IQueryable<NotificationEntity> GetAll();
        int Count();
        bool Save();
    }
}
