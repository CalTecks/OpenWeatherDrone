using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using UnityEngine.UI;
using System;

// Hier worden de API get's uitgevoerd en ook de JSON response omgezet naar een WeatherData object
// Er kan gesubscribed worden op het event OnWeatherDataUpdated
// WeatherManager maakt hier gebruik van

public class GetWeatherData : MonoBehaviour
{
    public event Action<WeatherData> OnWeatherDataUpdated;
    public string API_key = "a85d2d7bf37351d3763a2363d940f08f";
    private string durbuyURL, deathValleyURL, kirenskURL;
    private string jsonResponse;
    public WeatherData weatherData;
    public Button buttonDurbuy;
    public Button buttonDeathValley;
    public Button buttonKirensk;

    void Start()
    {
        // Kan eventueel ook anders, met naam van een stad ipv longitude en lattitude
        durbuyURL = "https://api.openweathermap.org/data/2.5/weather?lat=50.21&lon=5.27&appid=" + 
        API_key + "&units=metric";
        deathValleyURL = "https://api.openweathermap.org/data/2.5/weather?lat=36.35&lon=-117.6&appid=" + 
        API_key + "&units=metric";
        kirenskURL = "https://api.openweathermap.org/data/2.5/weather?lat=57.47&lon=108.6&appid=" + 
        API_key + "&units=metric";
        // bij opstart Durbuy als default gebruiken
        StartCoroutine(GetRequest(durbuyURL));
        buttonDurbuy.onClick.AddListener(() => StartCoroutine(GetRequest(durbuyURL)));
        buttonDeathValley.onClick.AddListener(() => StartCoroutine(GetRequest(deathValleyURL)));
        buttonKirensk.onClick.AddListener(() => StartCoroutine(GetRequest(kirenskURL)));
    }

    IEnumerator GetRequest(string URL) {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(URL))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();

            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError(URL + ": Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError(URL + ": HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    jsonResponse = webRequest.downloadHandler.text;
                    Debug.Log(URL + ":\nReceived: \n" + webRequest.downloadHandler.text);
                    JsonToObject();
                    break;

            }
        }
    }
    void JsonToObject() {
        weatherData = JsonUtility.FromJson<WeatherData>(jsonResponse);
        //Debug.Log("Weatherdata object created. Some parameters:");
        //Debug.Log("Location: " + weatherData.name);
        //Debug.Log("Wind speed: " + weatherData.wind.speed);
        //Debug.Log("Temperature: " + weatherData.main.temp + "°C");
        weatherData = JsonUtility.FromJson<WeatherData>(jsonResponse);
        if (weatherData != null)
        {
            OnWeatherDataUpdated?.Invoke(weatherData);
        }
        else
        {
            Debug.LogError("WeatherData is null, cannot invoke event.");
        }
    }
}
