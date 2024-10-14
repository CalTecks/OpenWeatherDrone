using System;
using UnityEngine;

public class WeatherManagerBackup : MonoBehaviour
{
    private GameObject getWeather;
    private GetWeatherData weatherDataComponent;
    private GameObject sun;

    void Start()
    {
        getWeather = GameObject.FindGameObjectWithTag("GetWeather");
        if (getWeather != null)
        {
            weatherDataComponent = getWeather.GetComponent<GetWeatherData>();
            // Subscribe to the event
            weatherDataComponent.OnWeatherDataUpdated += UpdateWeatherInScene;
        }
        else Debug.Log("No GetWeather object found.");

        sun = GameObject.FindGameObjectWithTag("SunLight");
        if (sun == null) Debug.Log("No SunLight GameObject found.");

    }

    void UpdateWeatherInScene(WeatherData weatherData)
    {

        UpdateSunPosition(weatherData);
    }

    void UpdateSunPosition(WeatherData weatherData)
    {
        bool night = false;
        long currentTime = GetUnixTimeNow();
        Debug.Log("CURRENT TIME:" + currentTime);
        long sunrise = weatherData.sys.sunrise;
        long sunset = weatherData.sys.sunset;
        // soms is de data ouder dan 24u, compenseren tot het klopt
        Debug.Log("CUR TIME - SUNRISE: " + (currentTime - sunrise));
        while (currentTime - sunrise > 86400) {
            Debug.Log("current time -24h" + currentTime);
            currentTime -= 86400;
            Debug.Log("new current time: " + currentTime);

        }
        if (currentTime < sunrise || currentTime > sunset)
        {
            Debug.Log("nachtmodus");
            night = true;
            // tijd 12 u terugdraaien of verder draaien
            if (currentTime > sunset)
            {
                Debug.Log("epoch -43200");
                currentTime -= 43200;
            }
            else if (currentTime < sunrise)
            {
                currentTime += 43200;
                Debug.Log("epoch +43200");
            }
        }
        // berekenen hoek
        // indien nacht is currentTime 12u verder of terug gedraaid
        // hierdoor weten we de hoek overdag, maar + 180 graden om terug 12u op te schuiven
        // aangezien +180 of -180 hetzelfde resultaat geeft enkel + gebruikt
        float dayProgress = Mathf.InverseLerp(sunrise, sunset, currentTime);
        Debug.Log("epoch sunrise:" + sunrise);
        Debug.Log("epoch sunset: " + sunset);
        Debug.Log("epoch current time: " + currentTime);
        Debug.Log("day progress:" + dayProgress);
        float sunAngle = dayProgress * 180f;
        if (night) sunAngle += 180f;
        sun.transform.rotation = Quaternion.Euler(sunAngle, 0, 0);
        Debug.Log("Sun angle:" + sunAngle);
    }

        long GetUnixTimeNow()
        {
            // Return current time in Unix format
            return (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
        }
    }