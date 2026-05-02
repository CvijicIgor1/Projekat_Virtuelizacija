using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Configuration;

namespace Service
{
    class Service
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(BatteryService));
            host.Open();
            Console.WriteLine("Servis pokrenut. Cekam konekciju...");
            Console.ReadLine(); 
            host.Close();
        }
    }
}
