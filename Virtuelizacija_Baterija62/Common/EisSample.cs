using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class EisSample
    {
        [DataMember] public int RowIndex { get; set; }
        [DataMember] public double FrequencyHz { get; set; }
        [DataMember] public double R_ohm { get; set; }        //impedansa realna
        [DataMember] public double X_ohm { get; set; }        //impedansa imaginarna
        [DataMember] public double T_degC { get; set; }
        [DataMember] public double Range_ohm { get; set; }
        [DataMember] public DateTime TimestampLocal { get; set; }

        public EisSample(int rowIndex, double frequencyHz, double r_ohm, double x_ohm, double t_degC, double range_ohm, DateTime timestampLocal)
        {
            RowIndex = rowIndex;
            FrequencyHz = frequencyHz;
            R_ohm = r_ohm;
            X_ohm = x_ohm;
            T_degC = t_degC;
            Range_ohm = range_ohm;
            TimestampLocal = timestampLocal;
        }

        public static EisSample EkstraktujSamplePodatke(string redIzFajla)
        {
            string[] podaci = redIzFajla.Split(',');

            int rowIndex = Convert.ToInt32(podaci[0]);
            double frequencyHz = Convert.ToDouble(podaci[1]);
            double r_ohm = Convert.ToDouble(podaci[2]);
            double x_ohm = Convert.ToDouble(podaci[3]);
            //4 se ne korisi u specifikaciji
            double t_degC = Convert.ToDouble(podaci[5]);
            double range_ohm = Convert.ToDouble(podaci[6]);
            DateTime timestampLocal = DateTime.Now; 

            return new EisSample(rowIndex, frequencyHz, r_ohm, x_ohm, t_degC, range_ohm, timestampLocal);
        }
    }
}
