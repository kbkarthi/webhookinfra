using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Net;
using System.Threading;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Repositories;
using SampleWebApiAspNetCore.Dtos;
using SampleWebApiAspNetCore.Models;
using WebhookServiceSample.Dtos;

namespace SampleWebApiAspNetCore.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly IWebhookRepository _webhookRepository;
        private readonly IConfigurationRoot _config;

        public NotificationsController(IWebhookRepository webhookRepository, IConfigurationRoot config)
        {
            _webhookRepository = webhookRepository;
            _config = config;
        }

        [HttpGet]
        public async Task<ActionResult<string>> Get()
        {
            var graphServiceClient = GetGraphClient();

            var sub = new Microsoft.Graph.Subscription();
            sub.ChangeType = "updated";
            sub.NotificationUrl = "https://kabaluwebhook.azurewebsites.net/api/v1/notifications";
            sub.Resource = "/users";
            sub.ExpirationDateTime = DateTime.UtcNow.AddMinutes(5);
            sub.ClientState = "SecretClientState";

            var newSubscription = await graphServiceClient
              .Subscriptions
              .Request()
              .AddAsync(sub);

            return $"Subscribed. Id: {newSubscription.Id}, Expiration: {newSubscription.ExpirationDateTime}";
        }

        [HttpPost]
        public async Task<ActionResult<string>> Post([FromQuery] string? validationToken = null)
        {
            // handle validation
            if (!string.IsNullOrEmpty(validationToken))
            {
                Console.WriteLine($"Received Token: '{validationToken}'");
                return Ok(validationToken);
            }

            // handle notifications
            using (StreamReader reader = new StreamReader(Request.Body))
            {
                string content = await reader.ReadToEndAsync();

                Console.WriteLine(content);

                var notifications = JsonSerializer.Deserialize<ChangeNotificationCollection>(content);

                if (notifications != null)
                {
                    foreach (var notification in notifications.Value)
                    {
                        Console.WriteLine($"Received notification: '{notification.Resource}', {notification.ResourceData.AdditionalData["id"]}");
                        NotificationEntity toAdd = new NotificationEntity();
                        toAdd.SubscriptionId = notification.SubscriptionId.ToString() ?? "";
                        toAdd.ResourceId = notification.ResourceData.AdditionalData["id"].ToString() ?? "";
                        toAdd.Resource = notification.Resource;
                        toAdd.Content = JsonSerializer.Serialize(notification); 
                        _webhookRepository.Add(toAdd);
                        _webhookRepository.Save();
                    }
                }
            }

            return Ok();
        }

        [HttpGet]
        [Route("GetAllNotifications", Name = nameof(GetAllNotifications))]
        public async Task<ActionResult<NotificationEntity>> GetAllNotifications(ApiVersion version)
        {
            List<NotificationEntity> notificationItems = _webhookRepository.GetAll().ToList();

            return Ok(notificationItems);
        }

        private GraphServiceClient GetGraphClient()
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                // get an access token for Graph
                var accessToken = GetAccessToken().Result;

                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));

            return graphClient;
        }

        private async Task<string> GetAccessToken()
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(_config["ClientId"])
              .WithClientSecret(_config["ClientSecret"])
              .WithAuthority($"https://login.microsoftonline.com/{_config["TenantId"]}")
              .WithRedirectUri("https://daemon")
              .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

    }
}