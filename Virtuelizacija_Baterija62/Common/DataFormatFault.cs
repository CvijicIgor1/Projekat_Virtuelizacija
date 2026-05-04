using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common
{
    // Baca se kada podaci nisu u ispravnom formatu
    [DataContract]
    public class DataFormatFault
    {
        [DataMember]
        public string Poruka { get; set; } // Neka citljiva poruka, npr "Ne mogu da konvertujem R_ohm u double"

        [DataMember]
        public string Polje { get; set; }   // koje polje je problem, npr "R_ohm", "BatteryId"

        [DataMember]
        public string PrimljenaVrednost { get; set; } // a sta je klijent poslao, npr "abc" (ako je problem u konverziji), ili "nesto" (ako je problem u formatu datuma)    

        public DataFormatFault(string poruka, string polje, string primljenaVrednost)
        {
            Poruka = poruka;
            Polje = polje;
            PrimljenaVrednost = primljenaVrednost;
        }
    }
}