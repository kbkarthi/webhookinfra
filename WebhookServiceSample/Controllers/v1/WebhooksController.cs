using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SampleWebApiAspNetCore.Dtos;
using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Models;
using SampleWebApiAspNetCore.Repositories;
using System.Net.Http.Headers;
using System.Text;
using WebhookServiceSample.Auth;
using WebhookServiceSample.Dtos;
using WebhookServiceSample.Repositories;
using WebhookServiceSample.Services;

namespace SampleWebApiAspNetCore.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly IWebhookRepository _webhookRepository;
        private readonly IMapper _mapper;
        private readonly IConfigurationRoot _config;
        private readonly DataAggregateService _dataAggregateService;
        private readonly string _key = "9ZzVbIm0bEeS63RWHWQ8irilQN022mmay9ac91qQzj1lAzFu5l9BKQ==";

        public WebhooksController(
            IWebhookRepository webhookRepository,
            IMapper mapper,
            IConfigurationRoot config,
            DataAggregateService dataAggregateService)
        {
            _webhookRepository = webhookRepository;
            _mapper = mapper;
            _config = config;
            _dataAggregateService = dataAggregateService;
        }

        [HttpGet]
        [Route("token", Name = nameof(GetToken))]
        public async Task<ActionResult> GetToken(ApiVersion version)
        {
            AadTokenManager tokenManager = AadTokenManager.GetInstance(_config);
            var token = await tokenManager.GetAadTokenAsync();

            return Ok(token);
        }

        [HttpGet(Name = nameof(GetAllWebhooks))]
        public ActionResult GetAllWebhooks(ApiVersion version, [FromQuery] QueryParameters queryParameters)
        {
            List<WebhookEntity> webhookItems = _webhookRepository.GetAll(queryParameters).ToList();

            // mapper doesn't support complex types, so hack to deserialize everytime
            webhookItems.ForEach(w => w.PivotParsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(w.Pivot) ?? new Dictionary<string, string>());

            var allItemCount = _webhookRepository.Count();

            return Ok(webhookItems);
        }

        [HttpGet]
        [Route("{id:int}", Name = nameof(GetSingleWebhook))]
        public ActionResult GetSingleWebhook(ApiVersion version, int id)
        {
            WebhookEntity webhookItem = _webhookRepository.GetSingle(id);

            if (webhookItem == null)
            {
                return NotFound();
            }

            WebhookDto item = _mapper.Map<WebhookDto>(webhookItem);

            // mapper doesn't support complex types, so hack to deserialize everytime
            item.PivotParsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(webhookItem.Pivot) ?? new Dictionary<string, string>();

            return Ok(item);
        }

        [HttpPost(Name = nameof(AddWebhook))]
        public ActionResult<WebhookDto> AddWebhook(ApiVersion version, [FromBody] WebhookCreateDto webhookCreateDto)
        {
            if (webhookCreateDto == null || _key != webhookCreateDto.Key)
            {
                return BadRequest();
            }

            WebhookEntity toAdd = _mapper.Map<WebhookEntity>(webhookCreateDto);

            if (!string.IsNullOrEmpty(webhookCreateDto.Pivot))
            {
                toAdd.PivotParsed = JsonConvert.DeserializeObject<Dictionary<string, string>>(webhookCreateDto.Pivot) ?? new Dictionary<string, string>();
            }

            _webhookRepository.Add(toAdd);

            if (!_webhookRepository.Save())
            {
                throw new Exception("Creating a webhookitem failed on save.");
            }

            WebhookEntity newWebhookItem = _webhookRepository.GetSingle(toAdd.Id);
            WebhookDto webhookDto = _mapper.Map<WebhookDto>(newWebhookItem);

            _dataAggregateService.Subscribe(toAdd);

            return CreatedAtRoute(nameof(GetSingleWebhook),
                new { version = version.ToString(), id = newWebhookItem.Id }, webhookDto);
        }

        [HttpDelete]
        [Route("{id:int}", Name = nameof(RemoveWebhook))]
        public ActionResult RemoveWebhook(int id)
        {
            WebhookEntity webhookItem = _webhookRepository.GetSingle(id);

            if (webhookItem == null)
            {
                return NotFound();
            }

            _webhookRepository.Delete(id);

            if (!_webhookRepository.Save())
            {
                throw new Exception("Deleting a webhookitem failed on save.");
            }

            _dataAggregateService.Unsubscribe(id);

            return NoContent();
        }

        [HttpPost]
        [Route("TriggerCallback", Name = nameof(TriggerWebhookCallback))]
        public async Task<ActionResult<WebhookDto>> TriggerWebhookCallback(ApiVersion version, [FromBody] WebhookCallbackDto webhookCallbackDto)
        {
            if (webhookCallbackDto == null)
            {
                return BadRequest();
            }

            WebhookEntity webhookItem = _webhookRepository.GetSingle(webhookCallbackDto.Id);
            if (webhookItem == null)
            {
                return NotFound($"Do not find webhook for {webhookCallbackDto.Id}");
            }

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(webhookItem.CallBackUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();

            HttpResponseMessage response = null;
            if (webhookCallbackDto.Method == HttpMethod.Post)
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var httpContent = new StringContent(JsonConvert.SerializeObject(webhookCallbackDto.Callbackdata), Encoding.UTF8, "application/json");
                response = await httpClient.PostAsync("", httpContent);
            }
            else if (webhookCallbackDto.Method == HttpMethod.Get)
            {
                response = await httpClient.GetAsync("");
            }

            if (response == null)
            {
                return Ok("response null, Callback not performed");
            }

            response.EnsureSuccessStatusCode();
            return Ok(response.Content);
        }

        [HttpGet]
        [Route("GetAPIData", Name = nameof(GetAPIData))]
        public async Task<ActionResult<SearchResultDTO>> GetAPIData(ApiVersion version)
        {
            var apiDataRepository = new ApiDataRepository(_config);
            var searchResult = await apiDataRepository.GetReportingDataAsync();

            return Ok(searchResult);
        }
    }
}