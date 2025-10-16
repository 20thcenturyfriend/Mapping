using UnityEngine;
using System.Collections;

#if UNITY_ANDROID && !UNITY_EDITOR
using UnityEngine.Android;
#endif

public class GPSManager : MonoBehaviour
{
    public static GPSManager Instance;

    [Header("Simulated GPS (Editor)")]
    public bool simulateInEditor = true;
    public Transform playerTransform;
    public Vector2 referenceGPS = new Vector2(21.02814156872187f, 105.83567415357955f); // starting lat/lon
    public Vector3 referencePosition; // Unity world anchor position
    public float metersPerUnityUnit = 1.0f;

    [Header("Filtering")]
    [Range(0.01f, 1f)]
    public float smoothingFactor = 0.1f; // smaller = smoother movement
    public float stableThresholdMeters = 2f; // threshold to decide "stable fix"

    private double currentLat;
    private double currentLon;
    private Vector2 smoothedGPS;
    private bool anchorSet = false;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        StartCoroutine(StartRealGPS());
#else
        if (simulateInEditor)
        {
            if (playerTransform != null)
            {
                referencePosition = playerTransform.position;
                currentLat = referenceGPS.x;
                currentLon = referenceGPS.y;
                Debug.Log("[GPSManager] Simulated GPS started from reference position.");
            }
            else
                Debug.LogWarning("[GPSManager] Player transform not assigned!");
        }
#endif
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (simulateInEditor && playerTransform != null)
        {
            Vector3 offset = playerTransform.position - referencePosition;
            float offsetMetersX = offset.x * metersPerUnityUnit;
            float offsetMetersZ = offset.z * metersPerUnityUnit;

            currentLat = referenceGPS.x + (offsetMetersZ / 111000f);
            currentLon = referenceGPS.y + (offsetMetersX / 111000f);

            // Debug.Log($"[SIM GPS] Lat: {currentLat:F6}, Lon: {currentLon:F6}");
        }
#endif
    }

#if UNITY_ANDROID && !UNITY_EDITOR
    IEnumerator StartRealGPS()
    {
        // Ask for location permission
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Debug.Log("[GPSManager] Requesting location permission...");
            Permission.RequestUserPermission(Permission.FineLocation);
            yield return new WaitForSeconds(2f);
        }

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogWarning("[GPSManager] GPS not enabled by user!");
            yield break;
        }

        Input.location.Start(1f, 0.1f);

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (Input.location.status != LocationServiceStatus.Running)
        {
            Debug.LogWarning("[GPSManager] GPS failed to start.");
            yield break;
        }

        Debug.Log("[GPSManager] GPS running. Waiting for stable fix...");

        // Wait for a stable GPS fix before setting anchor
        Vector2 lastFix = Vector2.zero;
        int stableCount = 0;

        while (stableCount < 3)
        {
            var data = Input.location.lastData;
            Vector2 currentFix = new Vector2((float)data.latitude, (float)data.longitude);

            if (lastFix != Vector2.zero)
            {
                float metersDiff = Vector2.Distance(GPS2Meters(lastFix, currentFix), Vector2.zero);
                if (metersDiff < stableThresholdMeters)
                    stableCount++;
                else
                    stableCount = 0;
            }

            lastFix = currentFix;
            Debug.Log($"[GPSManager] Waiting for stable fix... {stableCount}/3");
            yield return new WaitForSeconds(1f);
        }

        referenceGPS = lastFix;
        if (playerTransform != null)
            referencePosition = playerTransform.position;

        anchorSet = true;
        smoothedGPS = referenceGPS;
        Debug.Log($"[GPSManager] Stable anchor set at {referenceGPS.x:F6}, {referenceGPS.y:F6}");

        // Now start reading GPS updates
        while (true)
        {
            var data = Input.location.lastData;
            Vector2 rawGPS = new Vector2((float)data.latitude, (float)data.longitude);

            // Smooth the GPS reading
            smoothedGPS = Vector2.Lerp(smoothedGPS, rawGPS, smoothingFactor);

            currentLat = smoothedGPS.x;
            currentLon = smoothedGPS.y;

            yield return new WaitForSeconds(1f);
        }
    }

    // Approximate conversion between lat/lon difference → meters (rough)
    Vector2 GPS2Meters(Vector2 from, Vector2 to)
    {
        float latDiff = (to.x - from.x) * 111000f;
        float lonDiff = (to.y - from.y) * 111000f * Mathf.Cos(from.x * Mathf.Deg2Rad);
        return new Vector2(lonDiff, latDiff);
    }
#endif

    public double Latitude => currentLat;
    public double Longitude => currentLon;
}
