using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    class Client
    {
        static void Main(string[] args)
        {
            var factory = new ChannelFactory<IBatteryService>("BaterijaEndpoint");
            IBatteryService proxy = factory.CreateChannel();

            Console.WriteLine(proxy.Ping()); // dodao sam da bih proveravao da li app.config dobro radi

            DataHandler.SendFiles(proxy);  //cita i salje podatke serveru

            ((IClientChannel)proxy).Close();
            factory.Close();

            Console.ReadKey();
        }
    }
}
