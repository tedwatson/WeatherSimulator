using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using Newtonsoft.Json;
using MyNamespace;
using System.Net;
using Assets.Scripts;
using System;
using System.IO;
using DigitalRuby.WeatherMaker;


public class Main : MonoBehaviour {

    public GameObject japaneseStreetLamp;
    public Light lampPointLight;
    public GameObject weatherMakerPrefab;
    public GameObject world;

    // Manual Weather Overrides for debugging
    public bool printDebug = false;
    public bool manualWeatherOverride = false;
    public bool gentleSnow = false;
    public bool snowStorm = false;
    public bool thunderstorm = false;
    public bool clearDay = false;
    public bool hail = false;
    public bool sleet = false;


    public float cloudSpeedMultiplier = 1f;
    public bool cloudsSpeedUp = true;
    public float stillSeconds = 5f;
    public float secondsPerMinute = 1f / 10f;
    public float secondsPerHour = 1f / 4f;
    public float secondsPerHay = 3f;

    private bool manualLocation;
    private string manualLatitude;
    private string manualLongitude;

    public WeatherMakerScript weatherMaker;
    public WeatherMakerDayNightCycleScript dayNightCycle;
    public WeatherMakerThunderAndLightningScript thunderAndLightning;
    public WeatherMakerFullScreenFogScript fullScreenFog;
    public WeatherMakerWindScript wind;
    public WeatherMakerSkySphereScript skySphere;

    static double ipStoreMinutes = 60;
    static double locationStoreMinutes = 60;
    static double weatherStoreMinutes = 10;
    static double globalDeleteMinutes = 60 * 24;

    private bool isFirstPass = true;
    private Coroutine cloudChangeCoroutine;
    private Coroutine cloudVelocityCoroutine_x;
    private Coroutine cloudVelocityCoroutine_y;
    private DateTime realStartTime;
    private DateTime startTime;
    private float e = 2.7182818284590452353602874713527f;
    private bool debugMode = false;
    private string debugLat = "35.8569";
    private string debugLon = "139.6489";

    private int DestinationUtcOffsetInSeconds;
    private int ComputerUtcOffsetInSeconds;

    private string ipUrl = "https://api.ipify.org/?format=json";
    private string locationUrl = "http://freegeoip.net/json/";
    private string timeZoneDbUrl = "http://api.timezonedb.com/v2/get-time-zone?key=VZHJKBZ3QFZB&format=json&by=zone&zone=";
    private string weatherUrl = "https://api.darksky.net/forecast/1c6c6640edb8328a5a51501fbeaeed55/";
    private string googleGeocodeUrlFront = "http://maps.googleapis.com/maps/api/geocode/json?latlng=";
    private string googleGeocodeUrlBack = "&sensor=true";
    private string yahooWeatherApiUrlFront = "https://query.yahooapis.com/v1/public/yql?q=select%20*%20from%20weather.forecast%20where%20woeid%20in%20(SELECT%20woeid%20FROM%20geo.places%20WHERE%20text%3D%22(";
    private string yahooWeatherApiUrlBack = ")%22)&format=json&diagnostics=true&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys&callback=";

    private Location myLocation;
    private G_Location myG_Location;
    private TimeZoneDbCall myTimeZoneDbCall;
    private DarkSkyCall myDarkSkyCall;
    private YahooWeatherCall myYahooWeatherCall;
    private string myIP;
    //private static string resourcePath = "Assets/Resources/";

    private static string folderPath;

    private Color lampOffColor;
    private Color lampOnColor;
    private Renderer lampRenderer;
    private bool lampOn;
    float sunriseTime;
    float sunsetTime;

    // Use this for initialization
    void Start() {
        lampOffColor = new Color(0.483f, 0.475162f, 0.3693529f);
        lampOnColor = new Color(1.21f, 1.190365f, 0.9252941f);
        lampRenderer = japaneseStreetLamp.GetComponent<Renderer>();
        //Material material = renderer.
        //renderer.material.shader = Shader.Find("Emission");
        //lampRenderer.material.SetColor("_EmissionColor", lampNightColor);
        


        // Turn off world and weather while APIs load
        world.SetActive(false);
        //weatherMakerPrefab.SetActive(false);
        



        if (PlayerPrefs.GetInt("isManualLocation") == 1)
        {
            if (printDebug) print("isManualLocation = 1");
            manualLocation = true;
            manualLatitude = PlayerPrefs.GetFloat("Latitude").ToString();
            manualLongitude = PlayerPrefs.GetFloat("Longitude").ToString();
        }
        else manualLocation = false;

        incrementCloudCover = false;
        decrementCloudCover = false;
        incrementCloudVelocity_x = false;
        decrementCloudVelocity_x = false;
        incrementCloudVelocity_y = false;
        decrementCloudVelocity_y = false;
        //dayNightCycle.TimeZoneOffsetSeconds = Convert.ToInt32((DateTime.UtcNow - DateTime.Now).TotalSeconds); // 32400 is correct

        //Time.timeScale = 10;
        folderPath = (Application.platform == RuntimePlatform.Android ||
                        Application.platform == RuntimePlatform.IPhonePlayer ?
                        Application.persistentDataPath :
                        Application.dataPath)
                        + "/Resources/";


        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        StartCoroutine(GetAPIData());

        //dayNightCycleScript = dayNightCycle.GetComponent<WeatherMakerDayNightCycleScript>();
    }

    private bool incrementCloudCover;
    private bool decrementCloudCover;
    private bool incrementCloudVelocity_x;
    private bool incrementCloudVelocity_y;
    private bool decrementCloudVelocity_x;
    private bool decrementCloudVelocity_y;
    public float cloudCoverChangeSpeed = 1f;
    public float cloudVelocityChangeSpeed = 1f;
    private float baseCloudVelocity_x;
    private float baseCloudVelocity_y;

