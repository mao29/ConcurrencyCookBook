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

namespace WpfDeadlockDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        
        async Task DoSmthAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var task = DoSmthAsync();

            task.Wait();
        }       

        async Task DoSmthRightAsync()
        {
            await Task.Delay(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var task = DoSmthRightAsync();

            task.Wait();
        }

        private async void Button_Click_2(object sender, RoutedEventArgs e)
        {
            await DoSmthRightAsync();
        }
    }
}
