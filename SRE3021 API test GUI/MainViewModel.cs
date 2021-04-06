using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using AsyncAwaitBestPractices.MVVM;
using HUREL.Compton.CZT;

namespace SRE3021_API_test_GUI
{
    public class MainViewModel : INotifyPropertyChanged
    {

        public MainViewModel()
        {
            if (SRE3021API.IsTCPOpen && SRE3021API.IsUDPOpen)
            {
                Trace.WriteLine("SRE3021 Loading Successs");
            }

            SRE3021API.IMGDataEventRecieved += ProcessImgData;

            SRE3021API.CheckBaseline();
            for (int i = 0; i < 11; ++i)
            {
                for (int j = 0; j < 11; ++j)
                {
                    Debug.WriteLine($"X: {i}, Y: {j}, Baseline {SRE3021API.AnodeValueBaseline[i, j]} // TimingBaseline{SRE3021API.AnodeTimingBaseline[i, j]}");

                }
            }

           


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
                    SpectrumHisto = new ObservableCollection<HistoEnergy>(SpectrumEnergy.HistoEnergies);
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

        static void ProcessImgData(SRE3021ImageData imgData)
        {
            int interactionX = -1;
            int interactionY = -1;
            int interactionPotins = 0;
            int backgroundNoise = 0;
            for (int X = 0; X < 11; ++X)
            {
                for (int Y = 0; Y < 11; ++Y)
                {
                    if (imgData.AnodeTiming[X, Y] > 50)
                    {
                        ++interactionPotins;
                        if (interactionPotins == 2) 
                        {
                            return;
                        }
                        interactionX = X;
                        interactionY = Y;
                    }
                    else
                    {
                        backgroundNoise += imgData.AnodeValue[X, Y];
                    }
                    
                }
            }
            if (interactionX == -1)
            {
                return;
            }
            backgroundNoise = backgroundNoise / 118;

            SpectrumEnergy.AddEnergy(imgData.AnodeValue[interactionX, interactionY] * p1 + p2);

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

        static private SpectrumEnergy SpectrumEnergy = new SpectrumEnergy(2, 1500);

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

    public class SpectrumEnergy
    {
        public List<HistoEnergy> HistoEnergies = new List<HistoEnergy>();

        private List<double> EnergyBin = new List<double>();

        public SpectrumEnergy(int binSize, double MaxEnergy)
        {
            int binCount = (int)(MaxEnergy / binSize);
            for (int i = 0; i < binCount; ++i)
            {
                double energy = i * binSize;

                EnergyBin.Add(energy);
                HistoEnergies.Add(new HistoEnergy(energy));
            }
        }

        public void AddEnergy(double energy)
        {
            for (int i = 0; i < EnergyBin.Count - 1; ++i)
            {
                if (energy < EnergyBin[i + 1] && energy > EnergyBin[i])
                {
                    HistoEnergies[i].Count++;
                    break;
                }
            }
        }
        public void AddEnergy(List<double> energy)
        {
            foreach (double d in energy)
            {
                AddEnergy(d);
            }
        }

        public void Reset()
        {
            foreach (var data in HistoEnergies)
            {
                data.Count = 0;
            }
        }
    }

    public class HistoEnergy
    {
        public double Energy { get; set; }
        public int Count { get; set; }
        public HistoEnergy(double energy)
        {
            Energy = energy;

        }

    }

}

