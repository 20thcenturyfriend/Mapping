using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using NativeGalleryNamespace;
using System.IO;

public class Mechanics : MonoBehaviour
{
    [Header("Player & Node Settings")]
    public Transform player;
    public float nodeDetectionRadius = 1f;

    [Header("QR Camera UI")]
    public RawImage cameraImage;
    public AspectRatioFitter aspectFitter;
    public Button cancelButton;
    public Button scanPhotosButton;
    public Text scanmesText;

    private WebCamTexture webcamTexture;
    private BarcodeReader reader;
    private bool isScanning = false;

    private TargetNode currentNode;
    private List<TargetNode> connectedNodes = new List<TargetNode>();
    private int targetIndex = 0;
    private bool isPathVisible = false;

    private List<List<Vector3>> allPaths = new List<List<Vector3>>();
    private float lastRerouteTime = -10f;
    private float rerouteCooldown = 3f; // in seconds

    void Start()
    {
        cameraImage.enabled = false;
        cancelButton.gameObject.SetActive(false);
        scanPhotosButton.gameObject.SetActive(false);
        reader = new BarcodeReader();
    }

    void Update()
    {
        if (!isScanning || webcamTexture == null || webcamTexture.width < 100) return;
        float ratio = (float)webcamTexture.width / webcamTexture.height;
        aspectFitter.aspectRatio = ratio;
        cameraImage.rectTransform.localEulerAngles = new Vector3(0, 0, -webcamTexture.videoRotationAngle);
        Vector3 scale = cameraImage.rectTransform.localScale;
        scale.y = webcamTexture.videoVerticallyMirrored ? -1f : 1f;
        cameraImage.rectTransform.localScale = scale;
        try
        {
            var snap = new Texture2D(webcamTexture.width, webcamTexture.height);
            snap.SetPixels32(webcamTexture.GetPixels32());
            snap.Apply();
            var result = reader.Decode(snap.GetPixels32(), snap.width, snap.height);
            if (result != null)
            {
                Debug.Log("QR Code Detected: " + result.Text);
                HandleQRCode(result.Text);
            }
            Destroy(snap);
        }
        catch { }
    }

    [SerializeField] private float rerouteThreshold = 5f;

    void LateUpdate()
    {
        if (!isPathVisible || allPaths.Count == 0) return;

        List<Vector3> currentPath = allPaths[targetIndex == 0 ? allPaths.Count - 1 : targetIndex - 1]; // Use current shown path
        float minDistance = float.MaxValue;

        foreach (var point in currentPath)
        {
            float dist = Vector3.Distance(player.position, point);
            if (dist < minDistance)
            {
                minDistance = dist;
            }
        }

        if (minDistance > rerouteThreshold && Time.time - lastRerouteTime > rerouteCooldown)
        {
            Debug.Log("Player deviated from path. Rerouting...");
            HandleRerouting();
            lastRerouteTime = Time.time;
        }

    }

    private void LoadMainPath(TargetNode start, TargetNode destination)
    {
        allPaths.Clear();

        Waypoint startwp = FindClosestWaypoint(player.position);
        Waypoint end = FindClosestWaypoint(destination.transform.position);

        if (start == null || end == null)
        {
            Debug.LogWarning("Could not find valid waypoints for path.");
            return;
        }

        List<Vector3> mainPath = WaypointPathfinder.FindPath(startwp, end);

        if (mainPath != null && mainPath.Count > 0)
        {
            // Optional: insert player position for smooth start
            mainPath.Insert(0, player.position);

            // Optional: insert destination position at end (if not close enough)
            if (Vector3.Distance(mainPath[mainPath.Count - 1], destination.transform.position) > 0.1f)
            {
                mainPath.Add(destination.transform.position);
            }

            allPaths.Add(mainPath);
            Debug.Log($"Loaded main waypoint path from {start.name} to {destination.name}");
        }
        else
        {
            Debug.LogWarning($"No waypoint path found from {start.name} to {destination.name}");
        }
    }

