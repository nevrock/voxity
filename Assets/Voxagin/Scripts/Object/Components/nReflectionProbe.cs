using UnityEngine;
using UnityEngine.Rendering;

namespace Ngin {
    public class nReflectionProbe : nComponent {
        public ReflectionProbe reflectionProbe;
        public float _updateInterval = 5.0f; // Update interval in seconds
        private float timer = 0.0f;

        public string _mode;
        public string _refreshMode;
        public string _timeSlicingMode;

        public float _intensity;
        public Vector3 _size;
        public Vector3 _center;
        public bool _boxProjection;
        public float _nearClipPlane;
        public float _farClipPlane;
        public int _resolution;
        public bool _hdr;

        protected override void AddClasses() {
            reflectionProbe = ComponentCheck<ReflectionProbe>(true);
        }
        protected override void StoreData(Lexicon data) {
            _updateInterval = data.Get<float>("_updateInterval", 0.5f);

            _mode = data.Get<string>("mode", ReflectionProbeMode.Realtime.ToString());
            _refreshMode = data.Get<string>("refreshMode", ReflectionProbeRefreshMode.ViaScripting.ToString());
            _timeSlicingMode = data.Get<string>("timeSlicingMode", ReflectionProbeTimeSlicingMode.AllFacesAtOnce.ToString());
            Debug.Log("Time Slicing Mode: " + _timeSlicingMode);
            _intensity = data.Get<float>("intensity", 1.0f);
            _size = data.Get<Vector3>("size", new Vector3(40, 30, 40));
            _center = data.Get<Vector3>("center", Vector3.zero);
            _boxProjection = data.Get<bool>("boxProjection", false);
            _nearClipPlane = data.Get<float>("nearClipPlane", 0.1f);
            _farClipPlane = data.Get<float>("farClipPlane", 1000f);
            _resolution = data.Get<int>("resolution", 64);
            _hdr = data.Get<bool>("hdr", false);
        }

        protected override void Setup() {
            // Additional setup if needed
            if (reflectionProbe == null) {
                Debug.Log("Failed to add ReflectionProbe component.");
                return;
            }

            reflectionProbe.mode = (ReflectionProbeMode)System.Enum.Parse(typeof(ReflectionProbeMode), _mode);
            reflectionProbe.refreshMode = (ReflectionProbeRefreshMode)System.Enum.Parse(typeof(ReflectionProbeRefreshMode), _refreshMode);
            reflectionProbe.timeSlicingMode = (ReflectionProbeTimeSlicingMode)System.Enum.Parse(typeof(ReflectionProbeTimeSlicingMode), _timeSlicingMode);
            
            reflectionProbe.intensity = _intensity;
            reflectionProbe.size = _size;
            reflectionProbe.center = _center;
            reflectionProbe.boxProjection = _boxProjection;
            reflectionProbe.nearClipPlane = _nearClipPlane;
            reflectionProbe.farClipPlane = _farClipPlane;
            reflectionProbe.resolution = _resolution;
            reflectionProbe.hdr = _hdr;

            //reflectionProbe.RenderProbe();
        }

        protected override void Tick() {
            base.Tick();
            timer += Time.deltaTime;
            if (timer >= _updateInterval) {
                UpdateReflectionProbe();
                timer = 0.0f;
            }
        }

        public void UpdateReflectionProbe() {
            //if (reflectionProbe.mode == ReflectionProbeMode.Realtime && reflectionProbe.refreshMode == ReflectionProbeRefreshMode.ViaScripting) {
               // reflectionProbe.RenderProbe();
            //}
        }

        // Additional methods if needed
    }
}
