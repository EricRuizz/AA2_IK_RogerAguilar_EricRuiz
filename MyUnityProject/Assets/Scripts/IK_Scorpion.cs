using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using Unity.Profiling;

public class IK_Scorpion : MonoBehaviour
{
    MyScorpionController _myController= new MyScorpionController();

    public IK_tentacles _myOctopus;

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
        if(animPlaying)
            animTime += Time.deltaTime;

        NotifyTailTarget();
        
        if (Input.GetKeyDown(KeyCode.Space))
        {
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
        }

        if (animTime < animDuration)
        {
            Body.position = Vector3.Lerp(StartPos.position, EndPos.position, animTime / animDuration);
        }
        else if (animTime >= animDuration && animPlaying)
        {
            Body.position = EndPos.position;
            animPlaying = false;
        }

        ////////
        UpdateFutureLegBases();
        UpdateBodyPosition();
        ////////

        _myController.UpdateIK();
    }
    
    //Function to send the tail target transform to the dll
    public void NotifyTailTarget()
    {
        _myController.NotifyTailTarget(tailTarget);
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

            if (Physics.Raycast(raycastFutureLegBases[i].position, Vector3.down, out hit, 10.0f))
            {
                futureLegBases[i].transform.position = hit.point;
                Debug.Log(hit.transform.name);
            }
        }
    }

    private void UpdateBodyPosition()
    {
        float newAverageHeight = ComputeLegAverageHeight();
        float heightDiff = newAverageHeight - averageLegHeight;

        Vector3 bPos = Body.transform.position;
        Body.transform.position = new Vector3(bPos.x, bodyInitHeight + heightDiff, bPos.z);
        
        for (int i = 0; i < futureLegBases.Length; i++)
        {
            Vector3 fPos = futureLegBases[i].transform.position;
            futureLegBases[i].transform.position = new Vector3(fPos.x, futureLegBases[i].transform.position.y - heightDiff, fPos.z);
        }
    }
}
