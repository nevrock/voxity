using UnityEngine;
using System.IO;
using System;
using System.Collections.Generic;

namespace Ngin {
    public static class Ngin {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Init() {
        }

        public static T GetEnv<T>(string name, T defaultVal = default(T)) {
            return Env.Get<T>(name, defaultVal);
        }
        public static void SetEnv<T>(string name, T value) {
            Env.Set(name, value);
        }

        public static void SaveEnv() {
            _env.WriteSave(EnvSave);
        }
        public static void RefreshEnv() {
            string envSavePath = Application.persistentDataPath + "/saves/" + EnvSave;
            if (!System.IO.File.Exists(EnvSave)) {
                _env = Lexicon.FromResourcesLexicon(EnvLoad);
            } else {
                _env = Lexicon.FromLexiconFileSave(EnvSave);
            }
        }
        public static Lexicon Env {
            get {
                if (_env == null) {
                    RefreshEnv();
                }
                return _env;
            }
        }

        public static string Game;




        private static Lexicon _env;
        public const string SceneStart = "Scene/Start";
        public const string EnvSave = "Env";
        public const string EnvLoad = "Env";
        
    }
}