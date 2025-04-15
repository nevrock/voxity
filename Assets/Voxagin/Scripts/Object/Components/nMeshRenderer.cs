using UnityEngine;
using System.Collections.Generic;

namespace Ngin {
    public class nMeshRenderer : nComponent {
        [ReadOnly]
        public MeshRenderer meshRenderer;
        [ReadOnly]
        public MeshFilter meshFilter;
        public List<string> materials;

        protected override void AddClasses() {
            meshRenderer = ComponentCheck<MeshRenderer>(true);
            meshFilter = ComponentCheck<MeshFilter>(true);
        }
        protected override void StoreData(Lexicon data) {
            materials = data.Get<List<string>>("materials", new List<string>());
        }

        protected override void Launch() {
            Material[] mats = new Material[materials.Count];
            for (int i = 0; i < materials.Count; i++) {
                mats[i] = Assets.GetMaterial(materials[i]);
            }
            meshRenderer.sharedMaterials = mats;
        }
    }
}