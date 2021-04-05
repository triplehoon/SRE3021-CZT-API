using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HUREL.Compton.CZT;

namespace SRE3021_API_test_GUI
{
    public class MainViewModel: INotifyPropertyChanged
    {

        public MainViewModel()
        {
            if (SRE3021API.IsTCPOpen && SRE3021API.IsUDPOpen)
            {
                Trace.WriteLine("SRE3021 Loading Successs");
            }
        }

        


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }


    }
}

