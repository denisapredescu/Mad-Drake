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
    [SerializeField]
    private PlayerController playerController;

    private Vector2 difference;
    private Vector3 mousePos;
    private float distTip;
    private float distMouse;

    float gunRotationZ;

    private void Start()
    {
        transform = this.gameObject.transform;
        camera = Camera.main;
    }

    private void Update()
    {
        mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        
        CalculateDifferenceVector();
        RotateGun();
        
    }

    void CalculateDifferenceVector()
    {
        //using tipTransform instead of transform actually gives the right point

        distTip = (
            new Vector2(transform.position.x, transform.position.y) - 
            new Vector2(tipTransform.position.x, tipTransform.position.y)).magnitude;
        distMouse = (
            new Vector2(transform.position.x, transform.position.y) -
            new Vector2(mousePos.x, mousePos.y)).magnitude;

        //check if the distance from center to tip is bigger than the distance from center to point
        //that would cause the gun to glitch out because the difference vector would be in a completely other direction
        
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
    }

    void RotateGun()
    {
        gunRotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;

        if (MenuController.GameRunning)
        {
            if (parentTransform.localScale.x > 0.0f)
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, gunRotationZ);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0.0f, 0.0f, gunRotationZ + 180.0f);
            }
        }
    }

    
}
