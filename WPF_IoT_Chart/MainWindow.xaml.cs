using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPF_IoT_Chart
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel vm;
        public MainWindow()
        {
            vm = new ViewModel();
            InitializeComponent();
            DataContext = vm;
        }   

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            HubBrokerClient c = new HubBrokerClient();
            c.OnTelemetryReceived += C_OnTelemetryReceived;
            c.OnDisconnect += C_OnDisconnect;
            await c.ConnectAndSub();
            ButtonConnect.IsEnabled = false;
        }

        private void C_OnDisconnect(object? sender, EventArgs e)
        {
            ButtonConnect.IsEnabled = true;
        }

        private void C_OnTelemetryReceived(object? sender, TemperatureEventArgs e)
        {
            vm.AddItem(e.Temperature);
        }
    }
}
