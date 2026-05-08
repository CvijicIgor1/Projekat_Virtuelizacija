using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Configuration;
using System.IO;
using System.Diagnostics.Tracing;
using Common;

namespace Service
{
    class Service
    {
        static void Main(string[] args)
        {
            BatteryService servis = new BatteryService(); // instanca je u pitanju da se ne zbunimo

            MojEventListener mojListener = new MojEventListener();

            servis.OnTransferStarted += (s, e) => Console.WriteLine($"[EVENT] Sesija pocela: {e.BatteryId}/{e.TestId} SoC={e.SoC}%");

            servis.OnSampleReceived += (s, e) => Console.WriteLine($"[EVENT] Uzorak #{e.RowIndex} | T={e.T_degC}°C | {e.Primljeno}/{e.Ukupno}");

            servis.OnTransferCompleted += (s, e) => Console.WriteLine($"[EVENT] Transfer zavrsen: {e.BatteryId}, uzoraka: {e.PrimljenoUzoraka}");

            servis.OnWarningRaised += (s, e) => Console.WriteLine($"[UPOZORENJE][{e.Tip}] {e.Poruka}");

            string warningLog = ConfigurationManager.AppSettings["WarningLogPath"] ?? "warnings.log";

            servis.OnWarningRaised += (s, e) => File.AppendAllText(warningLog, $"{DateTime.Now:o} [{e.Tip}] {e.Poruka}{Environment.NewLine}");

            servis.OnTempSpikeDetected += (s, e) => mojListener.OnTempSpike(s, e);

            ServiceHost host = new ServiceHost(servis); // prosledjujemo istu instancu ne pravimo non stop novu 
            host.Open();
            Console.WriteLine("Servis pokrenut. Cekam konekciju...");
            Console.ReadLine(); 
            host.Close();
        }
    }
}
