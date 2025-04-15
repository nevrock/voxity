namespace Ngin
{
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.UI;
    using System.Collections.Generic;

    public class nCanvas : nComponent
    {
        public CanvasData canvasData;


        Canvas canvas;
        CanvasScaler canvasScaler;
        GraphicRaycaster graphicRaycaster;
        EventSystem eventSystem;

        PointerEventData pointerEventData;
        

        protected override void AddClasses()
        {
            canvas = ComponentCheck<Canvas>();
            canvasScaler = ComponentCheck<CanvasScaler>();
            graphicRaycaster = ComponentCheck<GraphicRaycaster>();
        }
        protected override void StoreData(Lexicon data)
        {
            this.canvasData = new CanvasData();
            canvasData.LoadFromLexicon(data);
        }
        protected override void Setup()
        {
            base.Setup();
        }
        protected override void Launch() 
        {
            base.Launch();

            eventSystem = Ngin.GetEnv<EventSystem>("eventSystem");
            pointerEventData = new PointerEventData(eventSystem);

            canvasData.Apply(canvas, canvasScaler, graphicRaycaster, eventSystem, pointerEventData);

            if (canvasData != null) {
                canvasData.SetCamera(Ngin.GetEnv<nCamera>($"Camera:CameraMain").camera);
            }
        }
        protected override void Tick() {
            base.Tick();
        }
        
    }
}