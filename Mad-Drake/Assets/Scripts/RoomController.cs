using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public List<GameObject> doors;
    public Transform cameraTransform;
    public Transform doorsTransform;
    private void Start()
    {
        for(int i = 0; i < doors.Count; i++)
        {
            doors[i].SetActive(true);
        }
    }
    public void OpenDoors()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].SetActive(false);
        }
    }

    public void CloseDoors()
    {
        for (int i = 0; i < doors.Count; i++)
        {
            doors[i].SetActive(true);
        }
    }

    public void DoorsFollowCamera()
    {
        doorsTransform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, 1);
    }
}
