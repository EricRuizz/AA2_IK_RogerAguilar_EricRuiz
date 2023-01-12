using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Purchasing.Security;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.UI;

public class MovingBall : MonoBehaviour
{
    public class MovementData
    {
        public Vector3 velocity;
        public Vector3 angularVel;
    }

    [SerializeField]
    IK_tentacles _myOctopus;
    [SerializeField]
    Transform _blueTarget;
    [SerializeField]
    SphereCollider _sphereCollider;

    //movement speed in units per second
    [Range(-1.0f, 1.0f)]
    [SerializeField]
    private float _movementSpeed = 5f;

    Vector3 _dir;

    //
    [HideInInspector] public bool ballShot = false;

    [SerializeField] Slider strength;
    [SerializeField] Slider effectStrength;

    [SerializeField] Text rotationVelocityText;

    [SerializeField] GameObject blueSpherePrefab;
    [SerializeField] GameObject greySpherePrefab;
    [SerializeField] Transform scorpionEndEffector;

    [Header("ARROWS")]
    [SerializeField] Transform greenArrow;
    [SerializeField] List<Transform> redArrows = new List<Transform>();
    [SerializeField] Transform greyArrow;

    private List<GameObject> blueTrajectory = new List<GameObject>();
    private List<GameObject> greyTrajectory = new List<GameObject>();

    Vector3 velocity;
    Vector3 acceleration;

    Vector3 gravity;

    Vector3 startingVelocity;
    Vector3 impactVect;
    Vector3 angularMomentum;
    Vector3 angularVelocity;

    float airDensity;
    float freeStream;

    int steps;
    bool showTrajectory;
    bool showInfo;
    //

