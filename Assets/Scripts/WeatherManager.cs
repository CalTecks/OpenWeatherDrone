using UnityEngine;
using TMPro;
using System;

public class WeatherManager : MonoBehaviour
{
    private GameObject getWeather;
    private GetWeatherData weatherDataComponent;
    private GameObject sun;
    private GameObject boundaryPlane;
    private Material dryMaterial, snowMaterial, movingGrassMaterial;
    private ParticleSystem rainSystem, snowSystem, cloudSystem;
    private float surfaceCompensation;
    private TMP_Text textLocation, textTemperature, textWindSpeed, textRain, textSnow, textCloudDensity;

    void Start()
    {
        // als ik deze als public variabelen gebruik en deze in inspector toewijs
        // krijg ik nog steeds de foutmelding dat ze niet zijn toegewezen
        // op deze manier werkt het wel, ik veronderstel een unity 6 bug
        // met tags lukt het iets beter maar ook niet altijd...
        GameObject rainObject = GameObject.FindGameObjectWithTag("RainSystem");
        rainSystem = rainObject.GetComponent<ParticleSystem>();
        
        GameObject snowObject = GameObject.FindGameObjectWithTag("SnowSystem");
        snowSystem = snowObject.GetComponent<ParticleSystem>();
        
        GameObject cloudObject = GameObject.FindGameObjectWithTag("CloudSystem");
        cloudSystem = cloudObject.GetComponent<ParticleSystem>();

        GameObject textLocationObject = GameObject.FindGameObjectWithTag("TextLocation");
        textLocation = textLocationObject.GetComponent<TMP_Text>();
        
        GameObject textTemperatureObject = GameObject.FindGameObjectWithTag("TextTemperature");
        textTemperature = textTemperatureObject.GetComponent<TMP_Text>();
        
        GameObject textWindSpeedObject = GameObject.FindGameObjectWithTag("TextWindSpeed");
        textWindSpeed = textWindSpeedObject.GetComponent<TMP_Text>();
        
        GameObject textRainObject = GameObject.FindGameObjectWithTag("TextRain");
        textRain = textRainObject.GetComponent<TMP_Text>();

        GameObject textSnowObject = GameObject.FindGameObjectWithTag("TextSnow");
        textSnow = textSnowObject.GetComponent<TMP_Text>();

        GameObject textCloudDensityObject = GameObject.FindGameObjectWithTag("TextCloudDensity");
        textCloudDensity = textCloudDensityObject.GetComponent<TMP_Text>();


        movingGrassMaterial = Resources.Load<Material>("WavingGrass/Materials/GrasMaterial");
        dryMaterial = Resources.Load<Material>("Stylize Snow Texture/Materials/Stylize_Grass");
        snowMaterial = Resources.Load<Material>("Stylize Snow Texture/Materials/Stylize Snow");
        boundaryPlane = GameObject.FindGameObjectWithTag("BoundaryPlane");
        // compensatie variabele berekenen (simpelweg oppervlakte ondergrond)
        surfaceCompensation = boundaryPlane.transform.localScale.x * boundaryPlane.transform.localScale.z;    
        
        getWeather = GameObject.FindGameObjectWithTag("GetWeather");
        if (getWeather != null)
        {
            weatherDataComponent = getWeather.GetComponent<GetWeatherData>();
            // Subscribe to the event
            weatherDataComponent.OnWeatherDataUpdated += UpdateWeatherInScene;
            weatherDataComponent.OnWeatherDataUpdated += UpdateHUDInScene;
        }
        else Debug.Log("No GetWeather object found.");

        sun = GameObject.FindGameObjectWithTag("SunLight");
        if (sun == null) Debug.Log("No SunLight GameObject found.");

    }

    void UpdateHUDInScene(WeatherData weatherData) {
  
        textLocation.text = "Location: " + weatherData.name;
        textTemperature.text = "Temperature: " + weatherData.main.temp + "°C";
        textWindSpeed.text = "Wind Speed: " + Math.Round(weatherData.wind.speed*3.6f,2) + " km/h"; // m/s naar km/h = snelheid * 3.6
        textRain.text = "Rain: " + weatherData.rain._1hr + " mm/hr";
        textSnow.text = "Snow: " + weatherData.snow._1hr + " mm/hr";
        textCloudDensity.text = "Cloud density: " + weatherData.clouds.all + " pct";
    }

    void UpdateWeatherInScene(WeatherData weatherData)
    {
        UpdateSunPosition(weatherData);
        DetermineWeatherEffects(weatherData);
    }

    void DetermineWeatherEffects(WeatherData weatherData) {


        // wind afhandelen
        ApplyWind(weatherData.wind.speed);

        // regen afhandelen
        ApplyRain(weatherData.rain._1hr);

        // sneeuw afhandelen
        ApplySnow(weatherData.snow._1hr);

        // wolken afhandelen
        ApplyClouds(weatherData.clouds.all);
    }


    void ApplyClouds(int clouds) {
        // clouds = 10; // ** ENABLE OM TE TESTEN MET DUMMY DATA **

        Debug.Log("*Cloud Density*: " + clouds + " pct");

        ParticleSystem.EmissionModule emission = cloudSystem.emission;
        // gedeeld door 3500f om handmatig tot iets te komen dat OK is
        emission.rateOverTime = clouds * (surfaceCompensation/3500f);
    }

