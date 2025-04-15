namespace Ngin {
    using System.Collections.Generic;
    using UnityEngine;
    [System.Serializable]
    public class LightData : nData {

        public string name;
        public bool isSun;
        public Color color;
        public float intensity;
        public string shadowType;
        public float range;

        public LightData(string name) {
            this.name = name;
        }
        public override void LoadFromLexicon(Lexicon lexicon) {
            name = lexicon.Get<string>("name", "light");
            isSun = lexicon.Get<bool>("isSun", true);
            color = lexicon.GetColor("color", Color.white);
            intensity = lexicon.Get<float>("intensity", 1f);
            range = lexicon.Get<float>("range", 10f);
            shadowType = lexicon.Get<string>("shadowType", "Soft");
        }
    }
}