using UnityEngine;

namespace Ngin
{
    public class nIK : nComponent
    {
        Transform _target;
        Transform _pole;

        public int _iterations = 10;
        public float _delta = 0.001f;
        public int _chainLength = 2;
        public float _snapBackStrength = 1f;
        public string _targetName;
        public string _poleName;
        public string TargetName { get { return _targetName; } }
        public string PoleName { get { return _poleName; } }
        public bool _copyRotationFromTarget = false;
        public float _copyRotationStrength = 1f;

        protected float[] _bonesLengths; //_target to Origin
        protected float _completeLength;
        protected Transform[] _bones;
        protected Vector3[] _positions;
        protected Vector3[] _startDirections;
        protected Quaternion[] _startRotations;
        protected Quaternion _startRotationTarget;
        protected Transform _root;

        bool _isLinked = false;

        protected override void StoreData(Lexicon data) {
            _iterations = data.Get<int>("iterations", 10);
            _delta = data.Get<float>("delta", 0.001f);
            _chainLength = data.Get<int>("chainLength", 2);
            _snapBackStrength = data.Get<float>("snapBackStrength", 1f);

            _targetName = data.Get<string>("targetName", "");
            _poleName = data.Get<string>("poleName", "");
            _copyRotationFromTarget = data.Get<bool>("copyRotationFromTarget", false);
            _copyRotationStrength = data.Get<float>("copyRotationStrength", 1f);
        }
        protected override void Setup() {
        }

        public void Link(nIKController controller, 
                        Transform effectorTarget, 
                        Transform effectorPole) {

            Log.Console("Linking IK: " + this.name);

            _target = effectorTarget;
            _pole = effectorPole;
            _isLinked = true;

            Init();
        }
        void Init()
        {
            //initial array
            _bones = new Transform[_chainLength + 1];
            _positions = new Vector3[_chainLength + 1];
            _bonesLengths = new float[_chainLength];
            _startDirections = new Vector3[_chainLength + 1];
            _startRotations = new Quaternion[_chainLength + 1];

            //find root
            _root = transform;
            for (var i = 0; i <= _chainLength; i++)
            {
                if (_root == null)
                    throw new UnityException("The chain value is longer than the ancestor chain!");
                _root = _root.parent;
            }

            //init target
            if (_target == null)
            {
                _target = new GameObject(gameObject.name + " _target").transform;
                SetPositionRootSpace(_target, GetPositionRootSpace(transform));
            }
            _startRotationTarget = GetRotationRootSpace(_target);


            //init data
            var current = transform;
            _completeLength = 0;
            for (var i = _bones.Length - 1; i >= 0; i--)
            {
                _bones[i] = current;
                _startRotations[i] = GetRotationRootSpace(current);

                if (i == _bones.Length - 1)
                {
                    //leaf
                    _startDirections[i] = GetPositionRootSpace(_target) - GetPositionRootSpace(current);
                }
                else
                {
                    //mid bone
                    _startDirections[i] = GetPositionRootSpace(_bones[i + 1]) - GetPositionRootSpace(current);
                    _bonesLengths[i] = _startDirections[i].magnitude;
                    _completeLength += _bonesLengths[i];
                }

                current = current.parent;
            }
        }

