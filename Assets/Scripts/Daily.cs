using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    class Daily
    {
        public string summary { get; set; }
        public string icon { get; set; }
        public Data[] data { get; set; }

        public Daily()
        {
            summary = null;
            icon = null;
            data = new Data[7]; // 7 Days
            for (int d = 0; d < 7; d++)
            {
                data[d] = new Data();
            }
        }
    }
}
