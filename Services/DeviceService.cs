using ReferenceInfoSystem.Models;
using System.Net.Http;
using System.Text.Json;

namespace ReferenceInfoSystem.Services
{
    public class DeviceService
    {
        private readonly HttpClient _httpClient;

        public DeviceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<Device>> LoadDevicesAsync(string deviceType)
        {
            var response = await _httpClient.GetAsync($"api/devices?type={deviceType}");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var devices = JsonSerializer.Deserialize<List<Device>>(content);
            return devices ?? new List<Device>();
        }
    }

}
