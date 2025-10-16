using UnityEngine;
public class PlayerIconFollow : MonoBehaviour
{
    public Transform player;
    void Update()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, -player.eulerAngles.y);
    }
}
