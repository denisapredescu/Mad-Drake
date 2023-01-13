using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGun : MonoBehaviour
{
    private Transform target;
    [SerializeField]
    private Transform parentTransform;
    [SerializeField]
    private Transform tipTransform;
    [SerializeField]
    private EnemySkirmisherBehaviour enemyBehaviour;

    private Vector2 difference;
    private float distTip;
    private float distTarget;

    private void Update()
    {
        target = enemyBehaviour.Target();

        if(target != null && enemyBehaviour.IsFollowing())
        {
            CalculateDifferenceVector();
            RotateGun();
        }
    }

    void CalculateDifferenceVector()
    {
        //using tipTransform instead of transform actually gives the right point

        distTip = (
            new Vector2(transform.position.x, transform.position.y) -
            new Vector2(tipTransform.position.x, tipTransform.position.y)).magnitude;
        distTarget = (
            new Vector2(transform.position.x, transform.position.y) -
            new Vector2(target.position.x, target.position.y)).magnitude;

        //check if the distance from center to tip is bigger than the distance from center to point
        //that would cause the gun to glitch out because the difference vector would be in a completely other direction

        if (distTip + 1.0f >= distTarget)
        {
            difference = target.position - transform.position;
        }
        else
        {
            difference = target.position - tipTransform.position;
        }

        //possible yet unlikely error that the mouse hits exactly the center
        if (difference.magnitude == 0.0f)
        {
            difference = Vector2.right;
        }
        else
        {
            difference.Normalize();
        }
    }

    void RotateGun()
    {
        float gunRotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        if (parentTransform.localScale.x <= 0.0f)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, gunRotationZ);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, gunRotationZ + 180.0f);
        }
    }
}
