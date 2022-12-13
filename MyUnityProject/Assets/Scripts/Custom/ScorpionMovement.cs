using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ScorpionMovement : MonoBehaviour
{
    [SerializeField] private Transform body;
    [SerializeField] private float speed;

    [HideInInspector] public bool moved;

    void Update()
    {
        Vector3 moveVector = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            moveVector += Vector3.back;
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveVector += Vector3.forward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            moveVector += Vector3.right;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveVector += Vector3.left;
        }

        if(moveVector != Vector3.zero)
        {
            body.position += moveVector.normalized * speed * Time.deltaTime;
            moved = true;
        }
    }
}
