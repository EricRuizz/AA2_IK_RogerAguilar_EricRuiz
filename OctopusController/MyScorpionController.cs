using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
  
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;
        //////
        float[] tailSolutions;
        float stopThreshold;

        float learningRate;

        Vector3[] axis;
        Vector3[] startOffsets;
        //////

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        
        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            for(int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation
            }

        }

        public void InitTail(Transform TailBase)
        {
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);

            //TODO: Initialize anything needed for the Gradient Descent implementation
            axis = new Vector3[_tail.Bones.Length];
            stopThreshold = 0.25f;
            learningRate = 0.1f;
            startOffsets = new Vector3[_tail.Bones.Length];
            tailSolutions = new float[_tail.Bones.Length];

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                if (i == 0)
                {
                    axis[i] = Vector3.forward;
                    tailSolutions[i] = _tail.Bones[i].localRotation.eulerAngles.y;
                }
                else
                {
                    axis[i] = Vector3.right;
                    tailSolutions[i] = _tail.Bones[i].localRotation.eulerAngles.x;
                }

                //startOffsets[i] = _tail.Bones[i].localPosition;

                Debug.Log(tailSolutions[i]);
            }

            for (int i = 0; i < _tail.Bones.Length - 1; i++)
            {
                startOffsets[i] = Quaternion.Inverse(_tail.Bones[i].rotation) * (_tail.Bones[i + 1].position - _tail.Bones[i].position);
            }

            for (int i = 0; i < _tail.Bones.Length; i++)
            {
                Debug.Log(startOffsets[i]);
            }
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {
            
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            if (DistanceFromTarget(tailTarget.position, tailSolutions) < 5.0f)
            {
                updateTail();
            }
        }
        #endregion


        #region private
        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            //
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {

        }

        private void updateTail()
        {
            if (DistanceFromTarget(tailTarget.position, tailSolutions) > stopThreshold)
            {
                ApproachTarget(tailTarget.position);
            }
        }

        private void ApproachTarget(Vector3 target)
        {
            for (int i = 0; i < tailSolutions.Length; i++)
            {
                float gradient = CalculateGradient(target, tailSolutions, i, learningRate);
                //tailSolutions[i] = tailSolutions[i] - gradient;
                tailSolutions[i] -= 10.0f * gradient;
                _tail.Bones[i].localRotation = Quaternion.Euler(tailSolutions[i] * axis[i]);
                //_tail.Bones[i].Rotate(axis[i] * tailSolutions[i]);

                if (DistanceFromTarget(target, tailSolutions) < 0.25f)
                {
                    return;
                }
            }
        }

        private float CalculateGradient(Vector3 target, float[] solutions, int i, float delta)
        {
            solutions[i] += delta;
            float distance1 = DistanceFromTarget(target, solutions);
            solutions[i] -= delta;

            float distance2 = DistanceFromTarget(target, solutions);

            return (distance1 - distance2) / delta;
        }

        private float DistanceFromTarget(Vector3 target, float[] solutions)
        {
            Vector3 point = ForwardKinematics(solutions);
            return Vector3.Distance(point, target);
        }

        private Vector3 ForwardKinematics(float[] solutions)
        {
            Vector3 prevPoint = _tail.Bones[0].position;
            Quaternion rotation = _tail.Bones[0].rotation;

            for (int i = 1; i < _tail.Bones.Length; i++)
            {
                rotation *= Quaternion.AngleAxis(solutions[i - 1], axis[i]);
                Vector3 nextPoint = prevPoint + rotation * startOffsets[i];
                Debug.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }

            return prevPoint;
        }

        #endregion
    }
}
