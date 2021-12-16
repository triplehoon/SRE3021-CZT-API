using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using AsyncAwaitBestPractices.MVVM;
using HUREL.Compton;
using HUREL.Compton.CZT;

namespace SRE3021_API_test_GUI
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public MainViewModel()
        {
            SRE3021API.InitiateSRE3021API();

            SRE3021API.IMGDataEventRecieved += ProcessImgDataSaving;
            //SRE3021API.ReadWriteASICReg(SRE3021ASICRegisterADDR.Anode_Channel_3_Disable, true);

            //SRE3021API.ReadWriteASICReg(SRE3021ASICRegisterADDR.Anode_Channel_1_Disable, true);
        }

        private AsyncCommand startCZTCommmand;
        public IAsyncCommand StartCZTCommmand
        {
            get { return startCZTCommmand ?? (startCZTCommmand = new AsyncCommand(StartCZT, CanExecuteStartCZT)); }
        }
        private bool CanExecuteStartCZT(object arg)
        {            
            return !IsCZTRunning;
        }

        private async Task StartCZT()
        {
            ListModeData.Clear();
            await Task.Run(() =>
            {
                Message = "Starting CZT";
                IsCZTRunning = true;
                SRE3021API.StartAcqusition();
                System.Windows.Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.ApplicationIdle,
                    new Action(() => {
                        StopCZTCommmand.RaiseCanExecuteChanged();
                        StartCZTCommmand.RaiseCanExecuteChanged();
                    }));
            
                

                Message = "CZT is Started";
                while(IsCZTRunning)
                {
                    for (int i = 0; i < 1000; ++i)
                    {
                        Thread.Sleep(1);
                        DataCount = DataCountStatic;
                    }                    
                    SpectrumHisto = new ObservableCollection<HistoEnergy>(SRE3021API.GetSpectrumEnergy.HistoEnergies);
                }
            });
            
        }
        bool IsCZTRunning = false;
        private AsyncCommand stopCZTCommmand;
        public IAsyncCommand StopCZTCommmand
        {
            get { return stopCZTCommmand ?? (stopCZTCommmand = new AsyncCommand(StopCZT, CanExecuteStopCZT)); }
        }

        private bool CanExecuteStopCZT(object arg)
        {
            return IsCZTRunning;
        }

        private string fileName = "test.csv";
        public string FileName
        {
            get
            {
                return fileName;
            }
            set
            {
                FileName = value;
                OnPropertyChanged(nameof(FileName));
            }
        }

        private async Task StopCZT()
        {
           
            await Task.Run(() =>
            {
                IsCZTRunning = false;
                SRE3021API.StopAcqusition();                
            });
            Message = "CZT is stop";
            System.Windows.Application.Current.Dispatcher.Invoke(
                    DispatcherPriority.ApplicationIdle,
                    new Action(() => {
                        StartCZTCommmand.RaiseCanExecuteChanged();
                        StopCZTCommmand.RaiseCanExecuteChanged();
                    }));
            Message = "Saving start...";
            using (System.IO.StreamWriter file = new System.IO.StreamWriter(FileName))
            {
                //int CathodeValue, int CathodeTiming, int[,] AnodeValue, int[,] AnodeTiming);
                file.Write("CathodeValue, CathodeTiming,");
                for (int x = 0; x < 11; ++x)
                {
                    for (int y = 0; y < 11; ++y)
                    {
                        file.Write($"AnodeValue({x}.{y}),");
                        file.Write($"AnodeTiming({x}.{y}),");
                    }
                }
                file.WriteLine();
                foreach (SRE3021ImageData lmd in ListModeData)
                {
                    file.Write($"{lmd.CathodeValue},");
                    file.Write($"{lmd.CathodeTiming},");
                    for (int x = 0; x < 11; ++x)
                    {
                        for (int y = 0; y < 11; ++y)
                        {
                            file.Write($"{lmd.AnodeValue[x,y]},");
                            file.Write($"{lmd.AnodeTiming[x, y]},");
                        }
                    }
                    file.WriteLine();
                }
            }
            Message = "Save done";
        }

        private AsyncCommand resetSpectrumCommand;
        public IAsyncCommand ResetSpectrumCommand
        {
            get { return resetSpectrumCommand ?? (resetSpectrumCommand = new AsyncCommand(ResetSpectrum)); }
        }
        private async Task ResetSpectrum()
        {
            await Task.Run(() =>
            {
                SpectrumEnergy.Reset();
            });
        }

        private AsyncCommand setHVCommand;
        public IAsyncCommand SetHVCommand
        {
            get { return setHVCommand ?? (setHVCommand = new AsyncCommand(SetHV)); }
        }
        private async Task SetHV()
        {
            await Task.Run(() =>
            {
                SRE3021API.SetHighVoltage(hvValue, 10, 100);
            });
        }

        private int hvValue;
        public int HVValue
        {
            get
            {
                return hvValue;
            }
            set
            {
                hvValue = value;
                OnPropertyChanged(nameof(HVValue));
            }
        }


        private string message;
        public string Message
        {
            get
            {
                return message;
            }
            set
            {
                message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
        static private List<string> Edata = new List<string>();
        static private List<SRE3021ImageData> ListModeData = new List<SRE3021ImageData>();
        static void ProcessImgDataSaving(SRE3021ImageData imgData)
        {
            ListModeData.Add(imgData);
            DataCountStatic++;
        }
        static int DataCountStatic = 0;

        const double p1 = 0.2987;
        const double p2 = 49.72;

        private int dataCount = 0;
        public int DataCount
        {
            get { return dataCount; }
            set
            {
                dataCount = value;
                OnPropertyChanged(nameof(DataCount));
            }
        }

        static private SpectrumEnergy SpectrumEnergy = new SpectrumEnergy(5, 2000);

        private ObservableCollection<HistoEnergy> spectrumHisto = new ObservableCollection<HistoEnergy>();
        public ObservableCollection<HistoEnergy> SpectrumHisto
        {
            get
            {
                return spectrumHisto;
            }
            set
            {
                spectrumHisto = value;
                OnPropertyChanged(nameof(SpectrumHisto));
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

