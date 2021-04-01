using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HUREL.Compton.CZT;

namespace SRE3021_API_test_GUI
{
    public class MainViewModel: INotifyPropertyChanged
    {

        public static SRE3021API SRE3021API
        public MainViewModel()
        { 
            try
            {
                SR
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
}
