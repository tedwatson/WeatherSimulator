using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNamespace
{
    class DarkSkyCall
    {
        public float latitude { get; set; }
        public float longitude { get; set; }
        public string timezone { get; set; }
        public Currently currently { get; set; }
        public Minutely minutely { get; set; }
        public Hourly hourly { get; set; }
        public Daily daily { get; set; }

        public DarkSkyCall()
        {
            latitude = float.NaN;
            longitude = float.NaN;
            timezone = null;
            currently = new Currently();
            minutely = new Minutely();
            hourly = new Hourly();
            daily = new Daily();
        }
    }
}
