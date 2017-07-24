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


public class ParameterCollector : MonoBehaviour {

    /* latitude is from -90 to 90
     *  longitude is from -180 to +180
     * 
     * 
     */

    public int iterations = 500;

    static private string folderPath;
    private string weatherUrl = "https://api.darksky.net/forecast/1c6c6640edb8328a5a51501fbeaeed55/";

    // Use this for initialization
    void Start () {
        folderPath = (Application.platform == RuntimePlatform.Android ||
                        Application.platform == RuntimePlatform.IPhonePlayer ?
                        Application.persistentDataPath :
                        Application.dataPath)
                        + "/Resources/";
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }
        StartCoroutine(MyCoroutine());
	}
	
    IEnumerator MyCoroutine()
    {
        print("starting coroutine");
        for (int i = 0; i < iterations; i++)
        {
            /*
            string latitude = randomLatitude().ToString();
            string longitude = randomLongitude().ToString();
            string myLatLon = latitude + "," + longitude;
            print("latitude = " + latitude + ", longitude = " + longitude);
            string url = weatherUrl + myLatLon;
            WWW www = new WWW(url);
            yield return www;
            */

            string jsonText = File.ReadAllText(filePath("35.8569,139.6489"));
            //IDictionary<string, object> deserializedJson = jsonText.ToDictionary();
            //Forecast myForecast = JsonConvert.DeserializeObject<Forecast>(jsonText);
            //print(myForecast.hourly.data[0]);




            using (var reader = new JsonTextReader(new StringReader(jsonText)))
            {
                while (reader.Read())
                {
                    /*
                    Console.WriteLine("{0} - {1} - {2}",
                                      reader.TokenType, reader.ValueType, reader.Value);
                                      */
                    print(reader.TokenType + " - " + reader.ValueType + " - " + reader.Value);
                }
            }



            yield return new WaitForSeconds(1000);
        }
    }



    private static string filePath(string fileName)
    {
        return folderPath + fileName + ".txt";
    }

    private float randomLatitude() { return UnityEngine.Random.Range(-90f, 90f); }

    private float randomLongitude() { return UnityEngine.Random.Range(-180f, 180f); }
    
}
