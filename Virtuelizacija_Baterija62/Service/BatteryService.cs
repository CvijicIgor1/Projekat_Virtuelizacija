using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class BatteryService : IBatteryService
    {
        public void EndSession()
        {
            Console.WriteLine("=====Sesija je zavrsena.=====");
        }

        public void PushSample(EisSample sample)
        {
            Console.WriteLine($"=====Uzorak {sample.RowIndex} primljen.=====");
        }

        public void StartSession(EisMeta meta)
        {
            Console.WriteLine($"=====Sesija je otvorena.=====");
        }

        public string Ping() //dodao sam da bih proveravao da li AppConfig radi kako treba
        {
            Console.WriteLine("Primljen ping od klijenta.");
            return "POVEZAN";
        }
    }
}
