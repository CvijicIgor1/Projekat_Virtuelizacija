using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceModel;

namespace Service
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,ConcurrencyMode = ConcurrencyMode.Single)]
    public class BatteryService : IBatteryService
    {
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

            if (double.IsNaN(sample.R_ohm) || double.IsInfinity(sample.R_ohm))
            {
                var fault = new DataFormatFault(
                    poruka: $"R_ohm nije realna vrednost (Row={sample.RowIndex}).",
                    polje: "R_ohm",
                    primljenaVrednost: sample.R_ohm.ToString());
                Console.WriteLine($"[FAULT:DataFormat] R_ohm={sample.R_ohm} nije realan broj.");
                throw new FaultException<DataFormatFault>(fault, fault.Poruka);
            }

            if (double.IsNaN(sample.T_degC) || double.IsInfinity(sample.T_degC))
            {
                var fault = new DataFormatFault(
                    poruka: $"T_degC nije realna vrednost (Row={sample.RowIndex}).",
                    polje: "T_degC",
                    primljenaVrednost: sample.T_degC.ToString());
                Console.WriteLine($"[FAULT:DataFormat] T_degC={sample.T_degC} nije realan broj.");
                throw new FaultException<DataFormatFault>(fault, fault.Poruka);
            }

            if (sample.FrequencyHz <= 0)
            {
                var fault = new ValidationFault(
                    poruka: $"FrequencyHz mora biti pozitivan (Row={sample.RowIndex}, primljeno: {sample.FrequencyHz}).",
                    pravilo: "FrequencyHzPozitivan",
                    ocekivanoOpisno: "FrequencyHz > 0",
                    primljenaVrednost: sample.FrequencyHz.ToString());
                Console.WriteLine($"[FAULT:Validation] FrequencyHz={sample.FrequencyHz} nije pozitivan.");
                throw new FaultException<ValidationFault>(fault, fault.Poruka);
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

            string status = (primljenoUzoraka >= aktivnaSesija.TotalRows) ? "COMPLETED" : "IN_PROGRESS";

            Console.WriteLine($"[ACK] Uzorak {sample.RowIndex,3} | " +
                              $"F={sample.FrequencyHz,10:F2} Hz | " +
                              $"R={sample.R_ohm,8:F5} Ω | " +
                              $"T={sample.T_degC,5:F1}°C | " +
                              $"Status: {status} ({primljenoUzoraka}/{aktivnaSesija.TotalRows})");

            return $"ACK: Uzorak {sample.RowIndex} prihvacen. Status: {status}";
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

            Console.WriteLine("=================================================");
            Console.WriteLine($"[ACK] Sesija zatvorena.");
            Console.WriteLine($"Baterija  : {batteryId} / {testId} / SoC={soc}%");
            Console.WriteLine($"Primljeno : {primljeno} od {ocekivano} uzoraka");
            Console.WriteLine("=================================================");

            return $"ACK: Sesija zatvorena. Status: COMPLETED. Primljeno uzoraka: {primljeno}/{ocekivano}.";
        }

        public string Ping()
        {
            Console.WriteLine("[Ping] Primljen ping od klijenta.");
            return "POVEZAN";
        }
    }
}
