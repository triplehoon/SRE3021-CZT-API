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
            //SRE3021API.StopAcqusition();
            Console.WriteLine($"Start");
            SRE3021API.CheckBaseline();
            for (int i = 0; i < 11; ++i)
            {
                for (int j = 0; j < 11; ++j)
                {
                    Console.WriteLine($"X: {i}, Y: {j}, Baseline {SRE3021API.AnodeValueBaseline[i, j]} // TimingBaseline{SRE3021API.AnodeTimingBaseline[i, j]}");

                }
            }
            
            SRE3021API.IMGDataEventRecieved += ProcessImgData;

            int aTime = 1;
            StratSREAcqusition("Cs137default", aTime);
            SRE3021API.StopAcqusition();

            for (int i = 1000; i <= 2000; i+=100)
            {
                StratSREAcqusition("Cs137_HVChanged", aTime, i);
            }
            SRE3021API.StopAcqusition();

            for (int i = 2300; i <= 2700; i += 50)
            {
                StratSREAcqusition("VTHR_Changed", aTime, 1500, i);
            }

            for (int i = 2300; i <= 2700; i += 50)
            {
                StratSREAcqusition("VTHR0_Changed", aTime, 1500, 2435, i);
            }

            for (int i = 300; i <= 1500; i += 200)
            {
                StratSREAcqusition("Hold_DLY_Changed", aTime, 1500, 2435, 2457, i);
            }
            Console.WriteLine($"{StratSREAcqusitionCount} minutes takes");


            SRE3021API.StopAcqusition();
            Thread.Sleep(500);

            SRE3021API.Close();
        }
        static int DataCount = 0;
        static int DoubleScatterCount = 0;
        static BlockingCollection<List<int>> Data = new BlockingCollection<List<int>>();
        static void ProcessImgData(SRE3021ImageData imgData)
        {
            List<int> ReturnValues = new List<int>();

            ReturnValues.Add(imgData.CathodeValue);
            ReturnValues.Add(imgData.CathodeTiming);
            for (int X = 0; X < 11; ++X)
            {
                for(int Y = 0; Y < 11; ++Y)
                {
                    ReturnValues.Add(X);
                    ReturnValues.Add(Y);
                    ReturnValues.Add(imgData.AnodeValue[X, Y]);
                    ReturnValues.Add(imgData.AnodeTiming[X, Y]);


                }
            }
            

            Data.Add(ReturnValues);
            ++DataCount;
            if (DataCount % 1000 == 0)
            {
                Console.WriteLine("DataCount {0}, {1}", DataCount, DoubleScatterCount);
            }
            
        }

        static int StratSREAcqusitionCount = 0;
        static void StratSREAcqusition(string fileName, int acqusitionMin, int HV = 1500,  int VTHR = 2435, int VTHR0 = 2457, int Hold_DLY = 300, int VFP0 = 1750)
        {
            bool DoneTryTake = false;
            string savePath = DateTime.Now.ToString("yyyyMMdd_HHmm_") + fileName + "_" + acqusitionMin + "min_" + HV + "_HV_" + VTHR + "_VTHR_" + VTHR0 + "_VTHR0_" + Hold_DLY + "_HOLD_DLY_" + VFP0 + "_VFP0" + ".csv";
            bool IsAcqusitionRunning = true;
            Task.Run(() =>
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                while (IsAcqusitionRunning)
                {
                    List<int> item;
                    using (StreamWriter outputFile = new StreamWriter("log\\"+savePath, true))
                    {
                        while (Data.TryTake(out item))
                        {
                            DoneTryTake = false;



                            outputFile.Write(sw.ElapsedMilliseconds);
                            outputFile.Write(",");
                            foreach (int i in item)
                            {
                                outputFile.Write(i);
                                outputFile.Write(",");
                            }
                            outputFile.WriteLine();

                        }
                    }
                    DoneTryTake = true;

                }
            });
            SRE3021API.StartAcqusition(HV, VTHR, VTHR0, Hold_DLY, VFP0);
            Console.WriteLine($"{savePath}Acqusition Start");
            for (int i = 0; i < acqusitionMin; ++i)
            {
                Thread.Sleep(1000 * 60);
                Console.WriteLine($"{i + 1} min passed. Total {acqusitionMin}");
            }


            SRE3021API.WriteSysReg(SRE3021SysRegisterADDR.CFG_PHYSTRIG_EN, 0);
            while (!DoneTryTake)
            {
                Console.WriteLine("Still Writing");
                Thread.Sleep(1000);
            }
            IsAcqusitionRunning = false;
        }
    }
}
