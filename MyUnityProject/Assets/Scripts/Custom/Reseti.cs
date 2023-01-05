using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reseti : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private Transform scorpion;

    private Vector3 ballPos;
    private Vector3 scorpionPos;

    private void Awake()
    {
        ballPos = ball.position;
        scorpionPos = scorpion.position;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            ball.GetComponent<MovingBall>().ballShot = false;

            ball.position = ballPos;
            scorpion.position = scorpionPos;
        }
    }
}
