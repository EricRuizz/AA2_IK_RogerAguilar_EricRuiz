using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using UnityEditor.Animations;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController
    {
        //////
        bool[] done;
        Vector3[] lastTargetPos;

        int[] tries;
        int maxTries;

        bool ballShot = false;
        int closestTentacle = 0;
        //////

        MyTentacleController[] _tentacles = new MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];


        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin { set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }


        public void TestLogging(string objectName)
        {
            Debug.Log("hello, I am initializing my Octopus Controller in object " + objectName);
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            done = new bool[4];
            lastTargetPos = new Vector3[4];
            tries = new int[4];
            maxTries = 10;

            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for (int i = 0; i < tentacleRoots.Length; i++)
            {

                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i], TentacleMode.TENTACLE);

                //TODO: initialize any variables needed in ccd
                done[i] = false;
                lastTargetPos[i] = randomTargets[i].position;
            }

            _randomTargets = randomTargets;
            //TODO: use the regions however you need to make sure each tentacle stays in its region
        }


        public void NotifyTarget(Transform target, Transform region) //region as targets final position
        {
            _currentRegion = region;
            _target = target;

            if (ballShot)
            {
                float lastDistance = 0.0f;
                for (int i = 0; i < _tentacles.Length; i++)
                {
                    if ((region.position - _tentacles[i].Bones[_tentacles[i].Bones.Length - 1].position).magnitude < lastDistance)
                    {
                        closestTentacle = i;
                    }
                }
            }
        }

        public void NotifyShoot()
        {
            ballShot = true;
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
        }




        #endregion


        #region private and internal methods
        //todo: add here anything that you need

        void update_ccd()
        {
            for (int i = 0; i < _tentacles.Length; i++) //tentacle roots
            {
                Vector3 targetPos;

                if (ballShot && i == closestTentacle)
                {
                    targetPos = _target.position;
                }
                else
                {
                    targetPos = _randomTargets[i].position;
                }

                if (!done[i])
                {
                    if (tries[i] < maxTries)
                    {
                        for (int j = 0; j < _tentacles[i].Bones.Length; j++)
                        {
                            double theta;
                            float sin;

                            int length = _tentacles[i].Bones.Length;

                            Vector3 r1 = _tentacles[i].Bones[length - 1].position - _tentacles[i].Bones[j].position;

                            Vector3 r2 = targetPos - _tentacles[i].Bones[j].position;

                            float dot = 0.0f;
                            Vector3 cross = Vector3.zero;

                            if (r1.magnitude * r2.magnitude <= 0.001f)
                            {
                                sin = 0;
                            }
                            else
                            {
                                dot = Vector3.Dot(r1, r2);
                                cross = Vector3.Cross(r1.normalized, r2.normalized);
                                sin = cross.magnitude;
                            }

                            Vector3 axis = cross.normalized;

                            theta = Math.Acos(dot / (r1.magnitude * r2.magnitude));

                            theta = theta % Math.PI;
                            theta *= sin > 0 ? 1 : -1;

                            if(theta > 0.025f)
                            {
                                //_tentacles[i].Bones[j].rotation *= Quaternion.AngleAxis((float)theta * Mathf.Rad2Deg, axis);
                                _tentacles[i].Bones[j].Rotate(axis, (float)theta * Mathf.Rad2Deg, Space.World);
                            }

                            _tentacles[i].Bones[j].localRotation = GetSwing(_tentacles[i].Bones[j].localRotation);
                        }

                        tries[i]++;
                    }
                }

                Vector3 difference = targetPos - _tentacles[i].Bones[_tentacles[i].Bones.Length - 1].position;

                if (difference.magnitude <= 0.1f)
                {
                    done[i] = true;
                }
                else
                {
                    done[i] = false;
                }

                if (targetPos != lastTargetPos[i])
                {
                    tries[i] = 0;
                    lastTargetPos[i] = targetPos;
                }
            }
        }


        public Quaternion GetTwist(Quaternion rot)
        {
            //todo: change the return value for exercise 3
            return GetSwing(rot) * CalculateTwist(rot);
        }

        private Quaternion CalculateTwist(Quaternion rot)
        {
            Quaternion qt;
            qt.x = 0;
            qt.y = rot.y;
            qt.z = 0;
            qt.w = rot.w;
            return qt.normalized;
        }
        public Quaternion GetSwing(Quaternion rot)
        {
            //todo: change the return value for exercise 3
            //return totalRotation * (rot * Quaternion.Inverse(CalculateTwist(rot)));

            rot = new Quaternion(rot.x, Mathf.Clamp(rot.y, 0.0f, 1.0f), Mathf.Clamp(rot.z, 0.0f, 10.0f), rot.w);

            Quaternion invertedRotation = new Quaternion(0f, rot.y, 0f, rot.w).normalized; // twist is in the Y axis

            invertedRotation = Quaternion.Inverse(invertedRotation);


            Quaternion qSwing = rot * invertedRotation;

            float angle;
            Vector3 axis;
            qSwing.ToAngleAxis(out angle, out axis);

            return Quaternion.AngleAxis(Mathf.Clamp(angle, 0.0f, _swingMax), axis);
        }


        #endregion
    }
}