using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController2 : MonoBehaviour
{
    public Transform target;
    public float speed = 4f;
    Rigidbody rig;

    void Start()
    {
        rig = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        Vector2 pos = Vector2.MoveTowards(transform.position, target.position, speed * Time.fixedDeltaTime);
        transform.LookAt(target);
    }
}
