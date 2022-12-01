using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor.Experimental.UIElements;
using UnityEngine;
using UnityEngine.Rendering;

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
        //////
        List<List<float>> distance = new List<List<float>>();
        List<List<Vector3>> tempJoints = new List<List<Vector3>>();

        bool walk = false;
        float maxLegDistance;

        bool[] moveLeg;
        float[] legLerpTParam;
        Vector3[] lerpInitPos;
        Vector3[] lerpFinalPos;
        //////


        #region public
        public void InitLegs(Transform[] LegRoots, Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            legTargets = LegTargets;
            legFutureBases = LegFutureBases;

            maxLegDistance = 1.0f;

            moveLeg = new bool[LegRoots.Length];
            legLerpTParam = new float[LegRoots.Length];
            lerpInitPos = new Vector3[LegRoots.Length];
            lerpFinalPos = new Vector3[LegRoots.Length];

            //Legs init
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);
                //TODO: initialize anything needed for the FABRIK implementation

                List<float> distancesToAdd = new List<float>();
                List<Vector3> tempJointsToAdd = new List<Vector3>();

                moveLeg[i] = false;
                legLerpTParam[i] = 0.0f;

                for (int j = 0; j < _legs[i].Bones.Length; j++)
                {
                    if (j + 1 < _legs[i].Bones.Length)
                    {
                        distancesToAdd.Add(Vector3.Distance(_legs[i].Bones[j].position, _legs[i].Bones[j + 1].position));
                        //distance[i].Add(Vector3.Distance(_legs[i].Bones[j].position, _legs[i].Bones[j + 1].position));
                    }

                    tempJointsToAdd.Add(_legs[i].Bones[j].position);
                    //tempJoints[i].Add(_legs[i].Bones[j].position);
                }

                distance.Add(distancesToAdd);
                tempJoints.Add(tempJointsToAdd);
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
                    axis[i] = Vector3.up;
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
            walk = true;
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            if (DistanceFromTarget(tailTarget.position, tailSolutions) < 5.0f)
            {
                updateTail();
            }

            if (walk)
            {
                updateLegPos();
            }

            LerpLegs();
        }
        #endregion


        #region private

        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            updateLegs();

            for (int i = 0; i < _legs.Length; i++)
            {
                if (Vector3.Distance(_legs[i].Bones[0].position, legFutureBases[i].position) > maxLegDistance && !moveLeg[i])
                {
                    MoveLegBase(i);
                }
            }
        }

        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {
            for (int legI = 0; legI < _legs.Length; legI++)
            {

                bool done = false;
                for (int j = 0; j < _legs[legI].Bones.Length; j++)
                {
                    tempJoints[legI][j] = _legs[legI].Bones[j].position;
                }

                if (!done)
                {
                    if (Vector3.Distance(tempJoints[legI][0], legTargets[legI].position) > distance[legI].Sum())
                    {
                        for (int jointI = 1; jointI < tempJoints[legI].Count - 1; jointI++)
                        {
                            float lambda = distance[legI][jointI] / Vector3.Magnitude(legTargets[legI].position - tempJoints[legI][jointI]);

                            tempJoints[legI][jointI] = (1 - lambda) * tempJoints[legI][jointI] + lambda * legTargets[legI].position;
                        }

                        done = true;
                    }
                    else
                    {
                        while (Vector3.Distance(tempJoints[legI][tempJoints[legI].Count - 1], legTargets[legI].position) > 0.1f)
                        {
                            //forward reaching
                            tempJoints[legI][tempJoints[legI].Count - 1] = legTargets[legI].position;

                            for (int jointI = tempJoints[legI].Count - 2; jointI >= 0; jointI--)
                            {
                                float lambda = distance[legI][jointI] / Vector3.Magnitude(tempJoints[legI][jointI + 1] - tempJoints[legI][jointI]);

                                tempJoints[legI][jointI] = (1 - lambda) * tempJoints[legI][jointI + 1] + lambda * tempJoints[legI][jointI];
                            }

                            //backward reaching
                            tempJoints[legI][0] = _legs[legI].Bones[0].position;

                            for (int jointI = 1; jointI < tempJoints[legI].Count - 1; jointI++)
                            {
                                float lambda = distance[legI][jointI - 1] / Vector3.Magnitude(tempJoints[legI][jointI - 1] - tempJoints[legI][jointI]);

                                tempJoints[legI][jointI] = (1 - lambda) * tempJoints[legI][jointI - 1] + lambda * tempJoints[legI][jointI];
                            }
                        }

                        done = true;
                    }

                    for (int j = 0; j <= _legs[legI].Bones.Length - 2; j++)
                    {
                        Vector3 crossProd = Vector3.Cross(Vector3.Normalize(_legs[legI].Bones[j + 1].position - _legs[legI].Bones[j].position), Vector3.Normalize(tempJoints[legI][j + 1] - tempJoints[legI][j]));
                        float dotProd = Vector3.Dot(Vector3.Normalize(_legs[legI].Bones[j + 1].position - _legs[legI].Bones[j].position), Vector3.Normalize(tempJoints[legI][j + 1] - tempJoints[legI][j]));

                        _legs[legI].Bones[j].Rotate(crossProd, Mathf.Acos(dotProd) * Mathf.Rad2Deg, Space.World);
                    }
                }
            }
        }

        private void MoveLegBase(int i)
        {
            moveLeg[i] = true;

            legLerpTParam[i] = 0.0f;
            lerpInitPos[i] = _legs[i].Bones[0].position;
            lerpFinalPos[i] = legFutureBases[i].position;
        }

        void LerpLegs()
        {
            for (int i = 0; i < _legs.Length; i++)
            {
                if (moveLeg[i])
                {
                    if (legLerpTParam[i] >= 1.0f)
                    {
                        moveLeg[i] = false;
                    }
                    else
                    {
                        _legs[i].Bones[0].position = lerpInitPos[i] + ((lerpFinalPos[i] - lerpInitPos[i]) * (legLerpTParam[i]));
                        legLerpTParam[i] = legLerpTParam[i] + (Time.deltaTime * 10.0f);
                        Debug.Log(legLerpTParam[i]);
                    }
                }
            }
        }


        //TODO: implement Gradient Descent method to move tail if necessary
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
            Quaternion rotation = _tail.Bones[0].parent.rotation;

            for (int i = 1; i < _tail.Bones.Length; i++)
            {
                rotation *= Quaternion.AngleAxis(solutions[i - 1], axis[i - 1]);
                Vector3 nextPoint = prevPoint + rotation * startOffsets[i-1];
                Debug.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }

            return prevPoint;
        }

        #endregion
    }
}
