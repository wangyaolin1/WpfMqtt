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

using MQTT_Client.ViewModels;

namespace WpfMqtt
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        UI_ViewModel ViewModel = new UI_ViewModel();

        public MainWindow()
        {
            InitializeComponent();

            base.DataContext = ViewModel;

            Closing += ViewModel.OnWindowClosing;
        }

        private void userCertificateValidationCallback(object sender)
        {
            Console.WriteLine("Certificate validated");
        }


        // this code runs when the main window closes (end of the app)
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            App.Current.Shutdown();
        }

        private void P_w_d_Box_PasswordChanged(object sender, RoutedEventArgs e)
        {
            var box = sender as PasswordBox;

            UI_ViewModel.the_p_w_d = box.SecurePassword;

        }

        private void MenuItem_Click_Quit(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MenuItem_Click_About(object sender, RoutedEventArgs e)
        {
            WpfMqtt.About about = new About();
            about.ShowDialog();
        }
    }
}
