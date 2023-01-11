using System;
using UnityEngine;
using UnityEngine.UI;

public class ZombieBehaviour : ShadowBehaviour
{
    private void Start()
    {
        speed = 0.5f;
        health = 7;
        rb2 = GetComponent<Rigidbody2D>();
        healthBar.maxValue = health;
    }
}
