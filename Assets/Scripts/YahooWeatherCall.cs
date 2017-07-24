using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    class YahooWeatherCall
    {
        public Query query { get; set; }

        public YahooWeatherCall()
        {
            query = new Query();
        }
    }

    class Query
    {
        public int count;
        public string created;
        public string lang;
        public Y_Results results;

        public Query()
        {
            count = -1;
            created = null;
            lang = null;
            results = new Y_Results();
        }
    }

    class Y_Results
    {
        public Y_Channel channel;

        public Y_Results()
        {
            channel = new Y_Channel();
        }
    }

    class Y_Channel
    {
        public Y_Location location;
        public Y_Wind wind;
        public Y_Atmosphere atmosphere;
        public Y_Astronomy astronomy;
        public Y_Item item;

        public Y_Channel()
        {
            location = new Y_Location();
            wind = new Y_Wind();
            atmosphere = new Y_Atmosphere();
            astronomy = new Y_Astronomy();
            item = new Y_Item();
        }
    }

    class Y_Location
    {
        public string city;
        public string country;
        public string region;

        public Y_Location()
        {
            city = null;
            country = null;
            region = null;
        }
    }

    class Y_Wind
    {
        public string chill;
        public string direction;
        public string speed;

        public Y_Wind()
        {
            chill = null;
            direction = null;
            speed = null;
        }
    }

    class Y_Atmosphere
    {
        public string humidity;
        public string pressure;
        public string rising;
        public string visibility;

        public Y_Atmosphere()
        {
            humidity = null;
            pressure = null;
            rising = null;
            visibility = null;
        }
    }

    class Y_Astronomy
    {
        public string sunrise;
        public string sunset;

        public Y_Astronomy()
        {
            sunrise = null;
            sunset = null;
        }
    }

    class Y_Item
    {
        public string title;
        public Y_Condition condition;
        public Y_Forecast[] forecast;

        public Y_Item()
        {
            title = null;
            condition = new Y_Condition();
            forecast = new Y_Forecast[10]; // 10 day forecast 
        }

    }

    class Y_Condition
    {
        public string code;
        public string date;
        public string temp;
        public string text;

        public Y_Condition()
        {
            code = null;
            date = null;
            temp = null;
            text = null;
        }
    }

    class Y_Forecast
    {
        public string code;
        public string date;
        public string day;
        public string high;
        public string low;
        public string text;

        public Y_Forecast()
        {
            code = null;
            date = null;
            day = null;
            high = null;
            low = null;
            text = null;
        }
    }
}
