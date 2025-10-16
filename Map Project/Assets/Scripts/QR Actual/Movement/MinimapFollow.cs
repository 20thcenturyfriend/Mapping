using UnityEngine;
public class MinimapFollow : MonoBehaviour
{
    public Transform player;
    void LateUpdate()
    {
        Vector3 newPosition = player.position;
        newPosition.y = transform.position.y; //Keep height constant
        transform.position = newPosition;
    }
}
