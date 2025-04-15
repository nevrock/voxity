using UnityEngine;
using System.Collections.Generic;

namespace Ngin {
    [ExecuteAlways]
    public class nAnimatedMeshRenderer : nComponent {
        [ReadOnly]
        public SkinnedMeshRenderer skinnedMeshRenderer;
        [ReadOnly]
        public MeshFilter meshFilter;
        public List<string> materials;

        protected override void AddClasses() {
            skinnedMeshRenderer = ComponentCheck<SkinnedMeshRenderer>(true);
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
            skinnedMeshRenderer.sharedMaterials = mats;
        }
    }
}