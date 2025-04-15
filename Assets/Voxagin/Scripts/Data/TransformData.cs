namespace Ngin {
    using UnityEngine;
    using System.Collections.Generic;
    [System.Serializable]
    public class TransformData : nData {
        public TransformData() {
            _position = Vector3.zero;
            _rotation = Quaternion.identity;
            _scale = Vector3.one;
        }
        public TransformData(Transform transform) {
            _transform = transform;
            _position = transform.position;
            _rotation = transform.rotation;
            _scale = transform.localScale;
        }
        public TransformData(Vector3 position, Quaternion rotation, Vector3 scale) {
            _position = position;
            _rotation = rotation;
            _scale = scale;
        }
        public TransformData(TransformData data) {
            _position = data._position;
            _rotation = data._rotation;
            _scale = data._scale;
        }
        public override void LoadFromLexicon(Lexicon lexicon) {
            Debug.Log("Loading TransformData:");
            Debug.Log("Position: " + lexicon.GetVector3("position", Vector3.zero));
            var rotationList = lexicon.Get<List<float>>("rotation", new List<float>());
            string rotationString = string.Join(", ", rotationList);
            Debug.Log("Rotation: " + lexicon.GetQuaternion("rotation", Quaternion.identity).eulerAngles + " vs: " + rotationString);
            Debug.Log("Scale: " + lexicon.GetVector3("scale", Vector3.one));
            _position = lexicon.GetVector3("position", Vector3.zero);
            _rotation = lexicon.GetQuaternion("rotation", Quaternion.identity);
            _scale = lexicon.GetVector3("scale", Vector3.one);
        }
        public void Link(Transform transform) {
            _transform = transform;
        }
        public void Apply() {
            if (_transform == null) {
                return;
            }
            _transform.position = _position;
            _transform.rotation = _rotation;
            _transform.localScale = _scale;
        }
        public TransformData Lerp(TransformData target, float t) {
            return new TransformData(Vector3.Lerp(_position, target._position, t),
                                     Quaternion.Lerp(_rotation, target._rotation, t),
                                     Vector3.Lerp(_scale, target._scale, t));
        }

        protected Vector3 _position;
        public Vector3 Position {
            get { return _position; }
            set { _position = value; }
        }
        public Vector3 position {
            get {
                return _position;
            }
            set {
                _position = value;
            }
        }
        protected Quaternion _rotation;
        public Quaternion Rotation {
            get { return _rotation; }
            set { _rotation = value; }
        }
        public Quaternion rotation {
            get {
                return _rotation;
            }
            set {
                _rotation = value;
            }
        }
        protected Vector3 _scale;
        public Vector3 Scale {
            get { return _scale; }
            set { _scale = value; }
        }
        public Vector3 scale {
            get {
                return _scale;
            }
            set {
                _scale = value;
            }
        }

        public Vector3 up {
            get {
                return _rotation * Vector3.up;
            } 
            set {
                _rotation = Quaternion.FromToRotation(Vector3.up, value);
            }
        }
        public Vector3 right {
            get {
                return _rotation * Vector3.right;
            } 
            set {
                _rotation = Quaternion.FromToRotation(Vector3.right, value);
            }
        }

        protected Transform _transform;
    }
}