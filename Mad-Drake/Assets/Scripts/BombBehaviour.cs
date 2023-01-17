using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombBehaviour : MonoBehaviour
{

    [SerializeField]
    private int damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Shadow"))
        {
            collision.gameObject.GetComponent<ShadowBehaviour>().TakeDamage(damage);
        } 
        else if (collision.gameObject.CompareTag("Shadow Guard"))
        {
            collision.gameObject.GetComponent<EnemySkirmisherBehaviour>().TakeDamage(damage);
        }
    }


}
