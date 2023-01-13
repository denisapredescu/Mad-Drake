using MyUtility;
using Unity.VisualScripting;
using UnityEngine;

public class EnemySkirmisherBehaviour : MonoBehaviour
{
    [SerializeField]
    private Transform target;
    private bool follow = true;
    [SerializeField]
    private float minimumDistanceToFire = 5.0f;
    [SerializeField]
    private float speed = 2.0f;
    private Rigidbody2D rb2;

    private void Start()
    {
        rb2 = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if(follow)
        {
            if (target.position.x - 0.1f >= transform.position.x)
            {
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, 1.0f);
            }

            if (target.position.x + 0.1f <= transform.position.x)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, 1.0f);
            }
        }
    }

    private void FixedUpdate()
    {
        if(follow)
        {
            if(FloatMath.Larger((target.position - transform.position).magnitude, minimumDistanceToFire))
                rb2.MovePosition(
                    transform.position +
                    speed * Time.fixedDeltaTime * (target.position - transform.position).normalized);
        }
    }

    public void SetFollowingObject(GameObject toFollow)
    {
        target = toFollow.transform;
    }
    public Transform Target()
    {
        return target;
    }
    public void SetFollowing(bool value)
    {
        follow = value;
    }
    public bool IsFollowing()
    {
        return follow;
    }
}
