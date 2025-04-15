namespace Ngin {
    using UnityEngine;
    public static class Game {
        // This class mainly deals with runtime actions and data
        private static Lexicon _env;

        public static T GetEnv<T>(string name, T defaultVal = default(T)) {
            return Env.Get<T>(name, defaultVal);
        }
        public static void SetEnv<T>(string name, T value) {
            Env.Set(name, value);
        }
        public static Lexicon Env {
            get {
                if (_env == null) {
                    RefreshEnv();
                }
                return _env;
            }
        }
        private static void RefreshEnv() {
            string envSavePath = Application.persistentDataPath + "/saves/" + GameSave;
            if (!System.IO.File.Exists(GameSave)) {
                _env = new Lexicon();
            } else {
                _env = Lexicon.FromLexiconFileSave(GameSave);
            }
        }

        private static string _scene;
        private static Signal _sceneSignal;
        public static string Scene {
            get {
                return _scene;
            }
            set {
                _scene = value;
                SceneSignal.Execute();
            }
        }
        public static Signal SceneSignal {
            get {
                if (_sceneSignal == null) {
                    _sceneSignal = new Signal();
                }   
                return _sceneSignal;
            }
        }
        public static void ListenToSceneSignal(Task t) {
            SceneSignal.AddListener(t);
        }

        public const string GameSave = "Game";
    }
}