namespace Ngin {
    using UnityEngine;
    public class NginSystem : MonoBehaviour {

        void Awake() {
            //_origin = nPrefab.Spawn(startObject);
            //Debug.Log("Origin: " + _origin);
            //_gui = nGuiObject.Spawn(guiObject);
            //Debug.Log("Gui: " + _gui);
        }
        void Start() {
            //_origin.LogConsole();
            //_gui.LogConsole();
            Game.Scene = "Start";
        }

       // public string startObject = "Start";
        //public string guiObject = "Gui";

        //GameObject _origin;
        //GameObject _gui;
    }
}