        // Update is called once per frame
        protected override void TickLate()
        {
            ResolveIK();
        }
        private void ResolveIK()
        {
            if (_target == null)
                return;

            if (_bonesLengths.Length != _chainLength)
                Init();

            //Fabric

            //  root
            //  (bone0) (bonelen 0) (bone1) (bonelen 1) (bone2)...
            //   x--------------------x--------------------x---...

            //get position
            for (int i = 0; i < _bones.Length; i++)
                _positions[i] = GetPositionRootSpace(_bones[i]);

            var targetPosition = GetPositionRootSpace(_target);
            var targetRotation = GetRotationRootSpace(_target);

            //1st is possible to reach?
            if ((targetPosition - GetPositionRootSpace(_bones[0])).sqrMagnitude >= _completeLength * _completeLength)
            {
                //just strech it
                var direction = (targetPosition - _positions[0]).normalized;
                //set everything after root
                for (int i = 1; i < _positions.Length; i++)
                    _positions[i] = _positions[i - 1] + direction * _bonesLengths[i - 1];
            }
            else
            {
                for (int i = 0; i < _positions.Length - 1; i++)
                    _positions[i + 1] = Vector3.Lerp(_positions[i + 1], _positions[i] + _startDirections[i], _snapBackStrength);

                for (int iteration = 0; iteration < _iterations; iteration++)
                {
                    //https://www.youtube.com/watch?v=UNoX65PRehA
                    //back
                    for (int i = _positions.Length - 1; i > 0; i--)
                    {
                        if (i == _positions.Length - 1)
                            _positions[i] = targetPosition; //set it to target
                        else
                            _positions[i] = _positions[i + 1] + (_positions[i] - _positions[i + 1]).normalized * _bonesLengths[i]; //set in line on distance
                    }

                    //forward
                    for (int i = 1; i < _positions.Length; i++)
                        _positions[i] = _positions[i - 1] + (_positions[i] - _positions[i - 1]).normalized * _bonesLengths[i - 1];

                    //close enough?
                    if ((_positions[_positions.Length - 1] - targetPosition).sqrMagnitude < _delta * _delta)
                        break;
                }
            }

            //move towards pole
            if (_pole != null)
            {
                var polePosition = GetPositionRootSpace(_pole);
                for (int i = 1; i < _positions.Length - 1; i++)
                {
                    var plane = new Plane(_positions[i + 1] - _positions[i - 1], _positions[i - 1]);
                    var projectedPole = plane.ClosestPointOnPlane(polePosition);
                    var projectedBone = plane.ClosestPointOnPlane(_positions[i]);
                    var angle = Vector3.SignedAngle(projectedBone - _positions[i - 1], projectedPole - _positions[i - 1], plane.normal);
                    _positions[i] = Quaternion.AngleAxis(angle, plane.normal) * (_positions[i] - _positions[i - 1]) + _positions[i - 1];
                }
            }

            //set position & rotation
            for (int i = 0; i < _positions.Length; i++)
            {
                if (i == _positions.Length - 2 && _copyRotationFromTarget)
                {
                    SetRotationRootSpace(_bones[i], Quaternion.FromToRotation(_startDirections[i], _positions[i + 1] - _positions[i]) * Quaternion.Inverse(_startRotations[i]));

                    _bones[i].rotation = Quaternion.Lerp(_bones[i].rotation, _target.rotation, _copyRotationStrength);
                }
                else if (i == _positions.Length - 1)
                {
                    if (_copyRotationFromTarget)
                    {
                        //_bones[i].rotation = _target.rotation;
                        _bones[i].localRotation = Quaternion.identity;
                    }
                    else
                    {
                        SetRotationRootSpace(_bones[i], Quaternion.Inverse(targetRotation) * _startRotationTarget * Quaternion.Inverse(_startRotations[i]));
                    }
                }
                else
                {
                    SetRotationRootSpace(_bones[i], Quaternion.FromToRotation(_startDirections[i], _positions[i + 1] - _positions[i]) * Quaternion.Inverse(_startRotations[i]));
                }
                SetPositionRootSpace(_bones[i], _positions[i]);
            }
        }
        private Vector3 GetPositionRootSpace(Transform current)
        {
            if (_root == null)
                return current.position;
            else
                return Quaternion.Inverse(_root.rotation) * (current.position - _root.position);
        }
        private void SetPositionRootSpace(Transform current, Vector3 position)
        {
            if (_root == null)
                current.position = position;
            else
                current.position = _root.rotation * position + _root.position;
        }
        private Quaternion GetRotationRootSpace(Transform current)
        {
            //inverse(after) * before => rot: before -> after
            if (_root == null)
                return current.rotation;
            else
                return Quaternion.Inverse(current.rotation) * _root.rotation;
        }
        private void SetRotationRootSpace(Transform current, Quaternion rotation)
        {
            if (_root == null)
                current.rotation = rotation;
            else
                current.rotation = _root.rotation * rotation;
        }

    }
}