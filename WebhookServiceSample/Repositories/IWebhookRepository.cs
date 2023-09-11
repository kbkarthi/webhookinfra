using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Models;

namespace SampleWebApiAspNetCore.Repositories
{
    public interface IWebhookRepository
    {
        WebhookEntity GetSingle(int id);
        void Add(WebhookEntity item);
        void Delete(int id);
        IQueryable<WebhookEntity> GetAll(QueryParameters queryParameters);
        int Count();
        bool Save();
    }
}
