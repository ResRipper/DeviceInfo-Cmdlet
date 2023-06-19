using System;
using System.Globalization;
using System.Linq;

namespace DeviceInfo.tools {

    internal class Tools {

        public double highest_num(string data) {
            // Return highest value as Double in string like ['1,0', '1.1']
            string[] strlist = data.Split(',');
            double[] floatlist = new double[strlist.Length];
            for (int i = 0; i < strlist.Length; i++) {
                floatlist[i] = Convert.ToDouble(strlist[i].Trim());
            }
            return floatlist.Max();
        }

        public DateTime str_date(string data) {
            // Convert time from WMI i.e. 20230401092040.000000+480 to Datetime
            data = data.Split('.')[0];
            return DateTime.ParseExact(data, "yyyyMMddHHmmss", CultureInfo.InvariantCulture);
        }
    }
}