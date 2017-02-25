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

namespace PushButton
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        GpioController gpio;
        GpioPin ledpin, pushbuttonpin;
        GpioPinValue ledpinvalue;

        public MainPage()
        {
            this.InitializeComponent();

            initGPIO();
        }

        private void initGPIO()
        {
            gpio = GpioController.GetDefault();

            if (gpio == null)
                return;

            ledpin = gpio.OpenPin(18);
            ledpin.SetDriveMode(GpioPinDriveMode.Output);
            ledpin.Write(GpioPinValue.Low);

            pushbuttonpin = gpio.OpenPin(12);

            // Check if input pull-up resistors are supported
            if (pushbuttonpin.IsDriveModeSupported(GpioPinDriveMode.InputPullUp))
                pushbuttonpin.SetDriveMode(GpioPinDriveMode.InputPullUp);
            else
                pushbuttonpin.SetDriveMode(GpioPinDriveMode.Input);

            // Set a debounce timeout to filter out switch bounce noise from a button press
            pushbuttonpin.DebounceTimeout = TimeSpan.FromMilliseconds(50);

            // Register for the ValueChanged event so when the push button being press,
            // function will be run.
            pushbuttonpin.ValueChanged += Pushbuttonpin_ValueChanged;

        }

        private async void Pushbuttonpin_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            // toggle the state of the LED every time the button is pressed
            if (args.Edge == GpioPinEdge.FallingEdge)
            {
                ledpinvalue = (ledpinvalue == GpioPinValue.Low) ? GpioPinValue.High : GpioPinValue.Low;
                ledpin.Write(ledpinvalue);
            }

            // need to invoke UI updates on the UI thread because this event
            // handler gets invoked on a separate thread.
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (args.Edge == GpioPinEdge.FallingEdge)
                {
                    ellipse.Fill = (ledpinvalue == GpioPinValue.Low) ? new SolidColorBrush(Colors.Blue) : new SolidColorBrush(Colors.Transparent);
                }
            });
        }
    }
}
