using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using HUREL.Compton.CZT;


namespace API_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var CZTAPI = new SRE3021API();
            CZTAPI.StopAqusition();
            CZTAPI.CheckBaseline();

            for (int i = 0; i < 11; ++i)
            {
                for (int j = 0; j < 11; ++j)
                {
                    Console.WriteLine($"X: {i}, Y: {j}, Baseline {CZTAPI.AnodeValueBaseline[i, j]} // TimingBaseline{CZTAPI.AnodeTimingBaseline[i, j]}");

                }
            }
            bool DoneTryTake = false;
            CZTAPI.IMGDataEventRecieved += ProcessImgData;
            Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                string savePath ="Ba_SimpleSum3_60s" + ".csv";
                while (true)
                { 
                    int item;
                    using (StreamWriter outputFile = new StreamWriter(savePath, true))
                    {
                        while (Data.TryTake(out item))
                        {
                            DoneTryTake = false;



                            outputFile.Write(sw.ElapsedMilliseconds);
                            outputFile.Write(",");
                            outputFile.WriteLine(item);
                        
                        }
                    }
                    DoneTryTake = true;

                }
            });
            CZTAPI.StartAqusition();
            Thread.Sleep(1000 * 10);

            CZTAPI.WriteSysReg(SRE3021SysRegisterADDR.CFG_PHYSTRIG_EN, 0);
            CZTAPI.StopAqusition();
            while (!DoneTryTake)
            {
                Thread.Sleep(1000);
            }
        }
        static int DataCount = 0;
        static int DoubleScatterCount = 0;
        static BlockingCollection<int> Data = new BlockingCollection<int>();
        static void ProcessImgData(SRE3021ImageData imgData)
        {
            int max = -500;
            int Xmax = 0;
            int Ymax = 0;
            for(int X = 0; X < 11; ++X)
            {
                for(int Y = 0; Y < 11; ++Y)
                {
                    if (imgData.AnodeValue[X,Y] > max)
                    {
                        max = imgData.AnodeValue[X, Y];
                        Xmax = X;
                        Ymax = Y;   
                    }
                }
            }
            /// Simple Sum
            int trueMax;
            if (Xmax > 0 && Xmax < 10)
            {
                if (Ymax > 0 && Ymax < 10)
                {
                    
                }
                else
                {
                    return;
                }
            }
            else
            {
                return;
            }


            Data.Add(trueMax);
            ++DataCount;
            if (DataCount % 1000 == 0)
            {
                Console.WriteLine("Max is {0}: {1} // {2}", trueMax, DataCount, DoubleScatterCount);
            }
            
        }
    }
}
