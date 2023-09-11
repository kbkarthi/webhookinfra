using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Helpers;
using SampleWebApiAspNetCore.Models;
using System.Linq.Dynamic.Core;

namespace SampleWebApiAspNetCore.Repositories
{
    public class WebhookSqlRepository : IWebhookRepository
    {
        private readonly WebhookDbContext _webhookDbContext;

        public WebhookSqlRepository(WebhookDbContext webhookDbContext)
        {
            _webhookDbContext = webhookDbContext;
        }

        public WebhookEntity GetSingle(int id)
        {
            return _webhookDbContext.WebhookItems.FirstOrDefault(x => x.Id == id);
        }

        public void Add(WebhookEntity item)
        {
            _webhookDbContext.WebhookItems.Add(item);
        }

        public void Delete(int id)
        {
            WebhookEntity webhookItem = GetSingle(id);
            _webhookDbContext.WebhookItems.Remove(webhookItem);
        }

        public IQueryable<WebhookEntity> GetAll(QueryParameters queryParameters)
        {
            IQueryable<WebhookEntity> _allItems = _webhookDbContext.WebhookItems;

            return _allItems;
        }

        public int Count()
        {
            return _webhookDbContext.WebhookItems.Count();
        }

        public bool Save()
        {
            return (_webhookDbContext.SaveChanges() >= 0);
        }
    }
}