    private void FixedUpdate()
    {
        if      (incrementCloudCover) skySphere.CloudCover += cloudCoverChangeSpeed * Time.deltaTime;
        else if (decrementCloudCover) skySphere.CloudCover -= cloudCoverChangeSpeed * Time.deltaTime;

        if      (incrementCloudVelocity_x) skySphere.CloudNoiseVelocity.x += cloudVelocityChangeSpeed * Time.deltaTime;
        else if (decrementCloudVelocity_x) skySphere.CloudNoiseVelocity.x -= cloudVelocityChangeSpeed * Time.deltaTime;

        if      (incrementCloudVelocity_y) skySphere.CloudNoiseVelocity.y += cloudVelocityChangeSpeed * Time.deltaTime;
        else if (decrementCloudVelocity_y) skySphere.CloudNoiseVelocity.y -= cloudVelocityChangeSpeed * Time.deltaTime;


        float currentTime = dayNightCycle.TimeOfDay;
        if (sunriseTime != -1 && sunriseTime != 0 && sunsetTime != -1 && sunsetTime != 0)
        {
            if ((currentTime < sunriseTime || currentTime > sunsetTime) && !lampOn)
            {
                // Turn lamp on
                print("turning lamp on");
                lampRenderer.material.SetColor("_EmissionColor", lampOnColor);
                lampPointLight.enabled = true;
                lampOn = true;
            }
            else if (currentTime > sunriseTime && currentTime < sunsetTime && lampOn)
            {
                // Turn lamp off
                print("turning lamp off");
                lampRenderer.material.SetColor("_EmissionColor", lampOffColor);
                lampPointLight.enabled = false;
                lampOn = false;
            }
        }




        /*
        skySphere.CloudNoiseVelocity.x = baseCloudVelocity_x * dayNightCycle.Speed * cloudSpeedMultiplier;
        skySphere.CloudNoiseVelocity.y = baseCloudVelocity_y * dayNightCycle.Speed * cloudSpeedMultiplier;
        */
    }