    private void HandleRerouting()
    {
        TargetNode destination = connectedNodes.Count > 0 ? connectedNodes[0] : null;
        if (destination == null) return;

        Waypoint start = FindClosestWaypoint(player.position);
        Waypoint end = FindClosestWaypoint(destination.transform.position);

        if (start == null || end == null) return;

        List<Vector3> reroutePath = WaypointPathfinder.FindPath(start, end);

        if (reroutePath != null && reroutePath.Count > 0)
        {
            reroutePath.Insert(0, player.position);

            if (Vector3.Distance(reroutePath[reroutePath.Count - 1], destination.transform.position) > 0.1f)
            {
                reroutePath.Add(destination.transform.position);
            }

            allPaths.Clear();
            allPaths.Add(reroutePath);
            targetIndex = 0;

            FindObjectOfType<PathVisualizer>()?.DrawPath(reroutePath);
            ShowScanMessage("You deviated from the path! Rerouting...", Color.yellow);
        }
    }

    private Waypoint FindClosestWaypoint(Vector3 position)
    {
        Waypoint[] waypoints = FindObjectsOfType<Waypoint>();
        Waypoint closest = null;
        float minDist = Mathf.Infinity;

        Vector3 playerForward = player.forward;
        float maxAngle = 100f;

        foreach (var wp in waypoints)
        {
            Vector3 toWaypoint = (wp.Position - position).normalized;
            float angle = Vector3.Angle(playerForward, toWaypoint);

            if (angle < maxAngle)
            {
                float dist = Vector3.Distance(position, wp.Position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = wp;
                }
            }
        }

        if (closest == null)
        {
            float fallbackDist = Mathf.Infinity;
            foreach (var wp in waypoints)
            {
                float dist = Vector3.Distance(position, wp.Position);
                if (dist < fallbackDist)
                {
                    fallbackDist = dist;
                    closest = wp;
                }
            }
        }

        return closest;
    }


    public void StartScanning()
    {
        if (!IsPlayerNearNode())
        {
            ShowScanMessage("Move closer to a QR target.");
            return;
        }

        if (webcamTexture == null)
            webcamTexture = new WebCamTexture();

        cameraImage.texture = webcamTexture;
        cameraImage.material.mainTexture = webcamTexture;
        webcamTexture.Play();
        cameraImage.enabled = true;
        cancelButton.gameObject.SetActive(true);
        scanPhotosButton.gameObject.SetActive(true);
        isScanning = true;
    }

    public void StopScanning()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
        {
            webcamTexture.Stop();
        }

