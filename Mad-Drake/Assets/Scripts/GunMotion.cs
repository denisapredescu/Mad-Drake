using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations;

public class GunMotion : MonoBehaviour
{
    private new Transform transform;
    private new Camera camera;
    [SerializeField]
    private Transform parentTransform;
    [SerializeField]
    private Transform tipTransform;

    private void Start()
    {
        transform = this.gameObject.transform;
        camera = Camera.main;
    }

    private void Update()
    {
        Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        
        //using tipTransform instead of transform actually gives the right point

        float distTip = (
            new Vector2(transform.position.x, transform.position.y) - 
            new Vector2(tipTransform.position.x, tipTransform.position.y)).magnitude;
        float distMouse = (
            new Vector2(transform.position.x, transform.position.y) -
            new Vector2(mousePos.x, mousePos.y)).magnitude;

        //check if the distance from center to tip is bigger than the distance from center to point
        //that would cause the gun to glitch out because the difference vector would be in a completely other direction
        Vector2 difference;
        if(distTip + 1.0f >= distMouse)
        {
            difference = mousePos - transform.position;
        } 
        else 
        {
            difference = mousePos - tipTransform.position;
        }

        //possible yet unlikely error that the mouse hits exactly the center
        if(difference.magnitude == 0.0f)
        {
            difference = Vector2.right;
        }
        else
        {
            difference.Normalize();
        }
        
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        
        //rotate gun
        if(parentTransform.localScale.x > 0.0f)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
        } else
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ + 180.0f);
        }

        //rotate player left and right
        if((parentTransform.position.x > mousePos.x && parentTransform.localScale.x > 0.0f) || 
            (parentTransform.position.x < mousePos.x && parentTransform.localScale.x < 0.0f))
        {
            //this is added to smooth the transition
            if(Mathf.Abs(parentTransform.position.x - mousePos.x) > 0.1f)
            {
                parentTransform.localScale = new Vector3(
                    -parentTransform.localScale.x,
                    parentTransform.localScale.y,
                    parentTransform.localScale.z);
            }
        }
    }
}
