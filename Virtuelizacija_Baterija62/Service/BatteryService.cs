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
            throw new NotImplementedException();
        }

        public string Ping() //dodao sam da bih proveravao da li AppConfig radi kako treba
        {
            Console.WriteLine("Primljen ping od klijenta.");
            return "POVEZAN";
        }

        public void PushSample(EisSample sample)
        {
            throw new NotImplementedException();
        }

        public void StartSession(EisMeta meta)
        {
            throw new NotImplementedException();
        }
    }
}
