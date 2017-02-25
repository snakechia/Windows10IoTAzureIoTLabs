using Emmellsoft.IoT.Rpi.SenseHat;
using Emmellsoft.IoT.Rpi.SenseHat.Fonts.SingleColor;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace SenseHAT
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        ISenseHat senseHat;
        SensorData sensordata = new SensorData();
        DispatcherTimer timer;
        
        ISenseHatDisplay display;
        TinyFont tinyFont = new TinyFont();
        int count = 0;

        // Lab 8
        DeviceClient deviceClient;
        string deviceName = "test";
        string deviceconnectionstring = "HostName=SCIoTDemo.azure-devices.net;DeviceId=test;SharedAccessKey=Z87GOjXWXIYle1rchzR9vzRjiNoNoHS67hYdsBOWr5E=";
        
        public MainPage()
        {
            this.InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;

            // Lab 8
            deviceClient = DeviceClient.CreateFromConnectionString(deviceconnectionstring);

            ReceiveDataFromAzureIoTHub();
        }

        private void Timer_Tick(object sender, object e)
        {
            getsensordata();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                senseHat = await SenseHatFactory.GetSenseHat();
                timer.Start();
            }
            catch
            {
                return;
            }
        }

        private void getsensordata()
        {
            senseHat.Sensors.HumiditySensor.Update();
            senseHat.Sensors.PressureSensor.Update();
            
            sensordata.temperature = (double)senseHat.Sensors.Temperature;
            sensordata.pressure = (double)senseHat.Sensors.Pressure;
            sensordata.humidity = (double)senseHat.Sensors.Humidity;
            sensordata.createdAt = DateTime.UtcNow;

            // this simple app we do not implement NotifyPropertyChanged
            // hence we manually refreshed the databinding.
            dataGrid.DataContext = null;
            dataGrid.DataContext = sensordata;

            displayDataOn8x8();

            sensordata.deviceId = deviceName;
            string message = JsonConvert.SerializeObject(sensordata);
            sendDataToAzureIoTHub(message);
        }

        // Display value on 8x8 LED Matrix using TinyFont
        private void displayDataOn8x8()
        {
            double value = sensordata.temperature;
            Color color = Colors.Blue;
            
            switch (count)
            {
                case 1:
                    value = double.Parse(senseHat.Sensors.Humidity.ToString());
                    color = Colors.Red;
                    break;

                default:
                    value = sensordata.temperature;
                    color = Colors.Blue;
                    break;
            }

            display = senseHat.Display;
            display.Clear();
            tinyFont.Write(display, ((int)Math.Round(value)).ToString(), color);
            display.Update();
            
            count++;

            // Reset counter to 0 so that the LED matrix can show temperature value
            if (count > 1)
            {
                count = 0;
            }
        }
        
        // Lab 8 - Send data to Azure IoT Hub
        private async Task sendDataToAzureIoTHub(string message)
        {
            try
            {
                var msg = new Message(Encoding.UTF8.GetBytes(message));
                await deviceClient.SendEventAsync(msg);
            }
            catch
            { }
        }

        // Lab 8 - Receive data from Azure IoT Hub
        public async Task ReceiveDataFromAzureIoTHub()
        {
            try
            {
                Message receivedMessage;
                string messageData;

                while (true)
                {
                    receivedMessage = await deviceClient.ReceiveAsync();
                    if (receivedMessage != null)
                    {
                        messageData = Encoding.ASCII.GetString(receivedMessage.GetBytes());
                        await deviceClient.CompleteAsync(receivedMessage);

                        if (messageData.Length > 2)
                            return;

                        timer.Stop();
                        
                        display.Clear();
                        tinyFont.Write(display, messageData, Colors.Green);
                        display.Update();

                        timer.Start();
                    }
                }
            }
            catch
            { }
        }

    }
}
