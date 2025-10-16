using UnityEngine;

public class GameMechanics : MonoBehaviour
{
    public QRScanner qrScanner;
    public PathManager pathManager;
    public ScanMessageUI messageUI;

    private TargetNode currentNode;

    void Start()
    {
        qrScanner.OnQRCodeScanned += HandleQRCode;
    }

    private void HandleQRCode(string data)
    {
        TargetNode nearestNode = FindNearestNode();
        if (nearestNode == null)
        {
            messageUI.ShowMessage("No QR target nearby rip bozo", Color.red);
            return;
        }

        if (nearestNode.expectedQRCode != data)
        {
            messageUI.ShowMessage("Wrong QR code man", Color.red);
            return;
        }

        currentNode = nearestNode;
        if (currentNode.connectedNodes.Count > 0)
        {
            TargetNode nextTarget = currentNode.connectedNodes[0];
            pathManager.DrawPath(currentNode, nextTarget);
            messageUI.ShowMessage("Radar alert...Path found!", Color.green);
        }
    }

    public void NotifyScanFailed(string reason)
    {
        if (messageUI != null)
            messageUI.ShowMessage(reason, Color.red);
    }

    public TargetNode FindNearestNode()
    {
        TargetNode[] allNodes = FindObjectsOfType<TargetNode>();
        TargetNode nearest = null;
        float minDist = 1.5f; 

        foreach (TargetNode node in allNodes)
        {
            float dist = Vector3.Distance(pathManager.player.position, node.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = node;
            }
        }
        return nearest;
    }
}
