using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopItemArea : MonoBehaviour
{
    public bool isInArea;
    // Start is called before the first frame update
    void Start()
    {
        isInArea = false;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.tag == "IgnoreBullet") {
            isInArea = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.tag == "IgnoreBullet") {
            isInArea = false;
        }
    }
}
