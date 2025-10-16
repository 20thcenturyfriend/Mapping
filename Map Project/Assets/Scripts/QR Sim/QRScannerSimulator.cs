//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.UI; 
//public class QRScannerSimulator : MonoBehaviour
//{
//    public Transform player;
//    public float nodeDetectionRadius = 1f;
//    public Text scanmesText;
//    private TargetNode currentNode;
//    private List<TargetNode> connectedNodes = new List<TargetNode>();
//    private int targetIndex = 0;
//    private bool isPathVisible = false;

//    void Update()
//    {
//        //if (Input.GetKeyDown(KeyCode.O))
//        //{
//        //    CycleOrTogglePath();
//        //}
//    }

//    public void CycleOrTogglePath()
//    {
//        TargetNode nearestNode = FindNearestNode();

//        if (nearestNode == null)
//        {
//            Debug.LogWarning("No nearby target node found.");
//            ShowScanMessage("No QR target nearby.");
//            ClearPath();
//            return;
//        }
//        if (nearestNode != currentNode)
//        {
//            currentNode = nearestNode;
//            connectedNodes = currentNode.GetConnections();
//            targetIndex = 0;
//            isPathVisible = false;  // reset visibility on new node
//        }

//        if (connectedNodes == null || connectedNodes.Count == 0)
//        {
//            Debug.Log("Current node has no connected targets.");
//            ShowScanMessage("This QR has no connections.");
//            ClearPath();
//            return;
//        }

//        if (!isPathVisible)
//        {
//            // Show path to current target
//            TargetNode nextTarget = connectedNodes[targetIndex];
//            DrawPathToTarget(nextTarget);
//            isPathVisible = true;
//        }
//        else
//        {
//            // Path is visible, cycle to next or turn off
//            targetIndex++;
//            if (targetIndex >= connectedNodes.Count)
//            {
//                // Reached the end � clear path and reset cycle
//                ClearPath();
//                targetIndex = 0;
//                isPathVisible = false;
//            }
//            else
//            {
//                // Draw path to next target
//                TargetNode nextTarget = connectedNodes[targetIndex];
//                DrawPathToTarget(nextTarget);
//                isPathVisible = true;
//            }
//        }
//    }

//    TargetNode FindNearestNode()
//    {
//        TargetNode[] allNodes = FindObjectsOfType<TargetNode>();
//        TargetNode nearest = null;
//        float minDist = nodeDetectionRadius;

//        foreach (TargetNode node in allNodes)
//        {
//            float dist = Vector3.Distance(player.position, node.transform.position);
//            if (dist < minDist)
//            {
//                minDist = dist;
//                nearest = node;
//            }
//        }

//        return nearest;
//    }

//    List<Vector3> CalculateNavMeshPath(Vector3 start, Vector3 end)
//    {
//        UnityEngine.AI.NavMeshPath navMeshPath = new UnityEngine.AI.NavMeshPath();
//        if (UnityEngine.AI.NavMesh.CalculatePath(start, end, UnityEngine.AI.NavMesh.AllAreas, navMeshPath))
//        {
//            return new List<Vector3>(navMeshPath.corners);
//        }

//        return null;
//    }

//    void DrawPathToTarget(TargetNode target)
//    {
//        TargetNode currentNode = FindNearestNode();
//        if (currentNode == null || target == null)
//        {
//            Debug.LogWarning("Current or target node is null.");
//            ShowScanMessage("Unable to find path.");
//            return;
//        }

//        Vector3 startPosition = currentNode.transform.position;
//        Vector3 endPosition = target.transform.position;

//        List<Vector3> path = CalculateNavMeshPath(startPosition, endPosition);

//        if (path != null && path.Count > 0)
//        {
//            FindObjectOfType<PathVisualizer>()?.DrawPath(path);
//            Debug.Log($"Path drawn from {currentNode.name} to: {target.name}");
//            ShowScanMessage($"Generating path to {target.name}...");
//        }
//        else
//        {
//            Debug.LogWarning("No path found to target.");
//            ShowScanMessage("Unable to find path.");
//        }
//    }


//    public void ShowScanMessage(string message)
//    {
//        if (scanmesText != null)
//        {
//            scanmesText.text = message;
//            StopAllCoroutines();
//            StartCoroutine(ClearMessageAfterDelay(2f));
//        }
//    }

//    IEnumerator ClearMessageAfterDelay(float delay)
//    {
//        yield return new WaitForSeconds(delay);
//        scanmesText.text = "";
//    }

//    void ClearPath()
//    {
//        FindObjectOfType<PathVisualizer>()?.Clear();
//        Debug.Log("Path cleared.");
//    }

//    public bool IsPlayerNearNode()
//    {
//        return FindNearestNode() != null;
//    }

//    public bool CheckAndDrawPath(string scannedData)
//    {
//        TargetNode nearestNode = FindNearestNode();

//        if (nearestNode == null)
//        {
//            ShowScanMessage("No QR target nearby.");
//            return false;
//        }

//        if (nearestNode.expectedQRCode != scannedData)
//        {
//            ShowScanMessage("Wrong QR code for this target.");
//            return false;
//        }

//        // It's the correct QR code for this node
//        currentNode = nearestNode;
//        connectedNodes = currentNode.GetConnections();
//        targetIndex = 0;
//        isPathVisible = false;

//        CycleOrTogglePath(); // Draw the path to the connected node
//        return true;
//    }

//}



