using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Unity.Mathematics;

namespace Ngin
{
    public class nIKJobs : nComponent
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

        protected float[] _bonesLengths;
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
            // No setup required for now
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
            _bones = new Transform[_chainLength + 1];
            _positions = new Vector3[_chainLength + 1];
            _bonesLengths = new float[_chainLength];
            _startDirections = new Vector3[_chainLength + 1];
            _startRotations = new Quaternion[_chainLength + 1];

            _root = transform;
            for (var i = 0; i <= _chainLength; i++)
            {
                if (_root == null)
                    throw new UnityException("The chain value is longer than the ancestor chain!");
                _root = _root.parent;
            }

            if (_target == null)
            {
                _target = new GameObject(gameObject.name + " _target").transform;
                SetPositionRootSpace(_target, GetPositionRootSpace(transform));
            }
            _startRotationTarget = GetRotationRootSpace(_target);

            var current = transform;
            _completeLength = 0;
            for (var i = _bones.Length - 1; i >= 0; i--)
            {
                _bones[i] = current;
                _startRotations[i] = GetRotationRootSpace(current);

                if (i == _bones.Length - 1)
                {
                    _startDirections[i] = GetPositionRootSpace(_target) - GetPositionRootSpace(current);
                }
                else
                {
                    _startDirections[i] = GetPositionRootSpace(_bones[i + 1]) - GetPositionRootSpace(current);
                    _bonesLengths[i] = _startDirections[i].magnitude;
                    _completeLength += _bonesLengths[i];
                }

                current = current.parent;
            }
        }

        protected override void TickLate()
        {
            if (_target == null)
                return;

            if (_bonesLengths.Length != _chainLength)
                Init();

            for (int i = 0; i < _bones.Length; i++)
                _positions[i] = GetPositionRootSpace(_bones[i]);

            NativeArray<float3> positions = new NativeArray<float3>(_positions.Length, Allocator.TempJob);
            NativeArray<float> bonesLengths = new NativeArray<float>(_bonesLengths.Length, Allocator.TempJob);
            NativeArray<float3> startDirections = new NativeArray<float3>(_startDirections.Length, Allocator.TempJob);

            for (int i = 0; i < _positions.Length; i++)
                positions[i] = _positions[i];

            for (int i = 0; i < _bonesLengths.Length; i++)
                bonesLengths[i] = _bonesLengths[i];

            for (int i = 0; i < _startDirections.Length; i++)
                startDirections[i] = _startDirections[i];

            IKJob ikJob = new IKJob
            {
                positions = positions,
                bonesLengths = bonesLengths,
                startDirections = startDirections,
                targetPosition = GetPositionRootSpace(_target),
                polePosition = _pole != null ? GetPositionRootSpace(_pole) : float3.zero,
                iterations = _iterations,
                delta = _delta,
                snapBackStrength = _snapBackStrength,
                hasPole = _pole != null
            };

            JobHandle jobHandle = ikJob.Schedule();
            jobHandle.Complete();

            for (int i = 0; i < _positions.Length; i++)
                _positions[i] = positions[i];

            positions.Dispose();
            bonesLengths.Dispose();
            startDirections.Dispose();

            ApplyPositions();
        }

        private void ApplyPositions()
        {
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
                        _bones[i].localRotation = Quaternion.identity;
                    }
                    else
                    {
                        SetRotationRootSpace(_bones[i], Quaternion.Inverse(GetRotationRootSpace(_target)) * _startRotationTarget * Quaternion.Inverse(_startRotations[i]));
                    }
                }
                else
                {
                    SetRotationRootSpace(_bones[i], Quaternion.FromToRotation(_startDirections[i], _positions[i + 1] - _positions[i]) * Quaternion.Inverse(_startRotations[i]));
                }
                SetPositionRootSpace(_bones[i], _positions[i]);
            }
        }

        [BurstCompile]
        struct IKJob : IJob
        {
            public NativeArray<float3> positions;
            public NativeArray<float> bonesLengths;
            public NativeArray<float3> startDirections;
            public float3 targetPosition;
            public float3 polePosition;
            public int iterations;
            public float delta;
            public float snapBackStrength;
            public bool hasPole;

            public void Execute()
            {
                NativeArray<float3> pos = new NativeArray<float3>(positions.Length, Allocator.Temp);
                for (int i = 0; i < positions.Length; i++)
                    pos[i] = positions[i];

                float totalLength = 0;
                for (int i = 0; i < bonesLengths.Length; i++)
                    totalLength += bonesLengths[i];

                if (math.lengthsq(targetPosition - pos[0]) >= math.pow(totalLength, 2))
                {
                    float3 direction = math.normalize(targetPosition - pos[0]);
                    for (int i = 1; i < pos.Length; i++)
                        pos[i] = pos[i - 1] + direction * bonesLengths[i - 1];
                }
                else
                {
                    for (int i = 0; i < pos.Length - 1; i++)
                        pos[i + 1] = math.lerp(pos[i + 1], pos[i] + startDirections[i], snapBackStrength);

                    for (int iteration = 0; iteration < iterations; iteration++)
                    {
                        for (int i = pos.Length - 1; i > 0; i--)
                        {
                            if (i == pos.Length - 1)
                                pos[i] = targetPosition;
                            else
                                pos[i] = pos[i + 1] + math.normalize(pos[i] - pos[i + 1]) * bonesLengths[i];
                        }

                        for (int i = 1; i < pos.Length; i++)
                            pos[i] = pos[i - 1] + math.normalize(pos[i] - pos[i - 1]) * bonesLengths[i - 1];

                        if (math.lengthsq(pos[pos.Length - 1] - targetPosition) < delta * delta)
                            break;
                    }
                }

                if (hasPole)
                {
                    for (int i = 1; i < pos.Length - 1; i++)
                    {
                        float3 planeNormal = math.cross(pos[i + 1] - pos[i - 1], pos[i] - pos[i - 1]);
                        float3 projectedPole = polePosition - math.dot(polePosition - pos[i - 1], planeNormal) * planeNormal;
                        float3 projectedBone = pos[i] - math.dot(pos[i] - pos[i - 1], planeNormal) * planeNormal;
                        float angle = math.degrees(math.acos(math.dot(math.normalize(projectedBone - pos[i - 1]), math.normalize(projectedPole - pos[i - 1]))));
                        pos[i] = math.mul(quaternion.AxisAngle(planeNormal, angle), pos[i] - pos[i - 1]) + pos[i - 1];
                    }
                }

                for (int i = 0; i < positions.Length; i++)
                    positions[i] = pos[i];

                pos.Dispose();
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
