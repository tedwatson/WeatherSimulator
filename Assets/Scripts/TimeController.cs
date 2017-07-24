using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TimeController : MonoBehaviour {

    public float maxSecondsPerDay = 2f;
    public float maxDistance = 9f;
    public Text timeText;
    public DragObject dragObject;

    private bool isHoldingCube;
    private DateTime currentTime;
    private float maxMultiplier;
    private float timeScale1;
    private float timeScale2;

    public void setIsHoldingCube(bool b) { isHoldingCube = b; }

    public void droppedCube()
    {
        Time.timeScale = 1;
        timeScale1 = 1;
        timeScale2 = 1;
    }

	// Use this for initialization
	void Start () {
        currentTime = DateTime.Now;
        isHoldingCube = false;
        maxMultiplier = 4000f;
        timeScale1 = 1;
        timeScale2 = 1;

        StartCoroutine(OperateClock());
        //Time.timeScale = 100;
	}

    private void FixedUpdate()
    {
        if (isHoldingCube)
        {
            CalculateTimeScaling(dragObject.getDistance());
            Time.timeScale = timeScale1;
        }
    }
    
    IEnumerator OperateClock()
    {
        yield return new WaitForSeconds(1);
        currentTime = currentTime.AddSeconds(timeScale2);
        timeText.text = currentTime.ToString();
        StartCoroutine(OperateClock());
    }

    private void CalculateTimeScaling(float distance)
    {
        float multiplier;
        if (distance > maxDistance) multiplier = maxMultiplier;
        else multiplier = ((distance / maxDistance) * (maxMultiplier - 1)) + 1;
        //print(multiplier);

        if (multiplier <= 100)
        {
            timeScale1 = multiplier;
            timeScale2 = 1;
        }
        else
        {
            timeScale1 = 100;
            timeScale2 = multiplier - 100;
        }
    }
    
}
