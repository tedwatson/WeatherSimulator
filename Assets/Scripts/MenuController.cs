using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour {

    public InputField latitudeInputField;
    public InputField longitudeInputField;

	void Start () {
		
	}
	
	void Update () {
		
	}

    void StartButtonClicked()
    {
        double latitude;
        double longitude;
        if (double.TryParse(latitudeInputField.text, out latitude))
        {

        }
    }

    bool isGoodLatitude(double lat)
    {
        return true;
    }

    bool isGoodLongitude(double lat)
    {
        return true;
    }


}
