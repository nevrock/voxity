namespace Ngin {
    using UnityEngine;
    using System.Collections.Generic;

    public class nPrefab {
        public static GameObject Spawn(string name, Transform parent = null) {
            GameObject obj = Resources.Load<GameObject>("Object/" + name);
            return Object.Instantiate(obj, parent);
        }

        public static GameObject Spawn(string name, Vector3 position, Quaternion rotation, Vector3 scale, Transform parent = null) {
            GameObject obj = Resources.Load<GameObject>("Object/" + name);
            GameObject instance = Object.Instantiate(obj, position, rotation, parent);
            instance.transform.localScale = scale;
            return instance;
        }
    }
}