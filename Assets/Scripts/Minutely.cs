using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNamespace
{
    class Minutely
    {
        public string summary { get; set; }
        public string icon { get; set; }
        public Data[] data { get; set; }

        public Minutely()
        {
            summary = null;
            icon = null;
            data = new Data[60]; // 60 Minutes
            for (int m = 0; m < 60; m++)
            {
                data[m] = new Data();
            }
        }

    }
}
