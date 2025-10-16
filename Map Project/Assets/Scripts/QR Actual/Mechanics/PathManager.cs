using System.Collections.Generic;
using UnityEngine;

public class PathManager : MonoBehaviour
{
    public Transform player;
    public PathVisualizer visualizer;
    public WaypointSelector selector;

    [SerializeField] private float rerouteThreshold = 2.5f;
    [SerializeField] private float rerouteCooldown = 1f;

    private float lastRerouteTime;
    private List<Vector3> fullPath;     //The actual reference path
    private List<Vector3> trimmedPath;  //The drawn path
    private TargetNode currentTarget;

    ///Draw path from player to target using WaypointSelector.
    public void DrawPath(TargetNode start, TargetNode end)
    {
        currentTarget = end; //Save destination for reroutes
        Waypoint endWp = FindClosestWaypoint(end.transform.position);
        if (endWp == null) return;
        //Build path from player -> endWp
        List<Vector3> path = selector.FindBestPath(player.position, endWp);
        if (path == null || path.Count == 0) return;
        //Add player as start and final target at the end
        path.Insert(0, player.position);
        path.Add(end.transform.position);
        fullPath = path;
        trimmedPath = new List<Vector3>(fullPath);
        visualizer.DrawPath(trimmedPath);
    }

    private void LateUpdate()
    {
        if (fullPath == null || fullPath.Count == 0 || currentTarget == null) return;
        //Always trim the drawn path
        TrimPathBehindPlayer();
        //Check if player strayed too far from reference path
        float minDist = float.MaxValue;
        foreach (var point in fullPath)
            minDist = Mathf.Min(minDist, Vector3.Distance(player.position, point));
        if (minDist > rerouteThreshold && Time.time - lastRerouteTime > rerouteCooldown)
        {
            Debug.Log("Beep beep. Rerouting...");
            lastRerouteTime = Time.time;
            Reroute();
        }
    }

    private void Reroute()
    {
        Waypoint startWp = FindClosestWaypoint(player.position);
        Waypoint endWp = FindClosestWaypoint(currentTarget.transform.position);
        if (startWp == null || endWp == null) return;
        //Build fresh path
        List<Vector3> path = selector.FindBestPath(player.position, endWp);
        if (path == null || path.Count == 0) return;
        path.Insert(0, player.position);
        path.Add(currentTarget.transform.position);
        fullPath = path;
        trimmedPath = new List<Vector3>(fullPath);
        visualizer.DrawPath(trimmedPath);
    }

    private Waypoint FindClosestWaypoint(Vector3 position)
    {
        Waypoint[] waypoints = FindObjectsOfType<Waypoint>();
        Waypoint closest = null;
        float minDist = Mathf.Infinity;

        foreach (var wp in waypoints)
        {
            float dist = Vector3.Distance(position, wp.Position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = wp;
            }
        }
        return closest;
    }
   private void TrimPathBehindPlayer()
{
    if (trimmedPath == null || trimmedPath.Count < 2) return;

        //Always keep the path starting at the players position
        trimmedPath[0] = player.position;

    //Try trimming wps if they’re "behind" the player
    for (int i = 1; i < trimmedPath.Count - 1; i++)
    {
        if (HasPassedWaypoint(player.position, trimmedPath[i], trimmedPath[i + 1]))
        {
            trimmedPath.RemoveAt(i);
            i--; //Stay in sync
        }
        else break; //Stop once we find the first waypoint not passed yet
    }

    visualizer.DrawPath(trimmedPath);
}

private bool HasPassedWaypoint(Vector3 playerPos, Vector3 wpPos, Vector3 nextWpPos)
{
    float waypointRadius = 2.0f;
    //Close enough
    if (Vector3.Distance(playerPos, wpPos) < waypointRadius)
        return true;
    //Player moved "beyond" the wp towards the next
    Vector3 toNext = (nextWpPos - wpPos).normalized;
    Vector3 toPlayer = (playerPos - wpPos).normalized;
    return Vector3.Dot(toNext, toPlayer) > 0.3f; 
}
}
