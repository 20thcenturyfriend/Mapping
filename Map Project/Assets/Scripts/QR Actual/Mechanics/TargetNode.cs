using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class MainPath
{
    public TargetNode destination;
    public List<Transform> waypoints = new List<Transform>();
}

public class TargetNode : MonoBehaviour
{
    public List<TargetNode> connectedNodes = new List<TargetNode>();
    [Tooltip("The exact QR code string goes here")]
    public string expectedQRCode;
    [Tooltip("Main path via waypoints")]
    public List<MainPath> mainPaths = new List<MainPath>();
    public List<TargetNode> GetConnections()
    {
        return connectedNodes;
    }
}
