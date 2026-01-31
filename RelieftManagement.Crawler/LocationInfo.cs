using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelieftManagement.Crawler
{
    public class LocationInfo   
    {
        public string Province { get; set; }
        public string Ward { get; set; }
        public double Area { get; set; }
        public long Population { get; set; }

        public double Density => Area > 0 ? Population / Area : 0;

        public override string ToString()
        {
            return $"Tỉnh: {Province} | Xã: {Ward} | DT: {Area} km² | DS: {Population} | MĐ: {Density:N2}";
        }
    }
}
