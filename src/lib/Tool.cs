using System;
using System.Globalization;
using System.Linq;

namespace DeviceInfo.Tools {

    internal class Tool {
        public double HighestNum(string data) {
            // Return highest value as Double in string like ['1.0', '1.1']
            string[] strlist = data.Split(',');
            double[] floatlist = new double[strlist.Length];
            for (int i = 0; i < strlist.Length; i++) {
                floatlist[i] = Convert.ToDouble(strlist[i].Trim());
            }
            return floatlist.Max();
        }

        public DateTime StrToDate(string data) {
            // Convert time from WMI i.e. 20230401092040.000000+480 to Datetime
            if (data == null || data.Length == 0) return DateTime.MinValue;
            data = data.Split('.')[0];
            return DateTime.ParseExact(data, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }
    }
}