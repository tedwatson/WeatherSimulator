using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    class G_Location
    {
        public G_Results[] results;

        public G_Location()
        {
            //results = new G_Results();
        }
    }

    class G_Results
    {
        public string formatted_address;

        public G_Results()
        {
            formatted_address = null;
        } 
    }
}
