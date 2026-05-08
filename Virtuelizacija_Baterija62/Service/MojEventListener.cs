using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Service.BatteryEventArgs;

namespace Service
{
    public class MojEventListener
    {
        public void OnTempSpike(object sender, TempSpikes e)
        {
            string type = "PORAST";
            if (e.deltaT < 0)
                type = "PAD";

            Console.WriteLine($"Temperatura je presla prag! {type} T: {e.T}, deltaT: {e.deltaT}, SoC: {e.soC}, Frequency: {e.frequency}");
        }
    }

}
