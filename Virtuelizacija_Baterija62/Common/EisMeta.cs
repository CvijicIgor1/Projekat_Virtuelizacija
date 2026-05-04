using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class EisMeta
    {
        [DataMember] public string BatteryId { get; set; }    
        [DataMember] public string TestId { get; set; }      
        [DataMember] public double SoC { get; set; }           // SoC je state of charge, dakkle napunjenost
        [DataMember] public string FileName { get; set; }     
        [DataMember] public int TotalRows { get; set; }

        public EisMeta(string batteryId, string testId, double soc, string fileName, int totalRows)
        {
            BatteryId = batteryId;
            TestId = testId;
            SoC = soc;
            FileName = fileName;
            TotalRows = totalRows;
        }

        public static EisMeta EkstraktujMetaPodatke(string csvFile)
        {
            string[] delovi = csvFile.Split(Path.DirectorySeparatorChar);  //bice isto za sve jer radimo samo Hioki
            string batteryId = delovi[delovi.Length - 5]; // "B01"
            string testId = delovi[delovi.Length - 3];     // "Test_1"


            string fileName = Path.GetFileName(csvFile);
            string[] podaci = fileName.Split('_');
            double soc = Convert.ToDouble(podaci[3]);
            int totalRows = File.ReadAllLines(csvFile).Length - 1; //ima ih 29, ali je prvi zaglavlje. Generalno je 28 svuda, ali ovako je sigurnije, ako budemo morali da prosirimo dataset
            return new EisMeta(batteryId, testId, soc, fileName, totalRows);
        }
    }
}