    // Start is called before the first frame update
    void Start()
    {
        airDensity = 1.09f;
        freeStream = 1.0f;
        gravity = Vector3.down * 9.8f;

        steps = 20;
        showInfo = true;
        showTrajectory = true;
        ballShot = false;

        for (int i = 0; i < steps; i++)
        {
            greyTrajectory.Add(Instantiate(greySpherePrefab, transform.position, Quaternion.identity));
            blueTrajectory.Add(Instantiate(blueSpherePrefab, transform.position, Quaternion.identity));
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;

        //get the Input from Horizontal axis
        float horizontalInput = Input.GetAxis("Horizontal");
        //get the Input from Vertical axis
        float verticalInput = Input.GetAxis("Vertical");

        //update the position
        transform.position = transform.position + new Vector3(-horizontalInput * _movementSpeed * Time.deltaTime, verticalInput * _movementSpeed * Time.deltaTime, 0);
        
        if(showInfo && !ballShot)
        {
            float totalTime = 0.35f / strength.value;
            float stepTime = totalTime / steps;

            Vector3 contactPoint = (scorpionEndEffector.position - transform.position).normalized * transform.gameObject.GetComponent<SphereCollider>().radius;

            MovementData data = CalculateVariables(contactPoint, transform);
            Vector3 simVelocity = data.velocity;
            Vector3 simAngularVel = data.angularVel;
            Vector3 simMagnus =  CalculateMagnusForce(simAngularVel);

            redArrows[0].position = transform.position;
            redArrows[0].LookAt(gravity.normalized + transform.position);

            redArrows[1].position = transform.position;
            redArrows[1].LookAt(simMagnus.normalized + transform.position);
            //redArrows[1].rotation = Quaternion.LookRotation(simMagnus.normalized);

            greyArrow.position = transform.position;
            greyArrow.LookAt(simVelocity.normalized + transform.position);
            //greyArrow.rotation = Quaternion.LookRotation(simVelocity.normalized);

            greyArrow.gameObject.SetActive(true);
            redArrows[0].gameObject.SetActive(true);
            redArrows[1].gameObject.SetActive(true);
            greenArrow.gameObject.SetActive(false);
        }

        if (ballShot)
        {
            Vector3 magnusForce = CalculateMagnusForce(angularVelocity);

            transform.position += velocity * Time.deltaTime;
            velocity += acceleration * Time.deltaTime;
            acceleration = gravity + magnusForce;

            if(showInfo)
            {
                greenArrow.gameObject.SetActive(true);
                greenArrow.position = transform.position;
                greenArrow.LookAt(velocity.normalized + transform.position);
            }
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            showInfo = !showInfo;

            if(!showInfo)
            {
                for (int i = 0; i < steps; i++)
                {
                    greyTrajectory[i].SetActive(false);
                    blueTrajectory[i].SetActive(false);
                }

                greyArrow.gameObject.SetActive(false);
                redArrows[0].gameObject.SetActive(false);
                redArrows[1].gameObject.SetActive(false);
                greenArrow.gameObject.SetActive(false);
                StopCoroutine(ComputeTrajectory());
            }
            else
            {
                for (int i = 0; i < steps; i++)
                {
                    greyTrajectory[i].SetActive(true);
                    blueTrajectory[i].SetActive(true);
                }
            }
        }

        if(showInfo && showTrajectory && !ballShot)
        {
            showTrajectory = false;
            StartCoroutine(ComputeTrajectory());
        }
    }

    IEnumerator ComputeTrajectory()
    {
        float totalTime = 0.35f / strength.value;
        float stepTime = totalTime / steps;

        Vector3 simBlueVelocity = Vector3.zero;
        Vector3 simBlueAcceleration = Vector3.zero;
        Vector3 contactPoint = (scorpionEndEffector.position - transform.position).normalized * transform.gameObject.GetComponent<SphereCollider>().radius;
        Vector3 simBlueAngularVel = Vector3.zero;

        MovementData data = CalculateVariables(contactPoint, transform);
        simBlueVelocity = data.velocity;
        simBlueAngularVel = data.angularVel;

        Vector3 simGreyVelocity = simBlueVelocity;
        Vector3 simGreyAcceleration = Vector3.zero;

        Vector3 bluePos = transform.position;
        Vector3 greyPos = transform.position;

        for (int i = 0; i < steps; i++)
        {
            greyPos += simGreyVelocity * stepTime;
            simGreyVelocity += simGreyAcceleration * stepTime;
            simGreyAcceleration = gravity;

            bluePos += simBlueVelocity * stepTime;
            simBlueVelocity += simBlueAcceleration * stepTime;
            simBlueAcceleration = gravity + CalculateMagnusForce(simBlueAngularVel);

            greyTrajectory[i].transform.position = greyPos;
            blueTrajectory[i].transform.position = bluePos;

            yield return null;

        }
        yield return new WaitForSeconds(0.7f);

        showTrajectory = true;
    }

    private MovementData CalculateVariables(Vector3 contactPoint, Transform transform)
    {
        MovementData mData = new MovementData();
        float totalTime = 0.35f / strength.value;

        mData.velocity = (_blueTarget.position - transform.position - (gravity * Mathf.Pow(totalTime, 2)) / 2.0f) / totalTime;

        impactVect = (contactPoint - transform.position).normalized;
        angularMomentum = Vector3.Cross(impactVect, mData.velocity.normalized);
        mData.angularVel = angularMomentum * (effectStrength.value * -1);

        return mData;
    }

    private Vector3 CalculateMagnusForce(Vector3 angularVel)
    {
        return airDensity * freeStream * Mathf.Pow((2.0f * Mathf.PI * 0.25f), 2) * 1.0f * angularVel;
    }

    private void OnCollisionEnter(Collision collision)
    {
        _myOctopus.NotifyShoot();
        acceleration = Vector3.zero;
        ballShot = true;

        MovementData data = CalculateVariables(collision.contacts[0].point, transform);
        velocity = data.velocity;
        angularVelocity = data.angularVel;

        rotationVelocityText.text = "Rotation velocity: " + angularVelocity.ToString("F2");
    }
}

