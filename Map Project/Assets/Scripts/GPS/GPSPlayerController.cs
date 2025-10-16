using UnityEngine;

public class GPSPlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform player;        // rotates horizontally (yaw)
    public Transform cameraPivot;   // tilts vertically (pitch)

    [Header("GPS Settings")]
    public float metersPerDegree = 111000f * 10f;
    public float movementScale = 1f;

    [Header("Compass Settings")]
    [Range(0.01f, 1f)] public float rotationSmoothing = 0.15f;
    public float deadZoneDegrees = 2f;

    [Header("Camera Tilt Settings")]
    public float pitchMultiplier = 1.0f;
    public float maxPitchAngle = 60f;

    private double anchorLat;
    private double anchorLon;
    private Vector3 anchorPosition;
    private bool anchorSet = false;
    private float smoothedHeading = 0f;
    private float smoothedPitch = 0f;

    void Start()
    {
        Input.compass.enabled = true;
        Input.gyro.enabled = true;
        Input.gyro.updateInterval = 0.02f;
    }

    void Update()
    {
        // --- COMPASS YAW ---
        float rawHeading = Input.compass.trueHeading;
        if (rawHeading < 0) rawHeading += 360f;

        float delta = Mathf.DeltaAngle(smoothedHeading, rawHeading);
        if (Mathf.Abs(delta) > deadZoneDegrees)
            smoothedHeading = Mathf.LerpAngle(smoothedHeading, rawHeading, rotationSmoothing);

        // --- GYRO PITCH (only tilt, ignore yaw) ---
        Quaternion deviceRotation = Input.gyro.attitude;
        deviceRotation = new Quaternion(deviceRotation.x, deviceRotation.y, -deviceRotation.z, -deviceRotation.w);

        // Get only the phone's tilt angle (pitch)
        Vector3 euler = deviceRotation.eulerAngles;
        float rawPitch = euler.x;
        if (rawPitch > 180) rawPitch -= 360; // normalize to [-180,180]
        rawPitch *= pitchMultiplier;

        // Smooth and clamp
        smoothedPitch = Mathf.Lerp(smoothedPitch, rawPitch, rotationSmoothing);
        smoothedPitch = Mathf.Clamp(smoothedPitch, -maxPitchAngle, maxPitchAngle);

        // --- APPLY ROTATION ---
        if (player != null)
            player.rotation = Quaternion.Euler(0, smoothedHeading, 0);  // yaw only

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(smoothedPitch, 0, 0);  // pitch only

#if UNITY_ANDROID && !UNITY_EDITOR
        // --- GPS POSITION ---
        if (Input.location.status == LocationServiceStatus.Running)
        {
            var data = Input.location.lastData;

            if (!anchorSet)
            {
                anchorLat = data.latitude;
                anchorLon = data.longitude;
                anchorPosition = player.position;
                anchorSet = true;
                Debug.Log($"GPS Anchor Set at {anchorLat}, {anchorLon}");
            }

            float deltaZ = (float)((data.latitude - anchorLat) * metersPerDegree);
            float deltaX = (float)((data.longitude - anchorLon) * metersPerDegree * Mathf.Cos((float)(anchorLat * Mathf.Deg2Rad)));

            player.position = anchorPosition + new Vector3(deltaX * movementScale, 0, deltaZ * movementScale);
        }
#endif
    }
}
