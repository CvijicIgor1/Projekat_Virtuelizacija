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
            string batteryId = ConfigurationManager.AppSettings["BatteryId"];  //nasa baza je Dataset → B01 → EIS measurements → Test_1 → Hioki, pa se 
            string testId = ConfigurationManager.AppSettings["TestId"];        // testId i batteryId nalaze u app.config, jer su isti za sve fajlove. Ne mogu iz naziva da ih vadim jer sam skinuo samo deo dataseta
                                                                               //koji nam treba za projekat, zbog git-a (pitacu na konsultacijama)

            string fileName = Path.GetFileName(csvFile);
            string[] podaci = fileName.Split('_');
            double soc = Convert.ToDouble(podaci[3]);

            return new EisMeta(batteryId, testId, soc, fileName, 28);
        }
    }
}
