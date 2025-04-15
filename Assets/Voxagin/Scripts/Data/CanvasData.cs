namespace Ngin {
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using System.Collections.Generic;
    [System.Serializable]
    public class CanvasData : nData {

        public string name;
        public string renderMode;
        public string scaleMode;
        public int sortOrder;
        public float planeDistance;
        public float matchWidthOrHeight;
        public bool isPixelPerfect;

        Canvas _canvas;

        public CanvasData(string name) {
            this.name = name;
        }
        public CanvasData() {
            
        }
        public override void LoadFromLexicon(Lexicon lexicon) {
            name = lexicon.Get<string>("name", "CanvasMain");
            renderMode = lexicon.Get<string>("renderMode", "ScreenSpaceOverlay");
            scaleMode = lexicon.Get<string>("scaleMode", "ConstantPixelSize");
            sortOrder = lexicon.Get<int>("sortOrder", 0);
            planeDistance = lexicon.Get<float>("planeDistance", 1f);
            matchWidthOrHeight = lexicon.Get<float>("matchWidthOrHeight", 0.5f);
            isPixelPerfect = lexicon.Get<bool>("isPixelPerfect", false);
        }
        public void Apply(Canvas canvas, 
                        CanvasScaler scaler, 
                        GraphicRaycaster raycaster,
                        EventSystem eventSystem, 
                        PointerEventData pointerEventData) {
            _canvas = canvas;
            canvas.pixelPerfect = isPixelPerfect;
            ConfigureCanvas(canvas);
            ConfigureScaler(scaler);
        }
        public void SetCamera(Camera camera) {
            _canvas.worldCamera = camera;
        }
        void ConfigureCanvas(Canvas canvas) {
            ConfigureRenderMode(canvas);
            canvas.sortingOrder = sortOrder;
        }
        void ConfigureRenderMode(Canvas canvas) {
            switch (renderMode) {
                case "ScreenSpaceOverlay":
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    break;
                case "WorldSpace":
                    canvas.renderMode = RenderMode.WorldSpace;
                    break;
                case "ScreenSpaceCamera":
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                    canvas.planeDistance = planeDistance;
                    break;
            }
            
        }
        void ConfigureScaler(CanvasScaler scaler) {
            switch (scaleMode) {
                case "ConstantPixelSize":
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
                    break;
                case "ScaleWithScreenSize":
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(Ngin.GetEnv<int>("screen:width", 1920), Ngin.GetEnv<int>("screen:height", 1080));
                    scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                    scaler.matchWidthOrHeight = matchWidthOrHeight;
                    break;
                case "ConstantPhysicalSize":
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPhysicalSize;
                    break;
            }
        }
    }
}