using UnityEngine;

namespace Ngin {
    public class nLight : nComponent {
        [ReadOnly]
        public Light light;
        public LightData lightData;

        protected override void AddClasses() {
            light = ComponentCheck<Light>(true);
        }
        protected override void StoreData(Lexicon data) {
            lightData = new LightData(data.Get<string>("name", "light"));
            lightData.LoadFromLexicon(data);
        }

        protected override void Launch() {
            if (lightData.isSun)
                light.type = LightType.Directional;
            else
                light.type = LightType.Point;

            light.shadows = LightShadows.Soft;
            light.intensity = lightData.intensity;
            light.bounceIntensity = 1.0f;
            light.range = lightData.range;
            light.color = lightData.color;
        }


        // data
        
    }
}