using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

namespace SRE3021_API_test_GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            NativeMethods.AllocConsole();
            //Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("NDIzOTc3QDMxMzkyZTMxMmUzMEtWcE1uNm5oR3JxKzh0aG5WNVhLWlVERW1LbkdBNUhHWjh2Wm95R3Nhdzg9");

            InitializeComponent();
        }
        static class NativeMethods
        {
            [DllImport("kernel32.dll", SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool AllocConsole();
        }

    }
}
