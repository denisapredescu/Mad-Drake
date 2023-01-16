using System.Collections;
using UnityEngine;

public class UniversalSetTarget : MonoBehaviour
{
    [SerializeField]
    private GameObject target;
    [SerializeField]
    private float delayActivate = 1.0f;

    private void Start()
    {
        if (target != null)
            MatchTargetWithScripts();
    }

    private void MatchTargetWithScripts()
    {
        if(gameObject.CompareTag("Shadow"))
        {
            gameObject.GetComponent<ShadowBehaviour>().SetFollowingObject(target);
        } 
        else if(gameObject.CompareTag("Shadow Guard"))
        {
            gameObject.GetComponent<EnemySkirmisherBehaviour>().SetFollowingObject(target);
        }
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;
        MatchTargetWithScripts();
    }

    public IEnumerator WaitToSetActivity(bool active)
    {
        yield return new WaitForSeconds(delayActivate);

        if (gameObject.CompareTag("Shadow"))
        {
            gameObject.GetComponent<ShadowBehaviour>().SetFollowing(active);
        }
        else if (gameObject.CompareTag("Shadow Guard"))
        {
            gameObject.GetComponent<EnemySkirmisherBehaviour>().SetFollowing(active);
        }
    }

    public void SetEnemyActivity(bool active)
    {
        StartCoroutine(WaitToSetActivity(active));
    }
}
