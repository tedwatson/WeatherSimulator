using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    class Hourly
    {
        public string summary { get; set; }
        public string icon { get; set; }
        public Data[] data { get; set; }

        public Hourly()
        {
            summary = null;
            icon = null;
            data = new Data[48]; // 48 Hours
            for (int h = 0; h < 48; h++)
            {
                data[h] = new Data();
            }
        }
    }
}
