using System.Buffers.Text;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class WaypointSelector : MonoBehaviour
{
    /// From the player's position, it finds the best possible wps within a certain radius that leads to the goal
    public float searchRadius = 5f; //How far from player to look for wps
    //Returns the best path from playerPosition to the goal wp
    public List<Vector3> FindBestPath(Vector3 playerPosition, Waypoint goal)
    {
        Waypoint[] allWaypoints = FindObjectsOfType<Waypoint>();
        Waypoint bestStart = null;
        float bestPathLength = Mathf.Infinity;
        List<Vector3> bestPath = null;

        foreach (Waypoint wp in allWaypoints)
        {
            float distToPlayer = Vector3.Distance(playerPosition, wp.Position);
            if (distToPlayer <= searchRadius)
            {
                //Make sure NavMesh says this wp is reachable from player
                if (!IsReachableOnNavMesh(playerPosition, wp.Position))
                    continue;
                //Find path from this candidate wp to the goal 
                List<Vector3> path = WaypointPathfinder.FindPath(wp, goal);
                if (path != null)
                {
                    //Calculate total path length: from player to wp + wp -> goal path length 
                    float pathLength = distToPlayer + CalculatePathLength(path);
                    if (pathLength < bestPathLength)
                    {
                        bestPathLength = pathLength;
                        bestStart = wp;
                        bestPath = path;
                    }
                }
            }
        }
        if (bestStart == null)
        {
            Debug.LogWarning("No suitable starting wp found near you, get a grip");
            return null;
        }
        //Insert player position as first point so path starts from player
        bestPath.Insert(0, playerPosition);
        return bestPath;
    }

    private bool IsReachableOnNavMesh(Vector3 from, Vector3 to)
    {
        NavMeshHit hitFrom, hitTo;

        //Snap both points onto the NavMesh (within 2 units tolerance)
        if (!NavMesh.SamplePosition(from, out hitFrom, 2f, NavMesh.AllAreas)) return false;
        if (!NavMesh.SamplePosition(to, out hitTo, 2f, NavMesh.AllAreas)) return false;
        //Try to compute a path along the NavMesh
        NavMeshPath navPath = new NavMeshPath();
        if (NavMesh.CalculatePath(hitFrom.position, hitTo.position, NavMesh.AllAreas, navPath))
        {
            return navPath.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }
    private float CalculatePathLength(List<Vector3> path)
    {
        float length = 0f;
        for (int i = 0; i < path.Count - 1; i++)
        {
            length += Vector3.Distance(path[i], path[i + 1]);
        }
        return length;
    }

    private Transform player;
    private void OnDrawGizmos()
    {
        if (player == null)
        {
            PathManager manager = GetComponent<PathManager>();
            if (manager != null)
                player = manager.player;
        }
        if (player == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(player.position, searchRadius);
    }
}