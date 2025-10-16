//using System.Collections.Generic;
//using UnityEngine;

//public class QRLocationManager : MonoBehaviour
//{
//    public static QRLocationManager Instance;

//    [SerializeField]
//    private List<QRLocationEntry> qrLocations;

//    private Dictionary<string, Transform> qrToLocationMap;

//    private void Awake()
//    {
//        Instance = this;

//        // Build the dictionary from the list
//        qrToLocationMap = new Dictionary<string, Transform>();
//        foreach (var entry in qrLocations)
//        {
//            if (!qrToLocationMap.ContainsKey(entry.qrCode) && entry.targetLocation != null)
//            {
//                qrToLocationMap.Add(entry.qrCode, entry.targetLocation);
//            }
//        }
//    }

//    public bool TryGetPosition(string qrCode, out Vector3 position)
//    {
//        if (qrToLocationMap.TryGetValue(qrCode, out Transform location))
//        {
//            position = location.position;
//            return true;
//        }

//        position = Vector3.zero;
//        return false;
//    }
//}


using System.Collections.Generic;
using UnityEngine;

public class QRLocationManager : MonoBehaviour
{
    public static QRLocationManager Instance;

    [System.Serializable]
    public class QRLocationEntry
    {
        public string qrCode;
        public TargetNode targetNode;
    }

    [SerializeField]
    public List<QRLocationEntry> qrLocations = new List<QRLocationEntry>();

    private Dictionary<string, TargetNode> qrToNodeMap;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        qrToNodeMap = new Dictionary<string, TargetNode>();

        foreach (var entry in qrLocations)
        {
            if (!string.IsNullOrEmpty(entry.qrCode) && entry.targetNode != null)
            {
                qrToNodeMap[entry.qrCode] = entry.targetNode;
            }
        }
    }

    // Return true and node if found
    public bool TryGetTargetNode(string qrCode, out TargetNode node)
    {
        return qrToNodeMap.TryGetValue(qrCode, out node);
    }
}
