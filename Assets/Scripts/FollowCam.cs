using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCam : MonoBehaviour
{
    static public GameObject POI; // The static point of interest

    [Header("Inscribed")]
    public float easing = 0.05f; // The easing for the camera to follow the PO
    public Vector2 minXY = Vector2.zero; // The minimum x and y values of the camera's position

    [Header("Dynamic")]
    public float camZ; // The desired Z pos of the camera
    void Awake()
    {
        camZ = this.transform.position.z;
    }

    void FixedUpdate()
    {
        Vector3 destination = Vector3.zero;
        if (POI != null)
        {
            Rigidbody poiRigid = POI.GetComponent<Rigidbody>();
            if((poiRigid != null) && poiRigid.IsSleeping())
            {
                POI = null;
            }
        }

        if (POI != null)
        {
            destination = POI.transform.position;
        }
        destination.x = Mathf.Max(minXY.x, destination.x);
        destination.y = Mathf.Max(minXY.y, destination.y);
        destination = Vector3.Lerp(transform.position, destination, easing);
        destination.z = camZ;
        transform.position = destination;
        Camera.main.orthographicSize = destination.y + 10;
    }    
}