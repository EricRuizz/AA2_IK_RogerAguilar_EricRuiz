using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetFollower : MonoBehaviour
{
    private Transform target;
    private float speed;
    private bool followTarget;

    void Start()
    {
        speed = 20.0f;
        followTarget = false;
    }

    public void StartFollowingTarget(Transform newTarget)
    {
        target = newTarget;
        followTarget = true;
    }

    void Update()
    {
        if(followTarget)
        {
            transform.position += (target.position - transform.position).normalized * speed * Time.deltaTime;

            if ((target.position - transform.position).magnitude <= 0.5f)
            {
                followTarget = false;
            }
        }
    }
}
