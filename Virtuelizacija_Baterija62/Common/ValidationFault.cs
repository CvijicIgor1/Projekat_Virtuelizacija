using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace Common
{
    [DataContract]
    public class ValidationFault
    {
        [DataMember]
        public string Poruka { get; set; } // Neka citljiva poruka

        [DataMember]
        public string Pravilo { get; set; }     // Kratko ime pravila koje je prekršeno

        [DataMember]
        public string OcekivanoOpisno { get; set; }  // Sta je server ocekivao

        [DataMember]
        public string PrimljenaVrednost { get; set; } // a sta je klijent poslao

        public ValidationFault(string poruka, string pravilo, string ocekivanoOpisno, string primljenaVrednost)
        {
            Poruka = poruka;
            Pravilo = pravilo;
            OcekivanoOpisno = ocekivanoOpisno;
            PrimljenaVrednost = primljenaVrednost;
        }
    }
}

