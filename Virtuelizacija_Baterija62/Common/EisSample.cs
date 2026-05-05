using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
            int rowIndex = int.Parse(podaci[0]);
            double frequencyHz = double.Parse(podaci[1], CultureInfo.InvariantCulture);
            double r_ohm = double.Parse(podaci[2], CultureInfo.InvariantCulture);
            double x_ohm = double.Parse(podaci[3], CultureInfo.InvariantCulture);
            double t_degC = double.Parse(podaci[5], CultureInfo.InvariantCulture);
            double range_ohm = double.Parse(podaci[6], CultureInfo.InvariantCulture);
            DateTime timestampLocal = DateTime.Now;

            if (rowIndex < 0 || frequencyHz < 0 || r_ohm < 0 || x_ohm < 0 || t_degC < 0 || range_ohm < 0)
            {
                File.AppendAllText("greske_log.txt", $"Nevalidan red!\nRed: {rowIndex}, Sadrzaj: {redIzFajla}{Environment.NewLine}");
            }

            return new EisSample(rowIndex, frequencyHz, r_ohm, x_ohm, t_degC, range_ohm, timestampLocal);
        }
    }
}
