using System.Collections.Generic;
using UnityEngine;

public class WaypointPathfinder : MonoBehaviour
{
    ///The A* pathfinding implementation. Basically, from the Wp Selector script (finds best wp), this script finds the best path from that* wp to the goal
    public static List<Vector3> FindPath(Waypoint start, Waypoint goal)
    {
        var openSet = new PriorityQueue<Waypoint>();
        var cameFrom = new Dictionary<Waypoint, Waypoint>();
        var gScore = new Dictionary<Waypoint, float>();
        openSet.Enqueue(start, 0);
        gScore[start] = 0;

        while (openSet.Count > 0)
        {
            Waypoint current = openSet.Dequeue();

            if (current == goal)
                return ReconstructPath(cameFrom, current);

            foreach (var neighbor in current.neighbors)
            {
                float tentativeScore = gScore[current] + Vector3.Distance(current.Position, neighbor.Position);

                if (!gScore.ContainsKey(neighbor) || tentativeScore < gScore[neighbor])
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeScore;

                    float priority = tentativeScore + Vector3.Distance(neighbor.Position, goal.Position);
                    openSet.Enqueue(neighbor, priority);
                }
            }
        }
        return null; //No path found
    }
    private static List<Vector3> ReconstructPath(Dictionary<Waypoint, Waypoint> cameFrom, Waypoint current)
    {
        List<Vector3> path = new List<Vector3> { current.Position };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current.Position);
        }
        return path;
    }
}