    IEnumerator GetAPIData()
    {
        string myLat;
        string myLon;

        DeleteOldFiles();


        // Get Public IP
        if (!hasFile("ip"))
        {
            // Retrieve IP from API
            //print("Retrieving IP from API");
            if (!debugMode)
            {
                WWW IPwww = new WWW(ipUrl);
                yield return IPwww;
                if (IPwww.error == null) myIP = JsonConvert.DeserializeObject<PublicIP>(IPwww.text).ip;
                else Debug.Log("Get Public IP ERROR: " + IPwww.error);

                // Write IP to file
                WriteFile("ip", myIP);
            }
        }
        else // hasFile("ip")
        {
            // Read IP from file
            //print("Reading IP from file");
            myIP = ReadFile("ip");
        }

        // Get Location
        if (!hasFile("Location"))
        {
            // Retrieve Location from API
            //print("Retrieving Location from API");
            if (!debugMode)
            {
                string Lurl = locationUrl + myIP;
                WWW Lwww = new WWW(Lurl);
                yield return Lwww;
                if (Lwww.error == null)
                {
                    ImportLocation(Lwww.text);
                }
                else Debug.Log("Get Location ERROR: " + Lwww.error);
                // Write Location to file
                WriteFile("Location", Lwww.text);
            }
        }
        else // hasFile("Location")
        {
            // Read Location from file
            //print("Reading Location from file");
            ImportLocation(ReadFile("Location"));
        }


        // Get LatLon
        string myLatLon;
        if (manualLocation)
        {
            myLat = manualLatitude;
            myLon = manualLongitude;
        }
        else
        {
            if (!debugMode)
            {
                myLat = myLocation.latitude;
                myLon = myLocation.longitude;
            }
            else
            {
                myLat = debugLat;
                myLon = debugLon;
            }
        }
        myLatLon = myLat + "," + myLon;

        if (manualLocation)
        {
            // Get location name using lat lon
            //print("Getting location name using Google Geocode");
            if (!hasFile("Google_Geocode_" + myLatLon))
            {
                // Retrieve Location from API
                //print("Retrieving Google Geocode from API");
                if (!debugMode)
                {
                    string Gurl = googleGeocodeUrlFront + myLatLon + googleGeocodeUrlBack;
                    WWW Gwww = new WWW(Gurl);
                    yield return Gwww;
                    if (Gwww.error == null)
                    {
                        ImportG_Location(Gwww.text);
                    }
                    else Debug.Log("Google Geocode ERROR: " + Gwww.error);
                    // Write Location to file
                    WriteFile("Google_Geocode_" + myLatLon, Gwww.text);
                }
            }
            else // (hasFile("Google_Geocode_" + myLatLon))
            {
                // Read Location from file
                //print("Reading Google Geocode from file");
                ImportG_Location(ReadFile("Google_Geocode_" + myLatLon));
            }
        }

        if (printDebug) print("myLatLon = " + myLatLon);
        // Get Dark Sky Weather
        if (!hasFile(myLatLon))
        {
            // Retrieve Dark Sky Weather from API
            //print("Retrieving Dark Sky Weather from API"); 
            if (!debugMode)
            {
                string Wurl = weatherUrl + myLatLon;
                WWW Wwww = new WWW(Wurl);
                yield return Wwww;
                if (Wwww.error == null)
                {
                    ImportWeather(Wwww.text);

                    WriteFile(myLatLon, Wwww.text);
                    //File.SetCreationTime(resourcePath + myLatLon + ".txt", DateTime.Now);

                }

                else Debug.Log("Get Dark Sky Weather ERROR: " + Wwww.error);
            }
        }
        else
        {
            // Read Weather from file
            //print("Reading Dark Sky Weather from file");
            ImportWeather(ReadFile(myLatLon));
        }

        ReportDarkSkyDataCompleteness();
        CopyPasteFromCurrentlyToFirstMinute();

        // Get Yahoo Weather
        if (!hasFile("Y_" + myLatLon))
        {
            // Retrieve Yahoo Weather from API
            //print("Retrieving Yahoo Weather from API");
            if (!debugMode)
            {
                string YWurl = yahooWeatherApiUrlFront + myLatLon + yahooWeatherApiUrlBack;
                WWW YWwww = new WWW(YWurl);
                yield return YWwww;
                if (YWwww.error == null)
                {
                    Y_ImportWeather(YWwww.text);

                    WriteFile("Y_" + myLatLon, YWwww.text);
                    //File.SetCreationTime(resourcePath + myLatLon + ".txt", DateTime.Now);

                }

                else Debug.Log("Get Yahoo Weather ERROR: " + YWwww.error);
            }
        }
        else
        {
            // Read Weather from file
            //print("Reading Yahoo Weather from file");
            Y_ImportWeather(ReadFile("Y_" + myLatLon));
        }

        // Get UTC Offset
        // Retrieve UTC Offset from API
        //print("Retrieving UTC Offset from API");
        for (int i = 0; i < 2; i++)
        {
            if (i == 0 || manualLocation)
            {
                string timezone = i == 0 ? myDarkSkyCall.timezone : myLocation.time_zone;
                string Uurl = timeZoneDbUrl + timezone;
                WWW Uwww = new WWW(Uurl);
                yield return Uwww;
                if (Uwww.error == null)
                {
                    //ImportTimeZoneDbCall(Uwww.text);
                    myTimeZoneDbCall = new TimeZoneDbCall();
                    myTimeZoneDbCall = JsonConvert.DeserializeObject<TimeZoneDbCall>(Uwww.text);
                    if (i == 0) DestinationUtcOffsetInSeconds = myTimeZoneDbCall.gmtOffset;
                    else ComputerUtcOffsetInSeconds = myTimeZoneDbCall.gmtOffset;
                }
                else Debug.Log("Get UTC Offset ERROR: " + Uwww.error);
                yield return new WaitForSeconds(1.1f); // API only allows 1 call per second
            }
        }

        
        // turn world and weather back on
        world.SetActive(true);
        //weatherMakerPrefab.SetActive(true);


        // Set Day Night Cycle Parameters

        // Will need to change this if we time travel
        dayNightCycle.TimeZoneOffsetSeconds = DestinationUtcOffsetInSeconds; // 32400 is correct
        dayNightCycle.Speed = 1;
        dayNightCycle.NightSpeed = 1;

        DateTime now = DateTime.Now;
        double localUnixTime = myDarkSkyCall.currently.time;
        if (hasFile(myLatLon))
        {
            DateTime weatherFileCreationTime = File.GetCreationTime(filePath(myLatLon));
            double weatherFileAgeInSeconds = (now - weatherFileCreationTime).TotalSeconds;
            localUnixTime += weatherFileAgeInSeconds;
        }
        DateTime localDateTime = UnixToDateTime(localUnixTime);
        if (manualLocation) localDateTime = ConvertLocalDateTimeToDestination(localDateTime);
        /*
        if (manualLocation)
        {

            localDateTime = localDateTime.AddSeconds(-ComputerUtcOffsetInSeconds);
            localDateTime = localDateTime.AddSeconds(DestinationUtcOffsetInSeconds);
        }
        */

        startTime = localDateTime;
        SetWeatherMakerTime(localDateTime);
        /*
        dayNightCycle.TimeOfDay = (float)(localDateTime - new DateTime
                                                                   (localDateTime.Year,
                                                                    localDateTime.Month,
                                                                    localDateTime.Day)).TotalSeconds;
        
        print("Local time is " + localDateTime.ToShortTimeString());
        dayNightCycle.Year = localDateTime.Year;
        dayNightCycle.Month = localDateTime.Month;
        dayNightCycle.Day = localDateTime.Day;
        */
        dayNightCycle.Latitude = double.Parse(myLat);
        dayNightCycle.Longitude = double.Parse(myLon);
        realStartTime = DateTime.Now;

        startTime = GetDateTimeFromWeathermakerTime(dayNightCycle.TimeOfDay, dayNightCycle.Day, dayNightCycle.Month, dayNightCycle.Year);
        if (printDebug) print("startTime = " + startTime.ToLongDateString() + " " + startTime.ToLongTimeString());

        CorrectDarkSkyData();
        UpdateWeather(myDarkSkyCall.minutely.data[0]);
        isFirstPass = false;

        DateTime sunriseDT = UnixToDateTime(myDarkSkyCall.daily.data[0].sunriseTime);
        DateTime sunsetDT = UnixToDateTime(myDarkSkyCall.daily.data[0].sunsetTime);
        if (manualLocation)
        {
            sunriseDT = ConvertLocalDateTimeToDestination(sunriseDT);
            sunsetDT = ConvertLocalDateTimeToDestination(sunsetDT);
        }
        sunriseTime = (float)(sunriseDT - new DateTime(sunriseDT.Year,
                                                       sunriseDT.Month,
                                                       sunriseDT.Day)).TotalSeconds;
        sunsetTime = (float)(sunsetDT - new DateTime(sunsetDT.Year,
                                                     sunsetDT.Month,
                                                     sunsetDT.Day)).TotalSeconds;

        

        print("sunriseTime = " + sunriseTime);
        print("sunsetTime = " + sunsetTime);

        StartCoroutine(SpeedUpTime());

        if (!manualWeatherOverride) StartCoroutine(Loops());
        else
        {
            if (gentleSnow)
            {
                UpdatePercip(1f, "snow", .05f);
                //UpdateWind(.2f, .5f);
                //UpdateVisibility(.9f);
                //UpdateCloudCover(.9f);

            }
            else if (snowStorm)
            {
                UpdatePercip(1f, "snow", 1f);
                UpdateWind(.7f, .5f);
                UpdateVisibility(.3f);
                UpdateCloudCover(1f);
            }
            else if (thunderstorm)
            {
                CheckForThunderstorm("severe thunderstorms");
                UpdatePercip(1f, "rain", 1f);
                UpdateWind(.7f, .5f);
                UpdateVisibility(.7f);
                UpdateCloudCover(1f);
            }
            else if (clearDay)
            {
                UpdatePercip(0f, "", 0f);
                UpdateWind(.1f, .5f);
                UpdateVisibility(1f);
                UpdateCloudCover(.2f);
            }
            else if (hail)
            {
                UpdatePercip(1f, "hail", 1f);
                UpdateWind(.7f, .5f);
                UpdateVisibility(.3f);
                UpdateCloudCover(1f);
            }
            else if (sleet)
            {
                UpdatePercip(1f, "sleet", 1f);
                UpdateWind(.7f, .5f);
                UpdateVisibility(.3f);
                UpdateCloudCover(1f);
            }
        }

       
    }

