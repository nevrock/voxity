namespace Ngin {
    using UnityEngine;
    using System;
    public class GameStateSpawner : nComponent { 
        public string state;
        public string objectName;
        protected override void AddClasses() {
        }
        protected override void StoreData(Lexicon data) {
            state = data.Get<string>("state", "Start");
            objectName = data.Get<string>("objectName", "Start");
        }
        protected override void Launch() {
            Game.ListenToSceneSignal(new Task(() => {
                //nObject.Spawn(objectName);
            }));
        }
    }
}