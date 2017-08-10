using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    class Currently
    {
        public int time { get; set; }
        public string summary { get; set; }
        public string icon { get; set; }
        public float nearestStormDistance { get; set; }
        public float precipIntensity { get; set; }
        public float precipIntensityError { get; set; }
        public float precipProbability { get; set; }
        public string precipType { get; set; }
        public float temperature { get; set; }
        public float apparentTemperature { get; set; }
        public float dewPoint { get; set; }
        public float humidity { get; set; }
        public float windSpeed { get; set; }
        public float windGust { get; set; }
        public float windBearing { get; set; }
        public float visibility { get; set; }
        public float cloudCover { get; set; }
        public float pressure { get; set; }
        public float ozone { get; set; }

        public Currently()
        {
            time = -1;
            summary = null;
            icon = null;
            nearestStormDistance = float.NaN;
            precipIntensity = float.NaN;
            precipIntensityError = float.NaN;
            precipProbability = float.NaN;
            precipType = null;
            temperature = float.NaN;
            apparentTemperature = float.NaN;
            dewPoint = float.NaN;
            humidity = float.NaN;
            windSpeed = float.NaN;
            windGust = float.NaN;
            windBearing = float.NaN;
            visibility = float.NaN;
            cloudCover = float.NaN;
            pressure = float.NaN;
            ozone = float.NaN;
        }
    }
}
