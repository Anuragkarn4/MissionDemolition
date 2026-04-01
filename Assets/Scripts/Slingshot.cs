using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slingshot : MonoBehaviour
{
    [Header("Inscribed")]
    public GameObject ProjectilePrefab;
    public float VelocityMult = 10f;
    public GameObject projLinePrefab;
    public LineRenderer leftRubberBand;
    public LineRenderer rightRubberBand;
    public AudioSource snapSound; 

    [Header("Trajectory Prediction")]
    public LineRenderer trajectoryLine;
    public int trajectoryPoints = 30;
    public float trajectoryTime = 3f; 

    [Header("Dynamic")]
    public GameObject LaunchPoint;
    public Vector3 LaunchPos;
    public GameObject Projectile;
    public bool aimingMode;

    void Start()
    {
        leftRubberBand.enabled = false;  
        rightRubberBand.enabled = false;
        trajectoryLine.enabled = false;
    }

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
        leftRubberBand.enabled = true;
        rightRubberBand.enabled = true;
    }


    void UpdateTrajectory(Vector3 velocity)
    {
        trajectoryLine.enabled = true;
        trajectoryLine.positionCount = trajectoryPoints;
        
        Vector3[] points = new Vector3[trajectoryPoints];
        Vector3 lastPos = Projectile.transform.position;
        
        for (int i = 0; i < trajectoryPoints; i++)
        {
            Vector3 dir = velocity.normalized;
            RaycastHit hit;
            
            // Predict next point with gravity
            float time = i * Time.fixedDeltaTime * 2f;
            Vector3 nextPos = Projectile.transform.position + velocity * time 
                            + 0.5f * Physics.gravity * time * time;
            
            // Raycast between points to detect collisions
            if (Physics.Raycast(lastPos, nextPos - lastPos, out hit, Vector3.Distance(lastPos, nextPos)))
            {
                points[i] = hit.point;
                trajectoryLine.positionCount = i + 1;  // Stop at collision
                break;
            }
            else
            {
                points[i] = nextPos;
            }
            lastPos = nextPos;
        }
        
        trajectoryLine.SetPositions(points);
    }

    void HideTrajectory()
    {
        trajectoryLine.enabled = false;
    }

    void Update()
    {
        if (!aimingMode || Projectile == null) 
        {
            leftRubberBand.enabled = false;
            rightRubberBand.enabled = false;
            HideTrajectory();
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

        // Predict trajectory
        Vector3 predictedVelocity = -mouseDelta * VelocityMult;
        UpdateTrajectory(predictedVelocity);

        leftRubberBand.SetPosition(0, transform.Find("LeftArm/LeftTip").position);
        leftRubberBand.SetPosition(1, Projectile.transform.position);

        rightRubberBand.SetPosition(0, transform.Find("RightArm/RightTip").position);
        rightRubberBand.SetPosition(1, Projectile.transform.position);

        if (Input.GetMouseButtonUp(0))
        {
            aimingMode = false;
            snapSound.Play();
            HideTrajectory();
            Rigidbody projRB = Projectile.GetComponent<Rigidbody>();
            projRB.isKinematic = false;
            projRB.collisionDetectionMode = CollisionDetectionMode.Continuous;
            projRB.velocity = -mouseDelta * VelocityMult;
            FollowCam.SWITCH_VIEW(FollowCam.eView.slingshot);

            FollowCam.POI = Projectile;
            Instantiate<GameObject>(projLinePrefab, Projectile.transform);

            leftRubberBand.enabled = false;
            rightRubberBand.enabled = false;
            Projectile = null;
            MissionDemolition.SHOT_FIRED();
        }
    }
}