    IEnumerator Loops()
    {
        float loopsPerSecond = 60;
        // Loop through Minutes
        if (printDebug) print("will begin looping through minutes");
        for (int m = 1; m < 60; m++)
        {
            if (myDarkSkyCall.minutely.data[m].time != -1)
            {
                if (printDebug) print("waiting for minute " + m);
                DateTime minuteTime = UnixToDateTime(myDarkSkyCall.minutely.data[m].time);
                if (manualLocation) minuteTime = ConvertLocalDateTimeToDestination(minuteTime);
                while (dayNightCycle.Year < minuteTime.Year ||
                        dayNightCycle.Month < minuteTime.Month ||
                        dayNightCycle.Day < minuteTime.Day ||
                        dayNightCycle.TimeOfDay < minuteTime.TimeOfDay.TotalSeconds)
                {
                    yield return new WaitForSeconds(1 / loopsPerSecond);
                }
                UpdateWeather(myDarkSkyCall.minutely.data[m]);
            }
        }
        // Loop through Hours
        if (printDebug) print("will begin looping through hours");
        for (int h = 1; h < 48; h++)
        {
            if (myDarkSkyCall.hourly.data[h].time != -1)
            {
                if (printDebug) print("waiting for hour " + h);
                DateTime hourTime = UnixToDateTime(myDarkSkyCall.hourly.data[h].time);
                if (manualLocation) hourTime = ConvertLocalDateTimeToDestination(hourTime);
                while (dayNightCycle.Year < hourTime.Year ||
                        dayNightCycle.Month < hourTime.Month ||
                        dayNightCycle.Day < hourTime.Day ||
                        dayNightCycle.TimeOfDay < hourTime.TimeOfDay.TotalSeconds)
                {
                    yield return new WaitForSeconds(1 / loopsPerSecond);
                }
                UpdateWeather(myDarkSkyCall.hourly.data[h]);
            }
        }

        // Loop through Days
        if (printDebug) print("will begin looping through days");
        for (int d = 2; d < 7; d++)
        {
            if (myDarkSkyCall.daily.data[2].time != -1)
            {
                if (printDebug) print("waiting for day " + d);
                DateTime dayTime = UnixToDateTime(myDarkSkyCall.daily.data[d].time);
                if (manualLocation) dayTime = ConvertLocalDateTimeToDestination(dayTime);
                while (dayNightCycle.Year < dayTime.Year ||
                        dayNightCycle.Month < dayTime.Month ||
                        dayNightCycle.Day < dayTime.Day ||
                        dayNightCycle.TimeOfDay < dayTime.TimeOfDay.TotalSeconds)
                {
                    yield return new WaitForSeconds(1 / loopsPerSecond);
                }
                UpdateWeather(myDarkSkyCall.daily.data[d]);
            }
        }
    }
    
    IEnumerator SpeedUpTime()
    {
        yield return new WaitForSeconds(5);
        DateTime newTime = startTime;
        double secondsInADay = 86400;
        while (GetDateTimeFromWeathermakerTime(dayNightCycle.TimeOfDay,
            dayNightCycle.Day,
            dayNightCycle.Month,
            dayNightCycle.Year) < startTime.AddSeconds(secondsInADay * 7))
        {
            double realTimePassedInSeconds = (DateTime.Now - realStartTime).TotalSeconds;
            /*
            double gameTimePassedInSeconds = -3150.312 + (1220.411 * realTimePassedInSeconds) + (192.9999 * Math.Pow(realTimePassedInSeconds, 2));
            newTime = startTime.AddSeconds(gameTimePassedInSeconds);
            SetWeatherMakerTime(newTime);
            */
            double speed = ((1929999 * realTimePassedInSeconds) + 6102055) / 5000;
            dayNightCycle.Speed = (float)speed;
            dayNightCycle.NightSpeed = (float)speed;
            yield return new WaitForSeconds(1f / 60f);
        }
        dayNightCycle.Speed = 1;
        dayNightCycle.NightSpeed = 1;
        
    }
    

    void UpdateWeather(Data data)
    {
        CheckForThunderstorm(data.icon);
        UpdatePercip(data.precipProbability,
                        data.precipType,
                        data.precipIntensity);
        UpdateWind(data.windSpeed, data.windBearing);
        UpdateVisibility(data.visibility);
        UpdateCloudCover(data.cloudCover);
    }

    void UpdateCloudCover(float cloudCover)
    {
        if (!float.IsNaN(cloudCover))
        {
            if (printDebug) print("cloud clover = " + cloudCover);
            //skySphere.CloudCover = cloudCover;
            if (skySphere.CloudCover != cloudCover)
            {
                if (isFirstPass && !manualWeatherOverride) skySphere.CloudCover = cloudCover;
                else
                {
                    if (cloudChangeCoroutine != null)
                    {
                        StopCoroutine(cloudChangeCoroutine);
                        incrementCloudCover = false;
                        decrementCloudCover = false;
                    }
                    cloudChangeCoroutine = StartCoroutine(SetCloudCover(cloudCover));
                }
            }
        }
    }
    
