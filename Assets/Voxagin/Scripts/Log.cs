using UnityEngine;
namespace Ngin
{
    public static class Log 
    {
        private static Lexicon vars;
        private static Lexicon Vars {
            get {
                if (vars == null)
                    vars = Lexicon.FromResourcesLexicon("Prefs");

                return vars;
            }
        }
        public static void Console(string name)
        {
            if (Vars.Get<bool>("is_log", true))
                Debug.Log(name);
        }
    }
}