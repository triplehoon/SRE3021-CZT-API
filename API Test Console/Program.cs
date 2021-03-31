using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading;
using HUREL.Compton.CZT;


namespace API_Test_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var CZTAPI = new SRE3021API();                        
            foreach (var regs in CZTAPI.SRE3021SysRegisters)
            {
                Console.WriteLine("{0} value is {1}", regs.AddressName, regs.Value);
            }

            Console.WriteLine($"X  Y    Channel    ASIC");
            for (int i = 0; i < 11; ++i)
            {
                for (int j = 0; j < 11; ++j)
                {
                    Console.WriteLine($"{i}  {j}    {CZTAPI.ChannelNumber[i, j]}    {CZTAPI.ASICChannelNumber[i, j]}");
                }
            }

            
        }


        static void ProcessImgData(SRE3021ImageData imgData)
        {
            int max = 0;
            foreach(var a in imgData.AnodeValue)
            {
                if (max < a)
                {
                    max = a;
                }
            }
            
            Console.WriteLine("Max is {0}: {1}", max, SRE3021API.UDPPacketCount);
        }
    }
}
