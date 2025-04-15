namespace Ngin {
    using UnityEngine;
    [System.Serializable]
    public class RectData : nData {
        public RectData() {
            _position = Vector2.zero;
            _size = Vector2.one;
            _anchorMin = Vector2.zero;
            _anchorMax = Vector2.one;
            _pivot = new Vector2(0.5f, 0.5f);
        }
        public RectData(RectTransform rectTransform) {
            _rectTransform = rectTransform;
            _position = rectTransform.anchoredPosition;
            _size = rectTransform.sizeDelta;
            _anchorMin = rectTransform.anchorMin;
            _anchorMax = rectTransform.anchorMax;
            _pivot = rectTransform.pivot;
        }
        public RectData(Vector2 position, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot) {
            _position = position;
            _size = size;
            _anchorMin = anchorMin;
            _anchorMax = anchorMax;
            _pivot = pivot;
        }
        public RectData(RectData data) {
            _position = data._position;
            _size = data._size;
            _anchorMin = data._anchorMin;
            _anchorMax = data._anchorMax;
            _pivot = data._pivot;
        }
        public override void LoadFromLexicon(Lexicon lexicon) {
            _position = lexicon.Get<Vector2>("position", Vector2.zero);
            _size = lexicon.Get<Vector2>("size", Vector2.one);
            _anchorMin = lexicon.Get<Vector2>("anchorMin", Vector2.zero);
            _anchorMax = lexicon.Get<Vector2>("anchorMax", Vector2.one);
            _pivot = lexicon.Get<Vector2>("pivot", new Vector2(0.5f, 0.5f));
        }
        public void Link(RectTransform rectTransform) {
            _rectTransform = rectTransform;
        }
        public void Apply() {
            if (_rectTransform == null) {
                return;
            }
            _rectTransform.anchoredPosition = _position;
            _rectTransform.sizeDelta = _size;
            _rectTransform.anchorMin = _anchorMin;
            _rectTransform.anchorMax = _anchorMax;
            _rectTransform.pivot = _pivot;
        }
        public RectData Lerp(RectData target, float t) {
            return new RectData(Vector2.Lerp(_position, target._position, t),
                                Vector2.Lerp(_size, target._size, t),
                                Vector2.Lerp(_anchorMin, target._anchorMin, t),
                                Vector2.Lerp(_anchorMax, target._anchorMax, t),
                                Vector2.Lerp(_pivot, target._pivot, t));
        }

        protected Vector2 _position;
        public Vector2 Position {
            get { return _position; }
            set { _position = value; }
        }
        protected Vector2 _size;
        public Vector2 Size {
            get { return _size; }
            set { _size = value; }
        }
        protected Vector2 _anchorMin;
        public Vector2 AnchorMin {
            get { return _anchorMin; }
            set { _anchorMin = value; }
        }
        protected Vector2 _anchorMax;
        public Vector2 AnchorMax {
            get { return _anchorMax; }
            set { _anchorMax = value; }
        }
        protected Vector2 _pivot;
        public Vector2 Pivot {
            get { return _pivot; }
            set { _pivot = value; }
        }

        protected RectTransform _rectTransform;
    }
}
