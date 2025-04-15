namespace Ngin {
    using UnityEngine;
    using UnityEngine.EventSystems;
    [ExecuteAlways]
    public class NginInputs : MonoBehaviour {
        public EventSystem eventSystem;

        void Awake() {
            Ngin.SetEnv<EventSystem>("eventSystem", eventSystem);
        }
    }
}