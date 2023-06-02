using CommunityToolkit.Mvvm.Input;
using LiveCharts;
using LiveCharts.Wpf;
using MQTTnet.Client;
using Newtonsoft.Json;
using System.Text;
using System.Windows.Input;
using Timer = System.Timers.Timer;
using MqttClient = GUIHOMEASSIST.Application.Communication.MqttClient;
using System.Collections.ObjectModel;
using System;
using GUIHOMEASSIST.Application.Communication;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows;

namespace GUIHOMEASSIST.Application.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly Timer _timer;
        private readonly Timer _timerfan;
        private readonly Timer _timerbell;
        private readonly MqttClient _mqttClient;
        public bool IsMqttConnected => _mqttClient.IsConnected;
        public int Deg { get; set; } = 0;
        public double Temperature { get; set; }
        public long Concentration { get; set; }
        public long Humidity { get; set; }
        public double SetTemp { get; set; } = 0;
        public long SetConcentration { get; set; } = 0;
        public bool ToggleLed1 { get; set; } = false;
        public bool ToggleLed2 { get; set; } = false;
        public bool Led1 { get; set; }
        public bool Led2 { get; set; }
        public bool Fanstatus { get; set; }
        public bool Warning { get; set; } 
        public bool A { get; set; } = true;
        public Visibility FanONVis { get; set; }
        public Visibility FanOFFVis { get; set; }
        public Visibility BellONVis { get; set; }
        public Visibility BellOFFVis { get; set; }
        public Visibility Blank { get; set; } = Visibility.Visible;
        public ICommand ConnectCommand { get; set; }
        public ICommand SetCommand { get; set; }
        public List<Tag> Tags { get; private set; }
        ///
        public SeriesCollection SeriesCollection1 { set; get; }
        public ObservableCollection<string> Lables1 { set; get; }
        ChartValues<Double> NhietDo = new ChartValues<double>();
        ChartValues<Double> NhietDoChuan = new ChartValues<double>();

        public SeriesCollection SeriesCollection2 { set; get; }
        public ObservableCollection<string> Lables2 { set; get; }
        ChartValues<Double> KhiGas = new ChartValues<double>();
        ChartValues<Double> KhiGasChuan = new ChartValues<double>();
        LineSeries LineNhietDo =
                    new LineSeries()
                    {
                        Title = "Nhiệt độ",
                        PointGeometry = DefaultGeometries.Circle,
                        PointForeground = Brushes.SkyBlue,
                        PointGeometrySize = 4
                    };
        LineSeries LineNhietDoChuan =
            new LineSeries()
            {
                Title = "Nhiệt độ chuẩn",
                PointGeometry = DefaultGeometries.Circle,
                PointForeground = Brushes.SkyBlue,
                PointGeometrySize = 4
            };

        LineSeries LineKhiGas =
        new LineSeries()
        {
            Title = "Nồng độ",

            PointGeometry = DefaultGeometries.Circle,
            PointForeground = Brushes.SkyBlue,
            PointGeometrySize = 4

        };
        
        LineSeries LineKhiGasChuan =
        new LineSeries()
        {
            Title = "Nồng độ chuẩn",

            PointGeometry = DefaultGeometries.Circle,
            PointForeground = Brushes.SkyBlue,
            PointGeometrySize = 4

        };
        public MainViewModel()
        {
            _mqttClient = new MqttClient();
            _timer = new Timer(1000);
            _timerfan = new Timer(200);
            _timerbell = new Timer(300);
            _timer.Elapsed += TimerElapsed;
            _timerfan.Elapsed += _TimerfanElapsed;
            _timerbell.Elapsed += _TimerbellElapsed;
            _mqttClient.ApplicationMessageReceived += OnApplicationMessageReceived;
            ConnectCommand = new RelayCommand(Connect);
            SetCommand = new RelayCommand(Set);
            ///
            SeriesCollection1 = new SeriesCollection();
            Lables1 = new ObservableCollection<string>();

            SeriesCollection2 = new SeriesCollection();
            Lables2 = new ObservableCollection<string>();
            ///
            Tags = new()
            {
                new("Tempsetpoint",SetTemp),
                new("Concentrationsetpoint",SetConcentration),
                new("LED_1",ToggleLed1),
                new("LED_2",ToggleLed2)
            };
        }

        private void _TimerbellElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (A)
            {
                Blank = Visibility.Visible;
                A = false;
                return;

            }
            if (!A)
            {
                Blank = Visibility.Hidden;
                A = true;
                return;
            }
        }

        private void _TimerfanElapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            Deg += 30;
        }

        private async void TimerElapsed(object? sender, EventArgs args)
        {
            
            if ((bool)GetTag("LED_1").Value != ToggleLed1)
            {
                GetTag("LED_1").Value = ToggleLed1;
                await _mqttClient.Publish("BTL/ESP32_PUB/led1", JsonConvert.SerializeObject(GetTag("LED_1")), true);
            }
            GetTag("LED_1").Value = ToggleLed1;

            if ((bool)GetTag("LED_2").Value != ToggleLed2)
            {
                GetTag("LED_2").Value = ToggleLed2;
                await _mqttClient.Publish("BTL/ESP32_PUB/led2", JsonConvert.SerializeObject(GetTag("LED_2")), true);
            }
            GetTag("LED_2").Value = ToggleLed2;
           
        }

        private async Task OnApplicationMessageReceived(MqttApplicationMessageReceivedEventArgs e)
        {
            var topic = e.ApplicationMessage.Topic;
            var payloadMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            var commandMessage = JsonConvert.DeserializeObject<CommandMessage>(payloadMessage);

            switch (commandMessage.Name)
            {
                case "Temp":
                    Temperature = Convert.ToDouble(commandMessage.Value);
                    Set();
                    NhietDo.Add(Temperature);
                    Lables1.Add(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
                    break;

                case "Gas":
                    Concentration = Convert.ToInt16(commandMessage.Value);
                    Set();
                    KhiGas.Add(Concentration);
                    Lables2.Add(DateTime.Now.Hour + ":" + DateTime.Now.Minute + ":" + DateTime.Now.Second);
                    break;

                case "Humidity":
                    Humidity = Convert.ToInt16(commandMessage.Value);
                    break;

                case "LED_1":
                    Led1 = Convert.ToBoolean(commandMessage.Value);
                    break;

                case "LED_2":
                    Led2 = Convert.ToBoolean(commandMessage.Value);
                    break;

                case "FAN":
                    Fanstatus = Convert.ToBoolean(commandMessage.Value);
                    _timerfan.Enabled = true;
                    if (Fanstatus == true)
                    {
                        FanONVis = Visibility.Visible;
                        FanOFFVis = Visibility.Hidden;
                    }
                    else
                    {
                        FanONVis = Visibility.Hidden;
                        FanOFFVis = Visibility.Visible;
                        _timerfan.Enabled = false;
                    }
                    break;

                case "Warning":
                    Warning = Convert.ToBoolean(commandMessage.Value);
                    _timerbell.Enabled = true;
                    if (Warning == true)
                    {
                        BellONVis = Visibility.Visible;
                        BellOFFVis = Visibility.Hidden;
                    }
                    else
                    {
                        BellONVis = Visibility.Hidden;
                        BellOFFVis = Visibility.Visible;
                        Blank = Visibility.Hidden;
                        _timerbell.Enabled = false;
                    }
                    break;



            }
        }
        public Tag GetTag(string tagName)
        {
            return Tags.Find(x => x.Name == tagName);

        }
        private async void Set()
        {
            NhietDoChuan.Add(SetTemp);
            GetTag("Tempsetpoint").Value = SetTemp;
            await _mqttClient.Publish("BTL/ESP32_PUB/tempsetpoint", JsonConvert.SerializeObject(GetTag("Tempsetpoint")), true);

            KhiGasChuan.Add(SetConcentration);
            GetTag("Concentrationsetpoint").Value = SetConcentration;
            await _mqttClient.Publish("BTL/ESP32_PUB/concentrationsetpoint", JsonConvert.SerializeObject(GetTag("Tempsetpoint")), true);
        }

        private async void Connect()
        {
            await _mqttClient.ConnectAsync();
            await _mqttClient.Subscribe("BTL/ESP32_PUB");
            await _mqttClient.Subscribe("BTL/ESP32_PUB/humidity");
            await _mqttClient.Subscribe("BTL/ESP32_PUB/temp");
            await _mqttClient.Subscribe("BTL/ESP32_PUB/gas");
            await _mqttClient.Subscribe("BTL/ESP32_PUB/led1");
            await _mqttClient.Subscribe("BTL/ESP32_PUB/led2");
            await _mqttClient.Subscribe("BTL/ESP32_PUB/fan");
            await _mqttClient.Subscribe("BTL/ESP32_PUB/warning");
            _timer.Enabled = true;
            
            LineNhietDo.Values = NhietDo;
            LineNhietDoChuan.Values = NhietDoChuan;
            SeriesCollection1.Add(LineNhietDo);
            SeriesCollection1.Add(LineNhietDoChuan);

            LineKhiGas.Values = KhiGas;
            LineKhiGasChuan.Values= KhiGasChuan;
            SeriesCollection2.Add(LineKhiGas);
            SeriesCollection2.Add(LineKhiGasChuan);
        }
    }
}
