using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single, ConcurrencyMode = ConcurrencyMode.Single)]
    public class BatteryService : IBatteryService
    {
        private StreamWriter sessionWriter = null;
        private string sessionCsvPath = null;
        //  Stanje aktivne sesije
        private EisMeta aktivnaSesija = null;
        private int poslednjiRowIndex = -1;
        private int primljenoUzoraka = 0;
        private bool sesijaNijeZapoceta = true;


        public string StartSession(EisMeta meta)
        {
            if (meta == null)
            {
                var fault = new DataFormatFault(
                    poruka: "Meta-zaglavlje sesije je null.",
                    polje: "EisMeta",
                    primljenaVrednost: "null");
                Console.WriteLine("[FAULT:DataFormat] meta je null.");
                throw new FaultException<DataFormatFault>(fault, fault.Poruka);
            }

            if (string.IsNullOrWhiteSpace(meta.BatteryId))
            {
                var fault = new DataFormatFault(
                    poruka: "BatteryId ne sme biti prazan.",
                    polje: "BatteryId",
                    primljenaVrednost: meta.BatteryId ?? "null");
                Console.WriteLine("[FAULT:DataFormat] BatteryId je prazan.");
                throw new FaultException<DataFormatFault>(fault, fault.Poruka);
            }

            if (string.IsNullOrWhiteSpace(meta.TestId))
            {
                var fault = new DataFormatFault(
                    poruka: "TestId ne sme biti prazan.",
                    polje: "TestId",
                    primljenaVrednost: meta.TestId ?? "null");
                Console.WriteLine("[FAULT:DataFormat] TestId je prazan.");
                throw new FaultException<DataFormatFault>(fault, fault.Poruka);
            }

            if (meta.SoC < 0 || meta.SoC > 100)
            {
                var fault = new ValidationFault(
                    poruka: $"SoC vrednost {meta.SoC} nije u opsegu 0-100.",
                    pravilo: "SoCOpseg",
                    ocekivanoOpisno: "0 <= SoC <= 100",
                    primljenaVrednost: meta.SoC.ToString());
                Console.WriteLine($"[FAULT:Validation] SoC={meta.SoC} van opsega.");
                throw new FaultException<ValidationFault>(fault, fault.Poruka);
            }

            if (meta.TotalRows <= 0)
            {
                var fault = new ValidationFault(
                    poruka: $"TotalRows mora biti pozitivan broj, primljeno: {meta.TotalRows}.",
                    pravilo: "TotalRowsPozitivan",
                    ocekivanoOpisno: "TotalRows > 0",
                    primljenaVrednost: meta.TotalRows.ToString());
                Console.WriteLine($"[FAULT:Validation] TotalRows={meta.TotalRows} nije pozitivan.");
                throw new FaultException<ValidationFault>(fault, fault.Poruka);
            }

            if (!sesijaNijeZapoceta)
            {
                var fault = new ValidationFault(
                    poruka: $"Sesija je vec aktivna ({aktivnaSesija.BatteryId}/{aktivnaSesija.TestId}). Pozovi EndSession pre nove sesije.",
                    pravilo: "JednaSesijaIstovremeno",
                    ocekivanoOpisno: "Nema aktivne sesije",
                    primljenaVrednost: $"{aktivnaSesija.BatteryId}/{aktivnaSesija.TestId}");
                Console.WriteLine("[FAULT:Validation] Pokusaj otvaranja sesije dok je druga aktivna.");
                throw new FaultException<ValidationFault>(fault, fault.Poruka);
            }

            // Sve OK - prihvatamo sesiju
            aktivnaSesija = meta;
            poslednjiRowIndex = -1;
            primljenoUzoraka = 0;
            sesijaNijeZapoceta = false;
            KreirajLogFajlove(meta.BatteryId, meta.TestId, meta.SoC.ToString());

            string baseDir = ConfigurationManager.AppSettings["DataOutputPath"] ?? "Data";
            sessionCsvPath = Path.Combine(baseDir, meta.BatteryId, meta.TestId, meta.SoC + "%", "session.csv");
            Directory.CreateDirectory(Path.GetDirectoryName(sessionCsvPath));
            sessionWriter = new StreamWriter(sessionCsvPath, append: true);
            sessionWriter.WriteLine("RowIndex,FrequencyHz,R_ohm,X_ohm,T_degC,Range_ohm,Timestamp");

            Console.WriteLine("=================================================");
            Console.WriteLine($"[ACK] Sesija otvorena.");
            Console.WriteLine($"BatteryId : {meta.BatteryId}");
            Console.WriteLine($"TestId : {meta.TestId}");
            Console.WriteLine($"SoC : {meta.SoC}%");
            Console.WriteLine($"Fajl : {meta.FileName}");
            Console.WriteLine($"Ukupno red: {meta.TotalRows}");
            Console.WriteLine("=================================================");

            return "ACK: Sesija otvorena. Status: IN_PROGRESS";
        }

        private void KreirajLogFajlove(string batteryId, string testId, string SoC)
        {
            (string sessionPutanja, string errorPutanja) = DobijPutanjeLogFajlova(batteryId, testId, SoC);

            string folder = System.IO.Path.GetDirectoryName(sessionPutanja);
            System.IO.Directory.CreateDirectory(folder); // pravi sve podfoldere

            System.IO.File.Create(sessionPutanja).Close();
            System.IO.File.Create(errorPutanja).Close();
        }

        public string PushSample(EisSample sample)
        {
            if (sesijaNijeZapoceta || aktivnaSesija == null)
            {
                var fault = new ValidationFault(
                    poruka: "Nema aktivne sesije. Pozovi StartSession pre slanja uzoraka.",
                    pravilo: "SesijaAktivna",
                    ocekivanoOpisno: "Sesija mora biti otvorena",
                    primljenaVrednost: "nema sesije");
                Console.WriteLine("[FAULT:Validation] PushSample bez aktivne sesije.");
                throw new FaultException<ValidationFault>(fault, fault.Poruka);
            }

            if (sample == null)
            {
                var fault = new DataFormatFault(
                    poruka: "Uzorak (EisSample) je null.",
                    polje: "EisSample",
                    primljenaVrednost: "null");
                Console.WriteLine("[FAULT:DataFormat] sample je null.");
                throw new FaultException<DataFormatFault>(fault, fault.Poruka);
            }

            (string sessionPutanja, string errorPutanja) = DobijPutanjeLogFajlova(aktivnaSesija.BatteryId, aktivnaSesija.TestId, aktivnaSesija.SoC.ToString());

            if (double.IsNaN(sample.R_ohm) || double.IsInfinity(sample.R_ohm))
            {
                var fault = new DataFormatFault(
                    poruka: $"R_ohm nije realna vrednost (Row={sample.RowIndex}).",
                    polje: "R_ohm",
                    primljenaVrednost: sample.R_ohm.ToString());
                Console.WriteLine($"[FAULT:DataFormat] R_ohm={sample.R_ohm} nije realan broj.");
                // throw new FaultException<DataFormatFault>(fault, fault.Poruka);
                File.AppendAllText(errorPutanja, $"Red: {sample.RowIndex}, Vreme: {DateTime.Now}, Razlog: Pogresan format R_ohm !\n");
                return $"[NACK]: Uzorak {sample.RowIndex} odbacen. Razlog: Pogresan format R_ohm !";
            }

            if (double.IsNaN(sample.T_degC) || double.IsInfinity(sample.T_degC))
            {
                var fault = new DataFormatFault(
                    poruka: $"T_degC nije realna vrednost (Row={sample.RowIndex}).",
                    polje: "T_degC",
                    primljenaVrednost: sample.T_degC.ToString());
                Console.WriteLine($"[FAULT:DataFormat] T_degC={sample.T_degC} nije realan broj.");
                //throw new FaultException<DataFormatFault>(fault, fault.Poruka);
                File.AppendAllText(errorPutanja, $"Red: {sample.RowIndex}, Vreme: {DateTime.Now}, Razlog: Pogresan format T !\n");
                return $"[NACK]: Uzorak {sample.RowIndex} odbacen. Razlog: Pogresan format T !";
            }

            if (sample.FrequencyHz <= 0)
            {
                var fault = new ValidationFault(
                    poruka: $"FrequencyHz mora biti pozitivan (Row={sample.RowIndex}, primljeno: {sample.FrequencyHz}).",
                    pravilo: "FrequencyHzPozitivan",
                    ocekivanoOpisno: "FrequencyHz > 0",
                    primljenaVrednost: sample.FrequencyHz.ToString());
                Console.WriteLine($"[FAULT:Validation] FrequencyHz={sample.FrequencyHz} nije pozitivan.");
                //throw new FaultException<ValidationFault>(fault, fault.Poruka);
                File.AppendAllText(errorPutanja, $"Red: {sample.RowIndex}, Vreme: {DateTime.Now}, Razlog: Frequency <=0 !\n");
                return $"[NACK]: Uzorak {sample.RowIndex} odbacen. Razlog: Frequency <=0 !";
            }

            if (sample.RowIndex <= poslednjiRowIndex)
            {
                var fault = new ValidationFault(
                    poruka: $"RowIndex mora monotono rasti. Poslednji: {poslednjiRowIndex}, novi: {sample.RowIndex}.",
                    pravilo: "MonotoniRowIndex",
                    ocekivanoOpisno: $"RowIndex > {poslednjiRowIndex}",
                    primljenaVrednost: sample.RowIndex.ToString());
                Console.WriteLine($"[FAULT:Validation] RowIndex={sample.RowIndex} ne raste monotono (poslednji={poslednjiRowIndex}).");
                throw new FaultException<ValidationFault>(fault, fault.Poruka);
            }

            poslednjiRowIndex = sample.RowIndex;
            primljenoUzoraka++;

            sessionWriter?.WriteLine($"{sample.RowIndex},{sample.FrequencyHz},{sample.R_ohm}," + $"{sample.X_ohm},{sample.T_degC},{sample.Range_ohm}," + $"{sample.TimestampLocal:o}");
            sessionWriter?.Flush();
            Console.WriteLine($"[STREAMING] Prenos u toku... ({primljenoUzoraka}/{aktivnaSesija.TotalRows})");

            string status = (primljenoUzoraka >= aktivnaSesija.TotalRows) ? "COMPLETED" : "IN_PROGRESS";

            string ACKLine = $"[ACK] Uzorak {sample.RowIndex,3}| " +
                              $"F={sample.FrequencyHz:F2}Hz | " +
                              $"R={sample.R_ohm:F5}Ω | " +
                              $"T={sample.T_degC:F1}°C | " +
                              $"Status: {status} ({primljenoUzoraka}/{aktivnaSesija.TotalRows})";

            Console.WriteLine(ACKLine);

            File.AppendAllText(sessionPutanja, ACKLine + "\n");
            return $"ACK: Uzorak {sample.RowIndex} prihvacen. Status: {status}";
        }

        public (string, string) DobijPutanjeLogFajlova(string batteryId, string testId, string SoC)
        {
            string logPath = ConfigurationManager.AppSettings["ServiceDataLogPath"];
            string addOn = $"/{batteryId}/{testId}/{SoC}";  //pravim punu putanju ovde

            string sessionLogFile = $"{logPath}/{addOn}/session.csv";
            string errorLogFile = $"{logPath}/{addOn}/rejects.txt";

            return (sessionLogFile, errorLogFile);
        }

        public string EndSession()
        {
            if (sesijaNijeZapoceta || aktivnaSesija == null)
            {
                var fault = new ValidationFault(
                    poruka: "Nema aktivne sesije koju treba zatvoriti.",
                    pravilo: "SesijaAktivna",
                    ocekivanoOpisno: "Sesija mora biti otvorena",
                    primljenaVrednost: "nema sesije");
                Console.WriteLine("[FAULT:Validation] EndSession bez aktivne sesije.");
                throw new FaultException<ValidationFault>(fault, fault.Poruka);
            }

            string batteryId = aktivnaSesija.BatteryId;
            string testId = aktivnaSesija.TestId;
            double soc = aktivnaSesija.SoC;
            int ocekivano = aktivnaSesija.TotalRows;
            int primljeno = primljenoUzoraka;

            // Resetujemo stanje
            aktivnaSesija = null;
            poslednjiRowIndex = -1;
            primljenoUzoraka = 0;
            sesijaNijeZapoceta = true;

            sessionWriter?.Close();
            sessionWriter = null;

            Console.WriteLine("=================================================");
            Console.WriteLine($"[ACK] Sesija zatvorena.");
            Console.WriteLine($"Baterija  : {batteryId} / {testId} / SoC={soc}%");
            Console.WriteLine($"Primljeno : {primljeno} od {ocekivano} uzoraka");
            Console.WriteLine("=================================================");
            Console.WriteLine("[STREAMING] Prenos završen.");

            return $"ACK: Sesija zatvorena. Status: COMPLETED. Primljeno uzoraka: {primljeno}/{ocekivano}.";
        }

        public string Ping()
        {
            Console.WriteLine("[Ping] Primljen ping od klijenta.");
            return "POVEZAN";
        }

        public FileManipulationResults SendFile(FileManipulationOptions options) //za KT2, nisam hteo da brisem
        {
            throw new NotImplementedException();
        }

        public FileManipulationResults GetFiles(FileManipulationOptions options) //za KT2, nisam hteo da brisem
        {
            throw new NotImplementedException();
        }
    }
}
