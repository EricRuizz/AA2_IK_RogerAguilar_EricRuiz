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

    [SerializeField] Transform blueTrajectory;
    [SerializeField] Transform greyTrajectory;
    [SerializeField] Transform scorpionEndEffector;

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
        ballShot = false;

        blueTrajectory.transform.position = transform.position;
        greyTrajectory.transform.position = transform.position;
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
        
        if (ballShot)
        {
            Vector3 magnusForce = CalculateMagnusForce(angularVelocity);

            transform.position += velocity * Time.deltaTime;
            velocity += acceleration * Time.deltaTime;
            acceleration = gravity + magnusForce;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            showInfo = !showInfo;

            if(showInfo)
            {
                blueTrajectory.transform.position = transform.position;
                greyTrajectory.transform.position = transform.position;

                blueTrajectory.gameObject.SetActive(true);
                greyTrajectory.gameObject.SetActive(true);
            }
            else
            {
                blueTrajectory.gameObject.SetActive(false);
                greyTrajectory.gameObject.SetActive(false);
            }
        }

        if(showInfo && !ballShot)
        {
            showInfo = false;
            StartCoroutine(ComputeTrajectory());
        }
    }

    IEnumerator ComputeTrajectory()
    {
        blueTrajectory.gameObject.SetActive(true);
        greyTrajectory.gameObject.SetActive(true);

        float totalTime = 0.35f / strength.value;
        float stepTime = totalTime / steps;

        Vector3 simBlueVelocity = Vector3.zero;
        Vector3 simBlueAcceleration = Vector3.zero;
        Vector3 contactPoint = (scorpionEndEffector.position - transform.position).normalized * transform.gameObject.GetComponent<SphereCollider>().radius;
        Vector3 simBlueAngularVel = Vector3.zero;

        MovementData data = CalculateVariables(contactPoint, greyTrajectory);
        simBlueVelocity = data.velocity;
        simBlueAngularVel = data.angularVel;

        Debug.Log(" AAA       " + data.velocity);
        Debug.Log(" VVV       " + simBlueVelocity);

        Vector3 simGreyVelocity = simBlueVelocity;
        Vector3 simGreyAcceleration = Vector3.zero;

        for (int i = 0; i < steps; i++)
        {
            greyTrajectory.position += simGreyVelocity * stepTime;
            simGreyVelocity += simGreyAcceleration * stepTime;
            simGreyAcceleration = gravity;

            blueTrajectory.position += simBlueVelocity * stepTime;
            simBlueVelocity += simBlueAcceleration * stepTime;
            simBlueAcceleration = gravity + CalculateMagnusForce(simBlueAngularVel);
            yield return null;

        }

        yield return new WaitForSeconds(0.7f);

        blueTrajectory.gameObject.SetActive(false);
        greyTrajectory.gameObject.SetActive(false);
        blueTrajectory.transform.position = transform.position;
        greyTrajectory.transform.position = transform.position;

        showInfo = true;
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

