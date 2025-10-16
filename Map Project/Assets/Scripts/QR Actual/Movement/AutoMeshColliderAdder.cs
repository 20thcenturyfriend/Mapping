#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
public class AutoMeshColliderAdder : MonoBehaviour
{
    [MenuItem("Tools/Add Mesh Colliders to All Children")]
    public static void AddColliders()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogWarning("Please select the root object of your map.");
            return;
        }

        int count = 0;
        Transform root = Selection.activeGameObject.transform;
        foreach (MeshFilter meshFilter in root.GetComponentsInChildren<MeshFilter>())
        {
            GameObject go = meshFilter.gameObject;
            if (!go.GetComponent<Collider>())
            {
                go.AddComponent<MeshCollider>();
                count++;
            }
        }

        Debug.Log($"Added MeshCollider to {count} objects under {Selection.activeGameObject.name}.");
    }
}
#endif