        cameraImage.enabled = false;
        cancelButton.gameObject.SetActive(false);
        scanPhotosButton.gameObject.SetActive(false);
        isScanning = false;
    }

    private void HandleQRCode(string data)
    {
        StopScanning();

        if (CheckAndDrawPath(data))
        {
            Debug.Log("Correct QR");
        }
        else
        {
            Debug.LogWarning("Wrong QR bruh");
        }
    }

    private bool CheckAndDrawPath(string scannedData)
    {
        TargetNode nearestNode = FindNearestNode();

        if (nearestNode == null)
        {
            ShowScanMessage("No QR target nearby rip", UnityEngine.Color.red);
            return false;
        }

        if (nearestNode.expectedQRCode != scannedData)
        {
            ShowScanMessage("Wrong QR code for this target bruh", UnityEngine.Color.red);
            return false;
        }

        currentNode = nearestNode;
        connectedNodes = currentNode.connectedNodes;
        targetIndex = 0;
        isPathVisible = false;

        if (connectedNodes.Count > 0)
        {
            TargetNode nextTarget = connectedNodes[0];
            LoadMainPath(currentNode, nextTarget);
        }

        CycleOrTogglePath();
        return true;
    }

    private void CycleOrTogglePath()
    {
        if (allPaths == null || allPaths.Count == 0)
        {
            ShowScanMessage("No path available", Color.red);
            ClearPath();
            return;
        }

        ClearPath();

        List<Vector3> currentPath = allPaths[0];
        FindObjectOfType<PathVisualizer>()?.DrawPath(currentPath);
        isPathVisible = true;

        ShowScanMessage("Drawing path...", Color.green);
    }

    private void DrawPathToTarget(TargetNode target)
    {
        TargetNode currentNode = FindNearestNode();
        if (currentNode == null || target == null)
        {
            ShowScanMessage("Unable to find path rip bozo", UnityEngine.Color.red);
            return;
        }

        Vector3 startPosition = currentNode.transform.position;
        Vector3 endPosition = target.transform.position;

        List<Vector3> path = CalculateNavMeshPath(startPosition, endPosition);

        if (path != null && path.Count > 0)
        {
            FindObjectOfType<PathVisualizer>()?.DrawPath(path);
            ShowScanMessage($"Generating path to... {target.name}", UnityEngine.Color.green);
        }
        else
        {
            ShowScanMessage("Unable to find path rip bozo", UnityEngine.Color.red);
        }
    }

    public void PickImageFromGallery()
    {
        if (!IsPlayerNearNode())
        {
            ShowScanMessage("Move closer to a QR target", UnityEngine.Color.white);
            return;
        }

        StopScanning();

        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path == null)
            {
                ShowScanMessage("No image selected", UnityEngine.Color.red);
                return;
            }

            StartCoroutine(ProcessQRCodeFromImage(path));
        }, "Select an image with a QR code");
    }

    private IEnumerator ProcessQRCodeFromImage(string path)
    {
        byte[] imageData = File.ReadAllBytes(path);
        Texture2D tex = new Texture2D(2, 2);
        tex.LoadImage(imageData);

        yield return null;

        var result = reader.Decode(tex.GetPixels32(), tex.width, tex.height);
        if (result != null)
        {
            Debug.Log("QR code from gallery: " + result.Text);
            HandleQRCode(result.Text);
        }
        else
        {
            ShowScanMessage("No QR code detected in image numbnuts", UnityEngine.Color.red);
        }

        Destroy(tex);
    }

    private List<Vector3> CalculateNavMeshPath(Vector3 start, Vector3 end)
    {
        UnityEngine.AI.NavMeshPath navMeshPath = new UnityEngine.AI.NavMeshPath();
        if (UnityEngine.AI.NavMesh.CalculatePath(start, end, UnityEngine.AI.NavMesh.AllAreas, navMeshPath))
        {
            return new List<Vector3>(navMeshPath.corners);
        }
        return null;
    }

    private TargetNode FindNearestNode()
    {
        TargetNode[] allNodes = FindObjectsOfType<TargetNode>();
        TargetNode nearest = null;
        float minDist = nodeDetectionRadius;

        foreach (TargetNode node in allNodes)
        {
            float dist = Vector3.Distance(player.position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = node;
            }
        }

        return nearest;
    }

    private bool IsPlayerNearNode()
    {
        return FindNearestNode() != null;
    }

    public void ShowScanMessage(string message, UnityEngine.Color? color = null)
    {
        if (scanmesText != null)
        {
            scanmesText.text = message;
            scanmesText.color = color ?? UnityEngine.Color.white;
            scanmesText.gameObject.SetActive(true);
            StopAllCoroutines();
            StartCoroutine(AnimateScanMessage());
        }
    }

    private IEnumerator ClearMessageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        scanmesText.text = "";
    }

    private void ClearPath()
    {
        FindObjectOfType<PathVisualizer>()?.Clear();
    }

    private IEnumerator AnimateScanMessage()
    {
        float duration = 2.4f;
        float elapsed = 0f;

        Vector3 originalScale = Vector3.one;
        Vector3 popScale = Vector3.one * 1.5f;
        Vector3 startPos = scanmesText.rectTransform.anchoredPosition;
        Vector3 endPos = startPos + new Vector3(0, 70f, 0);

        Color originalColor = scanmesText.color;
        Color fadeColor = originalColor;

        scanmesText.rectTransform.localScale = Vector3.zero;

        float popTime = 0.2f;
        while (elapsed < popTime)
        {
            float t = elapsed / popTime;
            scanmesText.rectTransform.localScale = Vector3.Lerp(Vector3.zero, popScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        scanmesText.rectTransform.localScale = originalScale;

        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            scanmesText.rectTransform.anchoredPosition = Vector3.Lerp(startPos, endPos, t);

            fadeColor.a = Mathf.Lerp(1f, 0f, t);
            scanmesText.color = fadeColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        scanmesText.text = "";
        scanmesText.color = originalColor;
        scanmesText.rectTransform.anchoredPosition = startPos;
        scanmesText.rectTransform.localScale = originalScale;
        scanmesText.gameObject.SetActive(false);
    }
}
