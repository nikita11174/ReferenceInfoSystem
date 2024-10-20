using ReferenceInfoSystem.Models;
using ReferenceInfoSystem.Models.ReferenceInfoSystem.Models;
using System.Globalization;
using System.Net.Http;
using System.Reflection;
using System.Resources;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ReferenceInfoSystem
{
    public partial class MainWindow : Window
    {
        private readonly HttpClient _httpClient;
        private List<Device>? _devices;
        private readonly ResourceManager _resourceManager;
        private CultureInfo _currentCulture;

        public List<DeviceType>? DeviceTypes { get; internal set; }


        public MainWindow() : this(new HttpClient()) { }


        public MainWindow(HttpClient httpClient)
        {
            InitializeComponent();
            _httpClient = httpClient;
            _resourceManager = new ResourceManager("ReferenceInfoSystem.Resources.Resources", typeof(MainWindow).Assembly);
            _currentCulture = CultureInfo.CurrentUICulture;
            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            await LoadDeviceTypesAsync();
        }

        public async Task LoadDeviceTypesAsync()
        {
            try
            {
                string url = "https://2392bb8b-2589-4515-a05d-bff3882c6c75.mock.pstmn.io/devices";
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var deviceTypes = JsonSerializer.Deserialize<List<DeviceType>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                UpdateDeviceTypeLocalNames(deviceTypes);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    SetDeviceTypeComboBoxItemsSource(deviceTypes);
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_resourceManager.GetString("ErrorLoadingDevices", _currentCulture)}: {ex.Message}");
            }
        }


        public async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedDeviceType = DeviceTypeComboBox.SelectedItem as DeviceType;
            if (selectedDeviceType == null)
            {
                MessageBox.Show(_resourceManager.GetString("SelectDeviceType", _currentCulture));
                return;
            }

            string endpoint = selectedDeviceType.Name.ToLower();
            string url = $"https://2392bb8b-2589-4515-a05d-bff3882c6c75.mock.pstmn.io/{endpoint}";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                _devices = JsonSerializer.Deserialize<List<Device>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                foreach (var device in _devices)
                {
                    device.Description = _resourceManager.GetString(device.Name ?? string.Empty, _currentCulture) ?? device.Name;
                }

                DevicesDataGrid.ItemsSource = _devices;
                PropertiesDataGrid.ItemsSource = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{_resourceManager.GetString("ErrorLoadingData", _currentCulture)}: {ex.Message}");
            }
        }

        public void DevicesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DevicesDataGrid.SelectedItem is Device selectedDevice)
            {
                PropertiesDataGrid.ItemsSource = GetDeviceProperties(selectedDevice);
            }
            else
            {
                PropertiesDataGrid.ItemsSource = null;
            }
        }

        private List<PropertyValue> GetDeviceProperties(Device selectedDevice)
        {
            var propertiesList = new List<PropertyValue>();
            var deviceType = selectedDevice.GetType();
            var allProperties = deviceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var standardProperties = new List<string> { "Id", "Code", "Name", "AdditionalProperties" };

            foreach (var prop in allProperties)
            {
                if (!standardProperties.Contains(prop.Name))
                {
                    var value = prop.GetValue(selectedDevice);
                    propertiesList.Add(new PropertyValue { Property = prop.Name, Value = value });
                }
            }

            foreach (var prop in selectedDevice.AdditionalProperties)
            {
                object? value = null;
                switch (prop.Value.ValueKind)
                {
                    case JsonValueKind.String:
                        value = prop.Value.GetString();
                        break;
                    case JsonValueKind.Number:
                        if (prop.Value.TryGetInt32(out int intValue))
                            value = intValue;
                        else if (prop.Value.TryGetDouble(out double doubleValue))
                            value = doubleValue;
                        break;
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        value = prop.Value.GetBoolean();
                        break;
                    default:
                        value = prop.Value.ToString();
                        break;
                }

                propertiesList.Add(new PropertyValue { Property = prop.Key, Value = value });
            }

            return propertiesList;
        }

        public void LanguageButton_Click(object sender, RoutedEventArgs e)
        {
            _currentCulture = _currentCulture.Name == "en" ? new CultureInfo("ru") : new CultureInfo("en");
            Thread.CurrentThread.CurrentUICulture = _currentCulture;
            Thread.CurrentThread.CurrentCulture = _currentCulture;
            UpdateUI();
        }

        public void UpdateUI()
        {
            this.Title = _resourceManager.GetString("WindowTitle", _currentCulture);
            DeviceTypeLabel.Content = _resourceManager.GetString("DeviceType", _currentCulture);
            LoadButton.Content = _resourceManager.GetString("LoadButton", _currentCulture);
            LanguageButton.Content = _resourceManager.GetString("Language", _currentCulture);

            identificator.Header = _resourceManager.GetString("ID", _currentCulture);
            Designation.Header = _resourceManager.GetString("Designation", _currentCulture);
            Name.Header = _resourceManager.GetString("Name", _currentCulture);
            Properties.Header = _resourceManager.GetString("Properties", _currentCulture);
            Property.Header = _resourceManager.GetString("Property", _currentCulture);
            Value.Header = _resourceManager.GetString("Value", _currentCulture);

            if (DeviceTypeComboBox.ItemsSource is List<DeviceType> deviceTypes)
            {
                UpdateDeviceTypeLocalNames(deviceTypes);
                SetDeviceTypeComboBoxItemsSource(deviceTypes);
            }
        }

        private void UpdateDeviceTypeLocalNames(List<DeviceType> deviceTypes)
        {
            foreach (var device in deviceTypes)
            {
                device.LocalName = _currentCulture.Name == "ru-RU" ? device.Description : char.ToUpper(device.Name[0]) + device.Name.Substring(1).ToLower();
            }
        }

        private void SetDeviceTypeComboBoxItemsSource(List<DeviceType> deviceTypes)
        {
            DeviceTypeComboBox.ItemsSource = null;
            DeviceTypeComboBox.ItemsSource = deviceTypes;
        }

    }
}
