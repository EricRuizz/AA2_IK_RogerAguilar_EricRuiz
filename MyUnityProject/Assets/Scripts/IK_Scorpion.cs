using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using Unity.Profiling;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class IK_Scorpion : MonoBehaviour
{
    MyScorpionController _myController= new MyScorpionController();

    public IK_tentacles _myOctopus;

    //////
    public ScorpionMovement scorpionMovement;

    [SerializeField] public GameObject magnusEffectSlider;
    [SerializeField] public GameObject strengthSlider;
    //////

    [Header("Body")]
    float animTime;
    public float animDuration = 5;
    bool animPlaying = false;
    public Transform Body;
    public Transform StartPos;
    public Transform EndPos;

    [Header("Tail")]
    public Transform tailTarget;
    public Transform tail;

    [Header("Legs")]
    public Transform[] legs;
    public Transform[] legTargets;
    public Transform[] futureLegBases;
    public Transform[] raycastFutureLegBases;

    ////////
    float bodyInitHeight;

    float averageLegHeight;
    ////////

    // Start is called before the first frame update
    void Start()
    {
        _myController.InitLegs(legs,futureLegBases,legTargets);
        _myController.InitTail(tail);

        ////////
        averageLegHeight = ComputeLegAverageHeight();
        SetInitPositions();
        ////////
    }

    // Update is called once per frame
    void Update()
    {
        //if (animPlaying)
        //    animTime += Time.deltaTime;

        NotifyTailTarget();

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    NotifyStartWalk();
        //    animTime = 0;
        //    animPlaying = true;
        //}

        //if (animTime < animDuration)
        //{
        //    Body.position = Vector3.Lerp(StartPos.position, EndPos.position, animTime / animDuration);
        //}
        //else if (animTime >= animDuration && animPlaying)
        //{
        //    Body.position = EndPos.position;
        //    animPlaying = false;
        //}

        NotifyStartWalk();
        animTime = 0;
        animPlaying = true;

        ////////
        if(scorpionMovement.moved)
        {
            UpdateFutureLegBases();
            UpdateBodyPosition();
            UpdateBodyRotation();

            scorpionMovement.moved = false;
        }
        ////////

        _myController.UpdateIK();
    }


    //Function to send the tail target transform to the dll
    public void NotifyTailTarget()
    {
        _myController.NotifyTailTarget(tailTarget);
        _myController.UpdateSliderValues(magnusEffectSlider.GetComponent<UnityEngine.UI.Slider>().value, strengthSlider.GetComponent<UnityEngine.UI.Slider>().value);
    }

    //Trigger Function to start the walk animation
    public void NotifyStartWalk()
    {

        _myController.NotifyStartWalk();
    }

    private float ComputeLegAverageHeight()
    {
        float averageHeight = 0.0f;
        for (int i = 0; i < futureLegBases.Length; i++)
        {
            averageHeight += futureLegBases[i].position.y;
        }

        return (averageHeight /= futureLegBases.Length);
    }

    private void SetInitPositions()
    {
        bodyInitHeight = Body.transform.position.y;
    }

    private void UpdateFutureLegBases()
    {
        for(int i = 0; i < futureLegBases.Length; i++)
        {
            RaycastHit hit;

            if (Physics.Raycast(raycastFutureLegBases[i].position, Vector3.down, out hit, 2.0f))
            {
                futureLegBases[i].transform.position = hit.point;
            }
        }
    }

    private void UpdateBodyPosition()
    {
        float newAverageHeight = ComputeLegAverageHeight();
        float heightDiff = newAverageHeight - averageLegHeight;

        Vector3 bPos = Body.transform.position;
        Body.transform.position = new Vector3(bPos.x, bodyInitHeight + heightDiff, bPos.z);
    }

    private void UpdateBodyRotation()
    {
        //get the planes
        Vector3 n1 = CalculateNormal(futureLegBases[2], futureLegBases[1], futureLegBases[0]);
        Vector3 n2 = CalculateNormal(futureLegBases[3], futureLegBases[4], futureLegBases[5]);

        Vector3 newNormal = (n1 + n2) / 2.0f;

        Body.transform.up = newNormal;
    }

    private Vector3 CalculateNormal(Transform t0, Transform t1, Transform t2)
    {
        return Vector3.Cross((t1.position - t0.position), (t2.position - t0.position));
    }

    private void OnDrawGizmos()
    {
        for (int i = 0; i < futureLegBases.Length; i++)
        {
            Gizmos.DrawSphere(futureLegBases[i].transform.position, 0.1f);
        }
    }
}
