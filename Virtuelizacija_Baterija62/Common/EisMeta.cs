using System;
using System.Collections.Generic;
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
    }
}
