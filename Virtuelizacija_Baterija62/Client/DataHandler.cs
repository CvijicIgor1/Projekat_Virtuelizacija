using Common;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client
{
    public class DataHandler
    {
        public static (List<string>, EisMeta) CSVProlaz()
        {
            string dataSetPutanja = ConfigurationManager.AppSettings["DataSetPath"];
            dataSetPutanja = Path.GetFullPath(dataSetPutanja);
            string[] sviCsvFajlovi = Directory.GetFiles(dataSetPutanja, "*.csv", SearchOption.AllDirectories);

            foreach (string csvFile in sviCsvFajlovi)  //samo prolazim kroz sve, ne saljem nigde za sada
            {
                //meta uzorak
                string fileName = Path.GetFileName(csvFile); // "Hk_IFR14500_SoC_5_04-07-2023_05-13.csv"
                EisMeta meta = EisMeta.EkstraktujMetaPodatke(csvFile); //izvucem metapodatke iz csv fajla, koji su u prvom redu


                //ostali redovi (bez zaglavlja naravno)
                List<string> redovi = new List<string>(); //red 0 ce biti naslov falja, jer su tu metapodaci
                string[] linijeUFajlu = File.ReadAllLines(csvFile);
                int rowIndex = 0;
                for (int i = 1; i < linijeUFajlu.Length; i++) //prvu liniju ne citam, to je heder
                {
                    string SpremnaLinija = String.Copy(linijeUFajlu[i]);
                    SpremnaLinija = rowIndex + "," + linijeUFajlu[i]; //dodajem redni broj reda, jer ce mi trebati za RowIndex u EisSample
                    redovi.Add(SpremnaLinija);
                    rowIndex++;
                }

                return (redovi, meta);
            }

            return (null, null);
        }

        public static void SendFiles(IBatteryService proxy)
        {
            (List<String> redovi, EisMeta meta) = CSVProlaz();

            if (redovi != null)
            {
                proxy.StartSession(meta);
                foreach (string red in redovi)
                {
                    EisSample s = EisSample.EkstraktujSamplePodatke(red);
                    string response = proxy.PushSample(s);
                }
                proxy.EndSession();
            }
            else
            {
                Console.WriteLine("Greska pri citanju CSV!");
            }
        }

        public static void SendFilesSimulacijaPrekida(IBatteryService proxy)
        {
            string dataSetPutanja = ConfigurationManager.AppSettings["DataSetPath"];
            dataSetPutanja = Path.GetFullPath(dataSetPutanja);
            string[] sviCsvFajlovi = Directory.GetFiles(dataSetPutanja, "*.csv", SearchOption.AllDirectories);

            if (sviCsvFajlovi.Length == 0) return;

            string csvFile = sviCsvFajlovi[0];
            EisMeta meta = EisMeta.EkstraktujMetaPodatke(csvFile);

            // Koristimo CsvReader koji implementira IDisposable
            using (CsvReader reader = new CsvReader(csvFile))
            {
                proxy.StartSession(meta);
                Console.WriteLine("[Simulacija] Pocetak prenosa...");

                int rowIndex = 0;
                bool prvaLinija = true;

                while (!reader.KrajFajla())
                {
                    string linija = reader.CitajSledeciRed();

                    if (prvaLinija) { prvaLinija = false; continue; } // preskoci header

                    // SIMULACIJA PREKIDA: posle 5. reda "pukne veza"
                    if (rowIndex == 5)
                    {
                        Console.WriteLine("[Simulacija] *** PREKID VEZE usred prenosa! ***");
                        // using ce automatski pozvati Dispose() i zatvoriti FileStream/StreamReader
                        // cak i kad dodje do izuzetka
                        throw new Exception("Simulirani prekid mrezne veze.");
                    }

                    try
                    {
                        string red = rowIndex + "," + linija;
                        EisSample s = EisSample.EkstraktujSamplePodatke(red);
                        proxy.PushSample(s);
                        rowIndex++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[GRESKA] Red {rowIndex}: {ex.Message}");
                    }
                }

                proxy.EndSession();
            }
            // Ovde je using automatski pozvao Dispose() - fajl je zatvoren
        }
    }
}
