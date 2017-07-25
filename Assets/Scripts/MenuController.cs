using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour {

    public InputField latitudeInputField;
    public InputField longitudeInputField;
    public Text warningText;
    public Text debugText;

	void Start () {
        warningText.text = "";
	}
	
	void Update () {
		
	}

    public void UseManualLocationButtonClicked()
    {
        float latitude;
        float longitude;
        if (float.TryParse(latitudeInputField.text, out latitude) &&
            float.TryParse(longitudeInputField.text, out longitude) &&
            isGoodLatitude(latitude) &&
            isGoodLongitude(longitude))
        {
            //debugText.text = latitude + ", " + longitude;
            PlayerPrefs.SetInt("isManualLocation", 1);
            PlayerPrefs.SetFloat("Latitude", latitude);
            PlayerPrefs.SetFloat("Longitude", longitude);
            SceneManager.LoadScene("Weather Scene");
        }
        else ShowInputError();
    }

    bool isGoodLatitude(float lat)
    {
        if (lat >= -90 && lat <= 90) return true;
        else return false;
    }

    bool isGoodLongitude(float lon)
    {
        if (lon >= -180 && lon <= 180) return true;
        else return false;
    }

    void ShowInputError()
    {
        warningText.text = "Please Provide a suitable Latitude and Longitude";
    }

}
