using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyNamespace
{
    class Data
    {
        public int time { get; set; }
        public string summary { get; set; }
        public string icon { get; set; }
        public int sunriseTime { get; set; }
        public int sunsetTime { get; set; }
        public float moonPhase { get; set; }
        public float precipIntensity { get; set; }
        public float precipIntensityMax { get; set; }
        public int precipIntensityMaxTime { get; set; }
        public float precipIntensityError { get; set; }
        public float precipProbability { get; set; }
        public string precipType { get; set; }
        public float temperature { get; set; }
        public float temperatureMin { get; set; }
        public int temperatureMinTime { get; set; }
        public float temperatureMax { get; set; }
        public int temperatureMaxTime { get; set; }
        public float apparentTemperatureMin { get; set; }
        public int apparentTemperatureMinTime { get; set; }
        public float apparentTemperatureMax { get; set; }
        public int apparentTemperatureMaxTime { get; set; }
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
        public bool foggy;

        public Data()
        {
            time = -1;
            summary = null;
            icon = null;
            sunriseTime = -1;
            sunsetTime = -1;
            moonPhase = float.NaN;
            precipIntensity = float.NaN;
            precipIntensityMax = float.NaN;
            precipIntensityMaxTime = -1;
            precipIntensityError = float.NaN;
            precipProbability = float.NaN;
            precipType = null;
            temperature = float.NaN;
            temperatureMin = float.NaN;
            temperatureMinTime = -1;
            temperatureMax = float.NaN;
            temperatureMaxTime = -1;
            apparentTemperatureMin = float.NaN;
            apparentTemperatureMinTime = -1;
            apparentTemperatureMax = float.NaN;
            apparentTemperatureMaxTime = -1;
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
            foggy = false;
        }
    }
}
