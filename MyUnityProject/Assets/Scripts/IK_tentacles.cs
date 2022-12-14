using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;



public class IK_tentacles : MonoBehaviour
{

    [SerializeField]
    Transform[] _tentacles = new Transform[4];

    [SerializeField]
    Transform[] _randomTargets;

    [SerializeField]
    TargetFollower _targetFollower;
    Transform _notifyTarget;


    MyOctopusController _myController = new MyOctopusController();
    


    [Header("Exercise 3")]
    [SerializeField, Range(0, 360)]
    float _twistMin ;

    [SerializeField, Range(0, 360)]
    float _twistMax;

    [SerializeField, Range(0, 360)]
    float _swingMin;

    [SerializeField, Range(0, 360)]
    float _swingMax;

    [SerializeField]
    bool _updateTwistSwingLimits = false;




    [SerializeField]
    float TwistMin{set{ _myController.TwistMin = value; }}



    #region public methods


    public void NotifyTarget(Transform target, Transform region)
    {
        _notifyTarget = target;
        _myController.NotifyTarget(_targetFollower.transform, target);
    }

    public void NotifyShoot()
    {
        _myController.NotifyShoot();
        _targetFollower.StartFollowingTarget(_notifyTarget);
    }


    #endregion


    // Start is called before the first frame update
    void Start()
    {
        
        _myController.TestLogging(gameObject.name);
        //TODO Tentacles with lerp
        _myController.Init(_tentacles, _randomTargets);

        _myController.TwistMax = _twistMax;
        _myController.TwistMin = _twistMin;
        _myController.SwingMax = _swingMax;
        _myController.SwingMin = _swingMin;

    }



    // Update is called once per frame
    void Update()
    {
        _myController.UpdateTentacles();

        if (_updateTwistSwingLimits) {
            _myController.TwistMax = _twistMax;
            _myController.TwistMin = _twistMin;
            _myController.SwingMax = _swingMax;
            _myController.SwingMin = _swingMin;
            _updateTwistSwingLimits = false;
        }

    }
}
