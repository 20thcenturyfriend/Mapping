using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Waypoint : MonoBehaviour
{
    //Stores list of other waypoints to act as nodes in the navigation graph
    public List<Waypoint> neighbors = new List<Waypoint>();

    public Vector3 Position => transform.position;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.3f);
        Gizmos.color = Color.green;
        foreach (var neighbor in neighbors)
        {
            if (neighbor != null)
                Gizmos.DrawLine(transform.position, neighbor.Position);
        }
    }
}
