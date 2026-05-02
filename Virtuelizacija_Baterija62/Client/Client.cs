using Common;
using System;
using System.Collections.Generic;
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
            IBatteryService client = factory.CreateChannel();

            Console.WriteLine(client.Ping()); // dodao sam da bih proveravao da li app.config dobro radi

            ((IClientChannel)client).Close();
            factory.Close();

            Console.ReadKey();
        }
    }
}
