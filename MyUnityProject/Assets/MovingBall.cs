using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing.Security;
using UnityEngine.UI;

public class MovingBall : MonoBehaviour
{
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

    Vector3 velocity;
    Vector3 acceleration;

    Vector3 startingVelocity;
    Vector3 impactVect;
    Vector3 angularMomentum;
    Vector3 angularVelocity;

    float airDensity;
    //

    // Start is called before the first frame update
    void Start()
    {
        airDensity = 0.1f;
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
            //var direction = Vector3.Cross(angularVelocity, velocity);
            //var magnitude = 4.0f / 3f * Mathf.PI * airDensity * Mathf.Pow(_sphereCollider.radius, 3);
            //rb.AddForce(magnitude * direction);

            acceleration = velocity * 0.9f;
            velocity = acceleration * Time.deltaTime * 250.0f;
            transform.position += velocity * Time.deltaTime;
            //acceleration = Vector3.down * (2f) + (magnitude * direction);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        _myOctopus.NotifyShoot();
        ballShot = true;

        startingVelocity = (_blueTarget.position - transform.position).normalized;
        velocity = startingVelocity * strength.value;
        impactVect = (collision.contacts[0].point - transform.position).normalized;
        angularMomentum = Vector3.Cross(impactVect, startingVelocity);

        angularVelocity = angularMomentum * (effectStrength.value * -1);

        rotationVelocityText.text = "Rotation velocity: " + angularVelocity.ToString("F2");
    }
}