    IEnumerator SetCloudCover(float cloudCover)
    {
        if (skySphere.CloudCover > cloudCover)
        {
            decrementCloudCover = true;
            while (skySphere.CloudCover > cloudCover)
            {
                yield return new WaitForSeconds(1f / 30f);
            }
            decrementCloudCover = false;
        }
        else if (skySphere.CloudCover < cloudCover)
        {
            incrementCloudCover = true;
            while (skySphere.CloudCover < cloudCover)
            {
                yield return new WaitForSeconds(1f / 30f);
            }
            incrementCloudCover = false;
        }
    }

    IEnumerator SetCloudVelocity_x(float cloudVelocity_x)
    {
        if (skySphere.CloudNoiseVelocity.x > cloudVelocity_x)
        {
            decrementCloudVelocity_x = true;
            while (skySphere.CloudNoiseVelocity.x > cloudVelocity_x)
            {
                yield return new WaitForSeconds(1f / 30f);
            }
            incrementCloudVelocity_x = false;
        }
        else if (skySphere.CloudNoiseVelocity.x < cloudVelocity_x)
        {
            incrementCloudVelocity_x = true;
            while (skySphere.CloudNoiseVelocity.x < cloudVelocity_x)
            {
                yield return new WaitForSeconds(1f / 30f);
            }
            decrementCloudVelocity_x = false;
        }
    }

    IEnumerator SetCloudVelocity_y(float cloudVelocity_y)
    {
        if (skySphere.CloudNoiseVelocity.y > cloudVelocity_y)
        {
            decrementCloudVelocity_y = true;
            while (skySphere.CloudNoiseVelocity.y > cloudVelocity_y)
            {
                yield return new WaitForSeconds(1f / 30f);
            }
            incrementCloudVelocity_y = false;
        }
        else if (skySphere.CloudNoiseVelocity.y < cloudVelocity_y)
        {
            incrementCloudVelocity_y = true;
            while (skySphere.CloudNoiseVelocity.y < cloudVelocity_y)
            {
                yield return new WaitForSeconds(1f / 30f);
            }
            decrementCloudVelocity_y = false;
        }
    }

    void UpdateVisibility(float visibility)
    {
        if (visibility != float.NaN) fullScreenFog.FogDensity = VisibilityToFogDensity(visibility);
    }

    void UpdateWind(float windSpeed, float windBearing)
    {
        if (windSpeed != float.NaN)
        {
            if (printDebug) print("Wind speed is " + windSpeed);
            wind.WindIntensity = ConvertWindSpeed(windSpeed);
            windSpeed *= dayNightCycle.Speed * cloudSpeedMultiplier;
            double d = (0.005 * windSpeed);
            double x = d * Math.Cos(windBearing % 90);
            double y = d * Math.Sin(windBearing % 90);
            if (windBearing < 180) x *= -1;
            if (windBearing < 90 || windBearing > 270) y *= -1;
            if (isFirstPass)
            {
                skySphere.CloudNoiseVelocity.x = (float) x;
                skySphere.CloudNoiseVelocity.y = (float) y;
            }
            else
            {
                if (skySphere.CloudNoiseVelocity.x != x)
                {
                    if (cloudVelocityCoroutine_x != null)
                    {
                        StopCoroutine(cloudVelocityCoroutine_x);
                        incrementCloudVelocity_x = false;
                        decrementCloudVelocity_x = false;
                    }
                    cloudVelocityCoroutine_x = StartCoroutine(SetCloudVelocity_x((float)x));
                }
                if (skySphere.CloudNoiseVelocity.y != y)
                {
                    if (cloudVelocityCoroutine_y != null)
                    {
                        StopCoroutine(cloudVelocityCoroutine_y);
                        incrementCloudVelocity_y = false;
                        decrementCloudVelocity_y = false;
                    }
                    cloudVelocityCoroutine_y = StartCoroutine(SetCloudVelocity_y((float)y));
                }
            }
            
        }
    }

    void UpdatePercip(float probability, string type, float intensity)
    {
        //print("Updating Percip");
        bool percipRollAchieved = false;
        if (!string.IsNullOrEmpty(type)) if (printDebug) print("Percip type is " + type);
        //else print("Percip type is null or empty");
        if (probability != float.NaN) if (printDebug) print("Percip probability is " + probability);
        //else print("probability is NaN");

        if (probability == float.NaN)
        {
            if (string.IsNullOrEmpty(type)) percipRollAchieved = false;
            else percipRollAchieved = true;
        }
        else if (probability > 0)
        {
            float percipRoll = UnityEngine.Random.Range(0, 1f);
            if (printDebug) print("percipRoll = " + percipRoll);
            if (probability >= percipRoll)
            {
                if (printDebug) print("percip roll achieved!");
                percipRollAchieved = true;
            }
            else
            {
                if (printDebug) print("percip roll failed");
                percipRollAchieved = false;
            }
        }
        if ((!string.IsNullOrEmpty(type)) && percipRollAchieved)
        {
            switch (type)
            {
                case "rain":
                    if (printDebug) print("raining");
                    weatherMaker.Precipitation = WeatherMakerPrecipitationType.Rain;
                    break;
                case "snow":
                    if (printDebug) print("snowing");
                    weatherMaker.Precipitation = WeatherMakerPrecipitationType.Snow;
                    break;
                case "sleet":
                    if (printDebug) print("sleeting");
                    weatherMaker.Precipitation = WeatherMakerPrecipitationType.Sleet;
                    break;
                case "hail":
                    if (printDebug) print("hailing");
                    weatherMaker.Precipitation = WeatherMakerPrecipitationType.Hail;
                    break;
                default:
                    weatherMaker.Precipitation = WeatherMakerPrecipitationType.None;
                    break;
            }
            if (intensity == float.NaN)
            {
                if (printDebug) print("percip intensity is NaN, setting intensity to 0.5");
                weatherMaker.PrecipitationIntensity = 0.5f;
            }
            else
            {
                if (printDebug) print("percip intensity is " + ConvertPercipIntensity(intensity));
                weatherMaker.PrecipitationIntensity = ConvertPercipIntensity(intensity);
            }
        }
    }

