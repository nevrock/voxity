using UnityEngine;

namespace Ngin {
    public class nCamera : nComponent {
        public Camera camera;
        protected override void AddClasses() {
            camera = ComponentCheck<Camera>(true);
        }
        protected override void StoreData(Lexicon data) {
            isOrthographic = data.Get<bool>("isOrthographic", false);
            focalLength = data.Get<float>("focalLength", 50); // Changed from fieldOfView to focalLength
            orthographicSize = data.Get<float>("orthographicSize", 5);
            nearClipPlane = data.Get<float>("near", 0.3f);
            farClipPlane = data.Get<float>("far", 1000);
        }

        protected override void Launch() {
            camera.orthographic = isOrthographic;
            if (!isOrthographic) {
                camera.usePhysicalProperties = true; // Set the camera to physicalCamera
                camera.focalLength = focalLength; // Set the focalLength
            } else {
                camera.orthographicSize = orthographicSize;
            }
            camera.nearClipPlane = nearClipPlane;
            camera.farClipPlane = farClipPlane;

            Ngin.SetEnv<nCamera>($"Camera:{this.gameObject.name}", this);

        }

        public bool isOrthographic;
        public float focalLength; // Changed from fieldOfView to focalLength
        public float orthographicSize;
        public float nearClipPlane;
        public float farClipPlane;
        
    }
}