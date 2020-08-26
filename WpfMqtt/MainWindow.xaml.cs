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

namespace WpfMqtt
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Connect(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Disconnect(object sender, RoutedEventArgs e)
        {

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
