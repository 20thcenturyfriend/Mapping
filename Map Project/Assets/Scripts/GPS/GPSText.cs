using UnityEngine;
using UnityEngine.UI;

public class GPSText : MonoBehaviour
{
    public Text gpsText;
    public GPSManager gpsManager;
    public float updateInterval = 1f;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= updateInterval)
        {
            timer = 0f;
            double lat = gpsManager.Latitude;
            double lon = gpsManager.Longitude;

            // Also show how far from reference (roughly)
            double latDiff = (lat - gpsManager.referenceGPS.x) * 111000f;
            double lonDiff = (lon - gpsManager.referenceGPS.y) * 111000f;

            gpsText.text = $"Lat: {lat:F6}\nLon: {lon:F6}\ndeltaLat(m): {latDiff:F1}\ndeltaLon(m): {lonDiff:F1}";
        }
    }

}
