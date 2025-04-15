using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

namespace Ngin
{
    public class nImage : nComponent
    {
        Image image;

        int refreshImageState = 0;
        bool hasSprite = false;

        string spriteName;
        string imageName;
        float ppuMultiplier;
        string type;
        bool fillCenter;
        string textureName;
        string textureType;
        List<float> pivot;
        bool raycasts;
        string colorHex;
        float alpha;
        protected override void StoreData(Lexicon data)
        {
            spriteName = data.Get<string>("sprite", "");
            imageName = data.Get<string>("image", "");
            ppuMultiplier = data.Get<float>("ppuMultiplier", 2.0f);
            type = data.Get<string>("type", "sliced");
            fillCenter = data.Get<bool>("fill_center", true);
            textureName = data.Get<string>("texture", "");
            textureType = data.Get<string>("textureType", "");
            pivot = data.Get<List<float>>("texturePivot", new List<float>(){0.5f, 0.5f});
            raycasts = data.Get<bool>("raycasts", false);
            colorHex = data.Get<string>("color", "#000000");
            alpha = data.Get<float>("alpha", 1f);
        }

        protected override void AddClasses()
        {
            image = ComponentCheck<Image>(true);
        }
        protected override void Setup()
        {
            base.Setup();

            Configure();
        }
        protected override void Launch() {
            base.Launch();
        }

        public void SetSprite(Sprite s) {
            image.sprite = s;
        }

        public Color Color {
            get { 
                return image.color;
            }
            set {
                image.color = value;
            }
        }

        protected override void Tick() {
            base.Tick();
        }

        void Configure()
        {
            if (spriteName != "") {
                Sprite sprite = Assets.GetSprite(imageName, spriteName);
                if (sprite != null) image.sprite = sprite;

                image.pixelsPerUnitMultiplier = ppuMultiplier;
                switch (type) {
                    case "sliced":
                        image.type = Image.Type.Sliced;
                        break;
                    case "simple":
                        image.type = Image.Type.Simple;
                        break;
                }
                image.fillCenter = fillCenter;
                
                hasSprite = true;
            } else {
                if (textureName != "")
                {
                    Texture2D tex = Assets.GetTexture(textureName);
                    if (tex != null)
                    {
                        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(pivot[0], pivot[1]), 100.0f);
                        image.sprite = sprite;
                    }
                }
            }
            
            image.raycastTarget = raycasts;

            Color colorHexU;
            if (ColorUtility.TryParseHtmlString(colorHex, out colorHexU))
            {
                colorHexU.a = alpha;
                image.color = colorHexU;
            }
        }

    }
}