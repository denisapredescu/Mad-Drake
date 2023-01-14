using UnityEngine;

public class Engine : MonoBehaviour
{
    void Start()
    {
        Physics2D.IgnoreLayerCollision(8, 9);
    }
}