    void CheckForThunderstorm(string icon)
    {
        if (!string.IsNullOrEmpty(icon) && (icon.Equals("thunderstorm") ||
                                            icon.Equals("thunderstorms") ||
                                            icon.Equals("severe thunderstorm") ||
                                            icon.Equals("severe thunderstorms") ||
                                            icon.Equals("thunder") ||
                                            icon.Equals("severe thunder")))
        {
            if (printDebug) print("thunderstorm");
            thunderAndLightning.EnableLightning = true;
            if (icon.Contains("severe"))
            {
                thunderAndLightning.LightningIntervalTimeRange.Maximum = 12.5f;
                thunderAndLightning.LightningIntervalTimeRange.Minimum = 5f;
            }
            else
            {
                thunderAndLightning.LightningIntervalTimeRange.Maximum = 25f;
                thunderAndLightning.LightningIntervalTimeRange.Minimum = 10f;
            }
        }
        else thunderAndLightning.EnableLightning = false;
    }

    static string filePath(string fileName)
    {
        return folderPath + fileName + ".txt";
    }

    void ImportLocation(string text)
    {
        myLocation = JsonConvert.DeserializeObject<Location>(text);
        if (!manualLocation) if (printDebug) print("My location is " + myLocation.city);
    }

    void ImportG_Location(string text)
    {
        myG_Location = JsonConvert.DeserializeObject<G_Location>(text);
        if (printDebug) print("My location is " + myG_Location.results[0].formatted_address);
    }

    void ImportWeather(string text)
    {
        myDarkSkyCall = new DarkSkyCall();
        myDarkSkyCall = JsonConvert.DeserializeObject<DarkSkyCall>(text);
        if (printDebug) print("The weather is " + myDarkSkyCall.currently.icon);
        if (printDebug) print("The temperature is " + myDarkSkyCall.currently.temperature);
    }

    void Y_ImportWeather(string text)
    {
        myYahooWeatherCall = new YahooWeatherCall();
        myYahooWeatherCall = JsonConvert.DeserializeObject<YahooWeatherCall>(text);
        //print("myYahooWeatherCall.query.count = " + myYahooWeatherCall.query.count);
        if (printDebug) print("The Yahoo weather is " + myYahooWeatherCall.query.results.channel.item.condition.text);
        if (printDebug) print("The Yahoo temperature is " + myYahooWeatherCall.query.results.channel.item.condition.temp);
    }

    static void DeleteOldFiles()
    {
        DateTime now = DateTime.Now;
        DateTime deadline;
        foreach (string file in Directory.GetFiles(folderPath)) // for every file in our resource directory
        {
            string fileName = file.ToString();
            //deadline = 
            //if (File.GetCreationTime(filePath(fileName)).CompareTo()
            if (!fileName.Substring(fileName.Length - ".meta".Length).Equals(".meta")) // ignore meta files
            {
                fileName = fileName.Substring(folderPath.Length); // remove resource path
                fileName = fileName.Substring(0, fileName.Length - ".txt".Length); // remove .txt
                deadline = fileCreatedTime(fileName);
                //print(fileName + " created time = " + deadline);
                switch (fileName)
                {
                    case "ip":
                        deadline = deadline.AddMinutes(ipStoreMinutes);
                        break;
                    case "Location":
                        deadline = deadline.AddMinutes(locationStoreMinutes);
                        break;
                    default:
                        deadline = deadline.AddMinutes(weatherStoreMinutes);
                        break;
                }
                /*
                print("fileName = " + fileName);
                print("now = " + now);
                print("deadline = " + deadline);
                print("fileCreatedTime(fileName)" + fileCreatedTime(fileName));
                */
                if (DateTime.Compare(now, deadline) > 0) // if we are past the deadline
                {
                    //print("deleting file " + fileName + ", deadline = " + deadline);
                    DeleteFile(fileName);
                }
                //else print("keeping file " + fileName + ", deadline = " + deadline);
            }


        }
    }
    
    bool hasFile(string name)
    {
        // Returns the File if it exists, otherwise returns null
        foreach (string file in Directory.GetFiles(folderPath))
        {
            string fileName = file.ToString();
            //print(fileName);
            if (!fileName.Substring(fileName.Length - ".meta".Length).Equals(".meta")) // ignore meta files
            {
                fileName = fileName.Substring(folderPath.Length); // remove resource path
                fileName = fileName.Substring(0, fileName.Length - ".txt".Length); // remove .txt

                if (fileName.Equals(name)) return true;
            }
        }
        return false;
    }
    
    static void DeleteFile(string fileName)
    {
        File.Delete(filePath(fileName));
    }

    static void WriteFile(string name, string text)
    {
        File.WriteAllText(filePath(name), text);
        File.SetCreationTime(filePath(name), DateTime.Now);
    }

    static string ReadFile(string name)
    {
        return File.ReadAllText(filePath(name));
    }

    static DateTime fileCreatedTime(string name)
    {
        return File.GetCreationTime(filePath(name));
    }

