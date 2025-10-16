using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using ZXing;
using NativeGalleryNamespace;
using System.IO;

public class QRScanner : MonoBehaviour
{
    public RawImage cameraImage;
    public AspectRatioFitter aspectFitter;
    public Button cancelButton;
    public Button scanPhotosButton;

    private WebCamTexture webcamTexture;
    private BarcodeReader reader;
    private bool isScanning;

    public event Action<string> OnQRCodeScanned;

    public float scanRange = 3f;
    public Transform player;

    public GameMechanics gameMechanics;

    void Awake()
    {
        reader = new BarcodeReader();
        HideUI();
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
                StopScanning();
                OnQRCodeScanned?.Invoke(result.Text);
            }
            Destroy(snap);
        }
        catch { }
    }

    public void StartScanning()
    {
        //Proximity check BEFORE opening cam
        TargetNode nearestNode = gameMechanics != null ? gameMechanics.FindNearestNode() : null;
        if (nearestNode == null || Vector3.Distance(player.position, nearestNode.transform.position) > scanRange)
        {
            if (gameMechanics != null)
                gameMechanics.NotifyScanFailed("No QR target nearby bruh");
            return;
        }

        if (webcamTexture == null)
            webcamTexture = new WebCamTexture();

        cameraImage.texture = webcamTexture;
        webcamTexture.Play();
        ShowUI();
        isScanning = true;
    }

    public void StopScanning()
    {
        if (webcamTexture != null && webcamTexture.isPlaying)
            webcamTexture.Stop();

        HideUI();
        isScanning = false;
    }

    public void PickImageFromGallery()
    {
        StopScanning();
        NativeGallery.GetImageFromGallery((path) =>
        {
            if (path != null)
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
            OnQRCodeScanned?.Invoke(result.Text);

        Destroy(tex);
    }

    private void ShowUI()
    {
        cameraImage.enabled = true;
        cancelButton.gameObject.SetActive(true);
        scanPhotosButton.gameObject.SetActive(true);
    }

    private void HideUI()
    {
        cameraImage.enabled = false;
        cancelButton.gameObject.SetActive(false);
        scanPhotosButton.gameObject.SetActive(false);
    }
}
