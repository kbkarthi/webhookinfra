using AutoMapper;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SampleWebApiAspNetCore.Dtos;
using SampleWebApiAspNetCore.Entities;
using SampleWebApiAspNetCore.Helpers;
using SampleWebApiAspNetCore.Models;
using SampleWebApiAspNetCore.Repositories;
using System.Runtime.InteropServices;
using System.Text.Json;
using WebhookServiceSample.Auth;

namespace SampleWebApiAspNetCore.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class WebhooksController : ControllerBase
    {
        private readonly IWebhookRepository _webhookRepository;
        private readonly IMapper _mapper;

        public WebhooksController(
            IWebhookRepository webhookRepository,
            IMapper mapper)
        {
            _webhookRepository = webhookRepository;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("/token", Name = nameof(GetToken))]
        public async Task<ActionResult> GetToken(ApiVersion version)
        {
            AadTokenManager tokenManager = AadTokenManager.GetInstance();
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
            if (webhookCreateDto == null)
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

            return NoContent();
        }
    }
}