    public static DateTime UnixToDateTime(double unixTimeStamp)
    {
        // Unix timestamp is seconds past epoch
        DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    double DateTimetoUnix(DateTime dateTime)
    {
        return (TimeZoneInfo.ConvertTimeToUtc(dateTime) -
               new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc)).TotalSeconds;
    }

    
    //[MenuItem("Tools/Clear Cache")]
    static void ClearCache()
    {
        foreach (string file in Directory.GetFiles(folderPath))
        {
            File.Delete(file);
        }
    }

    float VisibilityToFogDensity(float v)
    {
        float d = -0.00001020408f + (0.1000102f * (float)Math.Pow(e, (-0.919024f * v)));
        if (d < 0) return 0;
        else if (d > 1) return 1;
        else return d;
    }

    float ConvertPercipIntensity(float before)
    {
        float after = 3.333333f * before;
        if (after < 0) return 0;
        else if (after > 1) return 1;
        else return after;
    }

    float ConvertWindSpeed(float before)
    {
        float after = -1.665335e-16f - (0.004661178f * before) + 0.002733059f * (float) Math.Pow(before, 2);
        if (after < 0) return 0;
        else if (after > 1) return 1;
        else return after;
    }
    
    void CorrectDarkSkyData()
    {
        // Correct next 60 minutes
        for(int m = 0; m < 60; m++)
        {
            int code;
            if (m < 15) code = int.Parse(myYahooWeatherCall.query.results.channel.item.condition.code);
            else if ((dayNightCycle.TimeOfDay + (m * 60)) <= 86400)
                code = int.Parse(myYahooWeatherCall.query.results.channel.item.forecast[0].code);
            else code = int.Parse(myYahooWeatherCall.query.results.channel.item.forecast[1].code);
            CorrectDarkSkyDataUsingYahooCode(myDarkSkyCall.minutely.data[m], code);
        }

        // Correct next 48 hours
        for (int h = 0; h < 48; h++)
        {
            int code;
            if (h == 0) code = int.Parse(myYahooWeatherCall.query.results.channel.item.condition.code);
            else if ((dayNightCycle.TimeOfDay + (h * 60 * 60)) <= 86400)
                 code = int.Parse(myYahooWeatherCall.query.results.channel.item.forecast[0].code);
            else code = int.Parse(myYahooWeatherCall.query.results.channel.item.forecast[1].code);
            CorrectDarkSkyDataUsingYahooCode(myDarkSkyCall.hourly.data[h], code);
        }

        // Correct next 7 days
        for (int d = 0; d < 7; d++)
        {
            int code = int.Parse(myYahooWeatherCall.query.results.channel.item.forecast[d].code);
            CorrectDarkSkyDataUsingYahooCode(myDarkSkyCall.daily.data[d], code);
        }
    }

