using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomController : MonoBehaviour
{
    [SerializeField]
    private GameObject doors;
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private Transform doorsTransform;
    private void Start()
    {
        doors.SetActive(false);
    }
    public void OpenDoors()
    {
        doors.SetActive(false);
    }

    public void CloseDoors()
    {
        doors.SetActive(true);
    }

    private void Update()
    {
        doorsTransform.position = new Vector3(cameraTransform.position.x, cameraTransform.position.y, 1);
    }
}
