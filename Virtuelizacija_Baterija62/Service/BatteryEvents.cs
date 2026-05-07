using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class BatteryEvents
    {
        public class TransferStartedEventArgs : EventArgs
        {
            public string BatteryId { get; set; }
            public string TestId { get; set; }
            public double SoC { get; set; }
        }

        public class SampleReceivedEventArgs : EventArgs
        {
            public int RowIndex { get; set; }
            public double FrequencyHz { get; set; }
            public double T_degC { get; set; }
            public int Primljeno { get; set; }
            public int Ukupno { get; set; }
        }

        public class TransferCompletedEventArgs : EventArgs
        {
            public string BatteryId { get; set; }
            public int PrimljenoUzoraka { get; set; }
        }

        public class WarningRaisedEventArgs : EventArgs
        {
            public string Poruka { get; set; }
            public string Tip { get; set; }
        }
    }
}