    void CorrectDarkSkyDataUsingYahooCode(Data data, int code)
    {
        switch(GetStringFromConditionCode(code))
        {
            case "severe thunderstorms":
                data.icon = "severe thunderstorms";
                break;
            case "thunderstorms":
                data.icon = "thunderstorms";
                break;
            case "mixed rain and snow":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "rain";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "mixed rain and sleet":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "rain";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "mixed snow and sleet":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "snow";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "freezing drizzle":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "rain";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .05) data.precipIntensity = .05f;
                break;
            case "drizzle":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "rain";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .05) data.precipIntensity = .05f;
                break;
            case "freezing rain":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "rain";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "showers":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "rain";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "snow flurries":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "snow";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .05) data.precipIntensity = .05f;
                break;
            case "light snow showers":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "snow";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .05) data.precipIntensity = .05f;
                break;
            case "blowing snow":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "snow";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .05) data.precipIntensity = .05f;
                break;
            case "snow":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "snow";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "hail":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "hail";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "sleet":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "sleet";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "foggy":
                data.foggy = true;
                break;
            case "windy":
                if (data.windSpeed < 3) data.windSpeed = 3f;
                break;
            case "cloudy":
                if (data.cloudCover < .5) data.cloudCover = .5f;
                break;
            case "mostly cloudy (night)":
                if (data.cloudCover < .35) data.cloudCover = .35f;
                break;
            case "mostly cloudy(day)":
                if (data.cloudCover < .35) data.cloudCover = .35f;
                break;
            case "partly cloudy(night)":
                if (data.cloudCover < .15) data.cloudCover = .15f;
                break;
            case "partly cloudy(day)":
                if (data.cloudCover < .15) data.cloudCover = .15f;
                break;
            case "mixed rain and hail":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "rain";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "heavy snow":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "snow";
                if (data.precipProbability < .35) data.precipProbability = .35f;
                if (data.precipIntensity < .5) data.precipIntensity = .5f;
                break;
            case "partly cloudy":
                if (data.cloudCover < .15) data.cloudCover = .15f;
                break;
            case "thundershowers":
                data.icon = "thunderstorms";
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "rain";
                if (data.precipProbability < .1) data.precipProbability = .1f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
            case "snow showers":
                if (string.IsNullOrEmpty(data.precipType)) data.precipType = "snow";
                if (data.precipProbability < .1) data.precipProbability = .1f;
                if (data.precipIntensity < .1) data.precipIntensity = .1f;
                break;
        }
    }

    string GetStringFromConditionCode(int code)
    {
        switch (code)
        {
            case 0:  return "tornado";
            case 1:  return "tropical storm";
            case 2:  return "hurricane";
            case 3:  return "severe thunderstorms";
            case 4:  return "thunderstorms";
            case 5:  return "mixed rain and snow";
            case 6:  return "mixed rain and sleet";
            case 7:  return "mixed snow and sleet";
            case 8:  return "freezing drizzle";
            case 9:  return "drizzle";
            case 10: return "freezing rain";
            case 11: return "showers";
            case 12: return "showers";
            case 13: return "snow flurries";
            case 14: return "light snow showers";
            case 15: return "blowing snow";
            case 16: return "snow";
            case 17: return "hail";
            case 18: return "sleet";
            case 19: return "dust";
            case 20: return "foggy";
            case 21: return "haze";
            case 22: return "smoky";
            case 23: return "blustery";
            case 24: return "windy";
            case 25: return "cold";
            case 26: return "cloudy";
            case 27: return "mostly cloudy (night)";
            case 28: return "mostly cloudy(day)";
            case 29: return "partly cloudy(night)";
            case 30: return "partly cloudy(day)";
            case 31: return "clear(night)";
            case 32: return "sunny";
            case 33: return "fair(night)";
            case 34: return "fair(day)";
            case 35: return "mixed rain and hail";
            case 36: return "hot";
            case 37: return "isolated thunderstorms";
            case 38: return "scattered thunderstorms";
            case 39: return "scattered thunderstorms";
            case 40: return "scattered showers";
            case 41: return "heavy snow";
            case 42: return "scattered snow showers";
            case 43: return "heavy snow";
            case 44: return "partly cloudy";
            case 45: return "thundershowers";
            case 46: return "snow showers";
            case 47: return "isolated thundershowers";
            default: return "none";
        }
    }

    void ReportDarkSkyDataCompleteness()
    {
        bool hasCurrently = false;
        int minutelyCount = 0;
        int hourlyCount = 0;
        int dailyCount = 0;

        // Currently
        if (!string.IsNullOrEmpty(myDarkSkyCall.currently.summary)) hasCurrently = true;

        // Minutely
        for (int m = 0; m < 60; m++)
        {
            if (!string.IsNullOrEmpty(myDarkSkyCall.minutely.data[m].summary)) minutelyCount++;
            else break;
        }

        // Hourly
        for (int h = 0; h < 48; h++)
        {
            if (!string.IsNullOrEmpty(myDarkSkyCall.hourly.data[h].summary)) hourlyCount++;
            else break;
        }

        // Daily
        for (int d = 0; d < 7; d++)
        {
            if (!string.IsNullOrEmpty(myDarkSkyCall.daily.data[d].summary)) dailyCount++;
            else break;
        }

        if (printDebug) print("Dark Sky call does " + (hasCurrently ? "" : "not ") + "have current weather. Has first " + minutelyCount + " minutes, " + hourlyCount + " hours, and " + dailyCount + " days.");
    }

    void CopyPasteFromCurrentlyToFirstMinute()
    {
        myDarkSkyCall.minutely.data[0].time = myDarkSkyCall.currently.time;
        myDarkSkyCall.minutely.data[0].summary = myDarkSkyCall.currently.summary;
        myDarkSkyCall.minutely.data[0].icon = myDarkSkyCall.currently.icon;
        myDarkSkyCall.minutely.data[0].precipIntensity = myDarkSkyCall.currently.precipIntensity;
        myDarkSkyCall.minutely.data[0].precipIntensityError = myDarkSkyCall.currently.precipIntensityError;
        myDarkSkyCall.minutely.data[0].precipProbability = myDarkSkyCall.currently.precipProbability;
        myDarkSkyCall.minutely.data[0].precipType = myDarkSkyCall.currently.precipType;
        myDarkSkyCall.minutely.data[0].temperature = myDarkSkyCall.currently.temperature;
        myDarkSkyCall.minutely.data[0].apparentTemperature = myDarkSkyCall.currently.apparentTemperature;
        myDarkSkyCall.minutely.data[0].dewPoint = myDarkSkyCall.currently.dewPoint;
        myDarkSkyCall.minutely.data[0].humidity = myDarkSkyCall.currently.humidity;
        myDarkSkyCall.minutely.data[0].windSpeed = myDarkSkyCall.currently.windSpeed;
        myDarkSkyCall.minutely.data[0].windGust = myDarkSkyCall.currently.windGust;
        myDarkSkyCall.minutely.data[0].windBearing = myDarkSkyCall.currently.windBearing;
        myDarkSkyCall.minutely.data[0].visibility = myDarkSkyCall.currently.visibility;
        myDarkSkyCall.minutely.data[0].cloudCover = myDarkSkyCall.currently.cloudCover;
        myDarkSkyCall.minutely.data[0].pressure = myDarkSkyCall.currently.pressure;
        myDarkSkyCall.minutely.data[0].ozone = myDarkSkyCall.currently.ozone;
        myDarkSkyCall.minutely.data[0].time = myDarkSkyCall.currently.time;
        myDarkSkyCall.minutely.data[0].time = myDarkSkyCall.currently.time;
        myDarkSkyCall.minutely.data[0].time = myDarkSkyCall.currently.time;
        myDarkSkyCall.minutely.data[0].time = myDarkSkyCall.currently.time;
        myDarkSkyCall.minutely.data[0].time = myDarkSkyCall.currently.time;
        myDarkSkyCall.minutely.data[0].time = myDarkSkyCall.currently.time;
    }

    DateTime ConvertLocalDateTimeToDestination(DateTime localDateTime)
    {
        localDateTime = localDateTime.AddSeconds(-ComputerUtcOffsetInSeconds);
        localDateTime = localDateTime.AddSeconds(DestinationUtcOffsetInSeconds);
        return localDateTime;
    }

    DateTime GetDateTimeFromWeathermakerTime(float timeOfDay, int day, int month, int year)
    {
        DateTime dt = new DateTime(year, month, day);
        dt = dt.AddSeconds(timeOfDay);
        return dt;
    }

    void SetWeatherMakerTime(DateTime dt)
    {
        dayNightCycle.TimeOfDay = (float)(dt - new DateTime
                                                                   (dt.Year,
                                                                    dt.Month,
                                                                    dt.Day)).TotalSeconds;
        dayNightCycle.Year = dt.Year;
        dayNightCycle.Month = dt.Month;
        dayNightCycle.Day = dt.Day;
    }

}