    void ApplyWind(float windspeed) {
        // windspeed = 35f; // ** ENABLE OM TE TESTEN MET DUMMY DATA **

        Debug.Log("*Wind speed*:" + Math.Round(windspeed*3.6f,2) + "km/h");
        // shader variabelen aanpassen van het gras material
        movingGrassMaterial.SetVector("_WindSpeed", new Vector4(windspeed*3.6f, windspeed*3.6f,0,0));
        movingGrassMaterial.SetFloat("_WaveSpeed", (windspeed/5) + 5f);
    }

    void ApplySnow(float oneHourSnow) {
        //oneHourSnow = 5f; // ** ENABLE OM TE TESTEN MET DUMMY DATA **

        // surface compensation zorgt ervoor dat als de boundary plane van grootte
        // wordt gewijzigd, de emissie over de gehele oppervlakte wordt gecompenseert
        // zodat de invloed van het aanpassen van de boundary plane zijn grootte
        // minimaal of geen invloed heeft
        ParticleSystem.EmissionModule emission = snowSystem.emission;
        // gedeeld door 2.5f om handmatig tot iets te komen dat OK is
        Debug.Log("*Snow*: " + oneHourSnow);
        emission.rateOverTime = oneHourSnow * (surfaceCompensation/2.5f);
        Renderer objectRenderer = boundaryPlane.GetComponent<Renderer>();
        if (objectRenderer != null && snowMaterial != null && dryMaterial != null)
        {
            // vanaf een bepaalde hoeveelheid, sneeuw material toepassen op ondergrond
            if (oneHourSnow > 0.3f)
            {
                objectRenderer.material = snowMaterial;
                // snow particles comes here
            }
            else
            {
                objectRenderer.material = dryMaterial;
                // delete snow particles
            }

        }
        else Debug.Log("materials en/of objectrenderer niet OK!");
    }

    void ApplyRain(float oneHourRain) {
        //oneHourRain = 1.5f; // ** ENABLE OM TE TESTEN MET DUMMY DATA **

        // surface compensation zorgt ervoor dat als de boundary plane van grootte
        // wordt gewijzigd, de emissie over de gehele oppervlakte wordt gecompenseert
        // zodat de invloed van het aanpassen van de boundary plane zijn grootte
        // minimaal of geen invloed heeft
        Debug.Log("*Rain*: " + oneHourRain);
        ParticleSystem.EmissionModule emission = rainSystem.emission;
        // gedeeld door 2.5f om handmatig tot iets te komen dat OK is
        emission.rateOverTime = oneHourRain * (surfaceCompensation/2.5f);
    }

    void UpdateSunPosition(WeatherData weatherData)
    {
        bool night = false;
        long currentTime = GetUnixTimeNow();
        //Debug.Log("CURRENT TIME:" + currentTime);
        long sunrise = weatherData.sys.sunrise;
        long sunset = weatherData.sys.sunset;
        // soms is de data ouder dan 24u, compenseren tot het klopt
        //Debug.Log("CUR TIME - SUNRISE: " + (currentTime - sunrise));
        while (currentTime - sunrise > 86400) {
            //Debug.Log("current time -24h" + currentTime);
            currentTime -= 86400;
            //Debug.Log("new current time: " + currentTime);

        }
        if (currentTime < sunrise || currentTime > sunset)
        {
            //Debug.Log("nachtmodus");
            night = true;
        }
        // berekenen hoek
        // indien nacht is currentTime 12u verder of terug gedraaid
        // hierdoor weten we de hoek overdag, maar + 180 graden om terug 12u op te schuiven
        // aangezien +180 of -180 hetzelfde resultaat geeft enkel + gebruikt
        float dayProgress;
        if (night && currentTime < sunrise) {
            dayProgress = Mathf.InverseLerp(sunset-86400, sunrise, currentTime);            
        }
        else if (night && currentTime > sunset) {
            dayProgress = Mathf.InverseLerp(sunset, sunrise + 86400, currentTime);
        }
        else dayProgress = Mathf.InverseLerp(sunrise, sunset, currentTime);
        //Debug.Log("epoch sunrise:" + sunrise);
        //Debug.Log("epoch sunset: " + sunset);
        //Debug.Log("epoch current time: " + currentTime);
        //Debug.Log("day progress:" + dayProgress);
        float sunAngle = dayProgress * 180f;
        if (night) sunAngle += 180f;
        // zon ook wat schuin rond Y + Z axis om het wat realistischer te maken
        sun.transform.rotation = Quaternion.Euler(sunAngle, -25,25);
        //Debug.Log("Sun angle:" + sunAngle);
    }

        long GetUnixTimeNow()
        {
            // Return current time in Unix format
            return (long)(System.DateTime.UtcNow - new System.DateTime(1970, 1, 1)).TotalSeconds;
        }

    private void OnDestroy() {
        if (weatherDataComponent != null) {
            weatherDataComponent.OnWeatherDataUpdated -= UpdateWeatherInScene;
            weatherDataComponent.OnWeatherDataUpdated -= UpdateHUDInScene;
        }
    }
}