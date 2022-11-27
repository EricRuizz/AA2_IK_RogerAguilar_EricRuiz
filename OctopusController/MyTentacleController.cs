using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{


    internal class MyTentacleController

    //MAINTAIN THIS CLASS AS INTERNAL
    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        Transform _endEffectorSphere;

        public Transform[] Bones { get => _bones; }



        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            tentacleMode = mode;
            Transform transform = root;

            List<Transform> _tempBones = new List<Transform>();
            switch (tentacleMode)
            {
                case TentacleMode.LEG:
                    {
                        transform = transform.GetChild(0);
                        _tempBones.Add(transform);

                        while (transform.childCount != 0)
                        {
                            transform = transform.GetChild(1);
                            _tempBones.Add(transform);
                        }
                        _bones = _tempBones.ToArray();
                        _endEffectorSphere = transform;
                    }
                    break;
                case TentacleMode.TAIL:
                    {
                        _tempBones.Add(transform);
                        while (transform.childCount != 0)
                        {
                            transform = transform.GetChild(1);
                            _tempBones.Add(transform);
                        }
                        _bones = _tempBones.ToArray();
                        _endEffectorSphere = transform;
                    }
                    break;
                case TentacleMode.TENTACLE:
                    {
                        transform = transform.GetChild(0);
                        while (transform.childCount != 0)
                        {
                            transform = transform.GetChild(0);
                            _tempBones.Add(transform);
                        }
                        _bones = _tempBones.ToArray();
                        _endEffectorSphere = transform;
                    }
                    break;
            }

            return Bones;
        }
    }
}