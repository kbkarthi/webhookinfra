namespace WebhookServiceSample.Services
{
    using Newtonsoft.Json;
    using SampleWebApiAspNetCore.Entities;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Net.Http.Headers;
    using System.Text;
    using WebhookServiceSample.Dtos;
    using WebhookServiceSample.Repositories;

    public class DataAggregateService
    {
        private ApiDataRepository _dataRepository;
        private ConcurrentDictionary<int, ISet<string>> _devicesPerHook = new ConcurrentDictionary<int, ISet<string>>();
        private IDictionary<int, WebhookEntity> _webhookRepository = new Dictionary<int, WebhookEntity>();

        private Task _poller;

        public DataAggregateService(ApiDataRepository dataRepository)
        {
            _dataRepository = dataRepository;

            _poller = Task.Run(PullAndNotify);
        }

        public void Subscribe(WebhookEntity wh)
        {
            if (!_devicesPerHook.ContainsKey(wh.Id))
            {
                _devicesPerHook.TryAdd(wh.Id, new HashSet<string>());
                _webhookRepository[wh.Id] = wh;

                // retrigger if not running
                if (_poller.IsCompleted)
                {
                    _poller = Task.Run(PullAndNotify);
                }
            }
        }

        public void Unsubscribe(int id)
        {
            if (_devicesPerHook.ContainsKey(id))
            {
                _devicesPerHook.TryRemove(id, out _);
                _webhookRepository.Remove(id);
            }
        }

        public async void PullAndNotify()
        {
            try
            {
                while (true)
                {
                    await Task.Delay(20000);

                    if (_devicesPerHook.IsEmpty)
                    {
                        continue;
                    }

                    var response = await _dataRepository.GetReportingDataAsync();

                    List<DeviceDTO> devices = new List<DeviceDTO>();

                    foreach (var whDevices in _devicesPerHook)
                    {
                        devices.Clear();

                        foreach (var foundDevice in response.Value)
                        {
                            if (!whDevices.Value.Contains(foundDevice.Id))
                            {
                                whDevices.Value.Add(foundDevice.Id);
                                devices.Add(foundDevice);
                            }
                        }

                        if (devices.Count != 0)
                        {
                            await NotifyAsync(whDevices.Key, devices);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        public async Task NotifyAsync(int id, List<DeviceDTO> devices)
        {
            WebhookEntity webhookItem = _webhookRepository[id];

            HttpClient httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(webhookItem.CallBackUrl);
            httpClient.DefaultRequestHeaders.Accept.Clear();

            HttpResponseMessage response = null;
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var httpContent = new StringContent(JsonConvert.SerializeObject(devices), Encoding.UTF8, "application/json");
            response = await httpClient.PostAsync("", httpContent);

            response.EnsureSuccessStatusCode();
        }
    }
}