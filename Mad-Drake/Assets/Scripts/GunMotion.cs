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

    private void Start()
    {
        transform = this.gameObject.transform;
        camera = Camera.main;
    }

    private void Update()
    {
        Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
        Vector3 difference = mousePos - transform.position;
        difference.Normalize();
        float rotationZ = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        
        //rotate gun
        if(parentTransform.localScale.x > 0.0f)
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ);
        } else
        {
            transform.rotation = Quaternion.Euler(0.0f, 0.0f, rotationZ + 180);
        }

        //rotate player left and right
        if((parentTransform.position.x > mousePos.x && parentTransform.localScale.x > 0.0f) || 
            (parentTransform.position.x < mousePos.x && parentTransform.localScale.x < 0.0f))
        {
            parentTransform.localScale = new Vector3(
                -parentTransform.localScale.x,
                parentTransform.localScale.y,
                parentTransform.localScale.z);
        }
    }
}
