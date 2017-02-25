using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Gpio;
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

namespace BlinkingLED
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        DispatcherTimer timer;
        GpioController gpio;
        GpioPin ledpin;
        GpioPinValue pinvalue;

        public MainPage()
        {
            this.InitializeComponent();

            #region Detect the present of GPIO Controller
            gpio = GpioController.GetDefault();

            if (gpio == null)
            {
                btn.IsEnabled = false;
                return;
            }

            ledpin = gpio.OpenPin(18);
            ledpin.SetDriveMode(GpioPinDriveMode.Output);
            #endregion

            #region Define timer
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(300);
            timer.Tick += Timer_Tick;
            #endregion
        }

        private void Timer_Tick(object sender, object e)
        {
            if (pinvalue == GpioPinValue.High)
            {
                pinvalue = GpioPinValue.Low;
                led.Fill = new SolidColorBrush(Colors.Transparent);
            }
            else
            {
                pinvalue = GpioPinValue.High;
                led.Fill = new SolidColorBrush(Colors.Red);
            }

            ledpin.Write(pinvalue);
        }

        private void btn_Click(object sender, RoutedEventArgs e)
        {
            if (btn.Content.ToString() == "Start")
            {
                btn.Content = "Stop";
                timer.Start();
            }
            else
            {
                btn.Content = "Start";
                timer.Stop();
            }
        }
    }
}
