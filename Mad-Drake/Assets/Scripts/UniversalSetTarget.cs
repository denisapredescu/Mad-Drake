using UnityEngine;

public class UniversalSetTarget : MonoBehaviour
{
    [SerializeField]
    private GameObject target;

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
    }

    public void SetTarget(GameObject target)
    {
        this.target = target;
        MatchTargetWithScripts();
    }

    public void SetEnemyActivity(bool active)
    {
        if(gameObject.CompareTag("Shadow"))
        {
            gameObject.GetComponent<ShadowBehaviour>().SetFollowing(active);
        }
    }
}
