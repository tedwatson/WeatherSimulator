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
        PlayerPrefs.SetString("Override Setting", "none");
	}
	
	void Update () {
		
	}

    public void UseCurrentLocationButtonClicked()
    {
        PlayerPrefs.SetInt("isManualLocation", 0);
        StartGame();
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
            StartGame();
        }
        else ShowInputError();
    }

    public void GentleSnowButtonClicked()
    {
        PlayerPrefs.SetString("Override Setting", "gentle snow");
        StartGame();
    }

    public void SnowStormButtonClicked()
    {
        PlayerPrefs.SetString("Override Setting", "snow storm");
        StartGame();
    }

    public void ClearDayButtonClicked()
    {
        PlayerPrefs.SetString("Override Setting", "clear day");
        StartGame();
    }

    public void HailButtonClicked()
    {
        PlayerPrefs.SetString("Override Setting", "hail");
        StartGame();
    }

    public void SleetButtonClicked()
    {
        PlayerPrefs.SetString("Override Setting", "sleet");
        StartGame();
    }

    public void ThunderStormButtonClicked()
    {
        PlayerPrefs.SetString("Override Setting", "thunder storm");
        StartGame();
    }

    void StartGame()
    {
        SceneManager.LoadScene("Weather Scene");
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
