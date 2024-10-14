using UnityEngine;


// De JSON data wordt omgezet in een hoofdklasse met subklasses
// Niet alle gegevens zijn perse gebruikt en/of aanwezig

[System.Serializable]
public class WeatherData
{
    public Coord coord;
    public Main main;
    public Wind wind;
    public Rain rain;
    public Snow snow;
    public Clouds clouds;
    public Sys sys;
    public int timezone;
    public string name;
}

[System.Serializable]
public class Coord
{
    public float lon;
    public float lat;
}

[System.Serializable]
public class Main
{
    public float temp;
}
[System.Serializable]
public class Wind
{
    public float speed;
}
[System.Serializable]
public class Rain {
    [UnityEngine.Serialization.FormerlySerializedAs("1h")]
    public float _1hr;
}

[System.Serializable]
public class Snow {
    [UnityEngine.Serialization.FormerlySerializedAs("1h")]
    public float _1hr;
}

[System.Serializable]
public class Clouds {
    public int all;
}
[System.Serializable]
public class Sys {
    public string country;
    public long sunrise;
    public long sunset;
}