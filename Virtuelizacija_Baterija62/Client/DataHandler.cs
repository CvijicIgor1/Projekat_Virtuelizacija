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
        public static List<string> CSVProlaz()
        {
            string dataSetPutanja = ConfigurationManager.AppSettings["DataSetPath"];
            dataSetPutanja = Path.GetFullPath(dataSetPutanja);
            string[] sviCsvFajlovi = Directory.GetFiles(dataSetPutanja, "*.csv", SearchOption.AllDirectories);

            foreach (string csvFile in sviCsvFajlovi)  //samo prolazim kroz sve, ne saljem nigde za sada
            {
                List<string> redovi = new List<string> { csvFile }; //red 0 ce biti naslov falja, jer su tu metapodaci
                string[] linijeUFajlu = File.ReadAllLines(csvFile);
                int rowIndex = 0;
                foreach(string linija in linijeUFajlu)
                {
                    string SpremnaLinija = String.Copy(linija);
                    SpremnaLinija = rowIndex + "," + linija; //dodajem redni broj reda, jer ce mi trebati za RowIndex u EisSample
                    redovi.Add(SpremnaLinija);
                }

                return redovi;
            }

            return null;
        }
        
        public static void SendFiles(IBatteryService proxy)
        {
            List<String> redovi = CSVProlaz();

            if (redovi != null)
            {
                //ovde sam stao
            }
            else
            {
                Console.WriteLine("Greska pri citanju CSV!");
            }
        }
    }
}
