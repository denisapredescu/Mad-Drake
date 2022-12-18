using UnityEngine;

public class RotateAnimation : MonoBehaviour
{
    [SerializeField]
    private float speed = 200.0f;
    private void Update()
    {
        transform.Rotate(0, 0, -speed * Time.deltaTime);
    }
}
