using System.Collections.Generic;
using UnityEngine;

public class PathVisualizer : MonoBehaviour
{
    public GameObject arrowPrefab;
    public float arrowSpacing = 1f;

    private List<GameObject> spawnedArrows = new List<GameObject>();

    public void DrawPath(List<Vector3> pathPoints)
    {
        Clear();
        if (arrowPrefab == null || pathPoints == null || pathPoints.Count < 2)
            return;

        float distanceBetweenArrows = arrowSpacing;
        float distanceTraveled = 0f;

        List<Vector3> sampledPoints = new List<Vector3>();

        //Walk along the path
        for (int i = 0; i < pathPoints.Count - 1; i++)
        {
            Vector3 start = pathPoints[i];
            Vector3 end = pathPoints[i + 1];
            float segmentLength = Vector3.Distance(start, end);
            Vector3 direction = (end - start).normalized;

            while (distanceTraveled + distanceBetweenArrows <= segmentLength)
            {
                start += direction * distanceBetweenArrows;
                sampledPoints.Add(start);
                distanceTraveled += distanceBetweenArrows;
            }

            //Reset for next segment
            distanceTraveled = 0f;
        }

        //Create arrows at sampled points
        for (int i = 0; i < sampledPoints.Count - 1; i++)
        {
            Vector3 current = sampledPoints[i];
            Vector3 next = sampledPoints[i + 1];
            Vector3 forward = (next - current).normalized;

            float angleY = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(90f, angleY, 0f);

            GameObject arrow = Instantiate(arrowPrefab, current, rotation, transform);
            spawnedArrows.Add(arrow);
        }
    }

    public void Clear()
    {
        foreach (var arrow in spawnedArrows)
        {
            if (arrow != null)
                Destroy(arrow);
        }
        spawnedArrows.Clear();
    }
}
