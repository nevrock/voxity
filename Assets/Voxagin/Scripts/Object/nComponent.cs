using UnityEngine;
using System.Collections.Generic;
namespace Ngin {
    public class nComponent : MonoBehaviour {
        public void Configure(Lexicon data) {
            StoreData(data);
            AddClasses();
            Setup();
        }

        void Awake() {
            AddClasses();
            Setup();
        }
        void Start() {
            Launch();
        }
        void Update() {
            Tick();
        }
        void LateUpdate() {
            TickLate();
        }
        void FixedUpdate() {
            TickPhysics();
        }

        public virtual void EditorRefresh() {
            AddClasses();
            Setup();
            Launch();
        }

        protected virtual void StoreData(Lexicon data) {}
        protected virtual void AddClasses() {}
        protected virtual void Setup() {}
        protected virtual void Launch() {}
        protected virtual void Tick() {}
        protected virtual void TickLate() {}
        protected virtual void TickPhysics() {}

        public T ComponentCheck<T>(bool forceAdd = true) where T : Component
        {
            T val = this.gameObject.GetComponent<T>();
            if (val == null && forceAdd)
                return this.gameObject.AddComponent<T>();

            return val;
        }
        protected Transform FindChild(string name, Transform parent = null) {
            if (parent == null) {
                parent = this.transform;
            }
            if (parent.name == name) {
                return parent;
            }
            foreach (Transform child in parent) {
                Transform result = FindChild(name, child);
                if (result != null) {
                    return result;
                }
            }
            return null;
        }
        protected GameObject FindChildObject(string name, Transform parent = null) {
            Transform t = FindChild(name, parent);
            if (t == null)
                return null;
            return t.gameObject;
        }
        protected List<T> GetComponentsInChildren<T>(List<T> components = null, GameObject obj = null) where T : Component {
            if (obj == null) {
                obj = this.gameObject;
            }
            if (components == null) {
                components = new List<T>();
            }
            T component = obj.GetComponent<T>();
            if (component != null) {
                components.Add(component);
            }
            foreach (Transform child in obj.transform) {
                GetComponentsInChildren<T>(components, child.gameObject);
            }
            return components;
        }
    }
}