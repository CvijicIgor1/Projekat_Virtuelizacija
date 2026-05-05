using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
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
            string[] delovi = csvFile.Split(Path.DirectorySeparatorChar);
            string batteryId = delovi[delovi.Length - 5];  //B01
            string testId = delovi[delovi.Length - 3];  //Test_1
            string fileName = Path.GetFileName(csvFile);
            string[] podaci = fileName.Split('_');
            double soc = double.Parse(podaci[3], CultureInfo.InvariantCulture);
            int totalRows = File.ReadAllLines(csvFile).Length - 1;
            return new EisMeta(batteryId, testId, soc, fileName, totalRows);
        }
    }
}
