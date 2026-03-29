using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject ProjectilePrefab;
    public float VelocityMult = 10f;
    public GameObject projLinePrefab;

    [Header("Dynamic")]
    public GameObject LaunchPoint;
    public Vector3 LaunchPos;
    public GameObject Projectile;
    public bool aimingMode;

    void Awake()
    {
        Transform LaunchPointTrans = transform.Find("LaunchPoint");
        LaunchPoint = LaunchPointTrans.gameObject;
        LaunchPoint.SetActive(false);
        LaunchPos = LaunchPointTrans.position;
    }

    void OnMouseEnter()
    {
        LaunchPoint.SetActive(true);
    }

    void OnMouseExit()
    {
        LaunchPoint.SetActive(false);
    }

    void OnMouseDown()
    {
        aimingMode = true;
        Projectile = Instantiate(ProjectilePrefab) as GameObject;
        Projectile.transform.position = LaunchPos;
        Projectile.GetComponent<Rigidbody>().isKinematic = true;
    }

    void Update()
    {
        if (!aimingMode || Projectile == null) 
        {
            aimingMode = false;
            return;
        }

        Vector3 mousePos2D = Input.mousePosition;
        mousePos2D.z = -Camera.main.transform.position.z;
        Vector3 mousePos3D = Camera.main.ScreenToWorldPoint(mousePos2D);

        Vector3 mouseDelta = mousePos3D - LaunchPos;

        float maxMagnitude = this.GetComponent<SphereCollider>().radius;
        if (mouseDelta.magnitude > maxMagnitude)
        {
            mouseDelta.Normalize();
            mouseDelta *= maxMagnitude;
        }

        Vector3 projPos = LaunchPos + mouseDelta;
        Projectile.transform.position = projPos;

        if (Input.GetMouseButtonUp(0))
        {
            aimingMode = false;
            Rigidbody projRB = Projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * VelocityMult;
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);

            FollowCam.POI = Projectile;
            Instantiate<GameObject>(projLinePrefab, Projectile.transform);
            Projectile = null;
            MissionDemolition.SHOT_FIRED();
        }
    }
}