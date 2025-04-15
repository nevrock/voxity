// custom dictionary, with any type of data as long as 

using UnityEngine;
using System.IO;
using System;
using System.IO.Compression;
using System.Text;
using System.Collections.Generic;
using System.Collections;

namespace Ngin
{
    [System.Serializable]
    public class Lexicon : INameable, IEnumerable<KeyValuePair<string, object>>
    {   
        public string Name { get; set; }
        public Dictionary<string, object> Objects {get; set;}
        
        public Lexicon()
        {
            Objects = new Dictionary<string, object>();
        }
        public Lexicon(string name) { 
            this.Name = name;
            Objects = new Dictionary<string, object>();
        }

        
        public int Length 
        {
            get { if (Objects == null) return 0; return Objects.Count; }
        }

        public T Get<T>(string name, T defaultVal = default(T))
        {
            if (Objects == null)
            {
                return defaultVal;
            }
     
            if (name.Contains(":"))
            {
                string[] nameComponents = name.Split(":");
                Lexicon m = Get<Lexicon>(nameComponents[0], null);
                if (m == null)
                    return defaultVal;

                List<string> nameComponentsRemaining = new List<string>();
                for (int k = 1; k < nameComponents.Length; k++)
                {
                    nameComponentsRemaining.Add(nameComponents[k]);
                }
                string newName = FormName(nameComponentsRemaining);
                return m.Get<T>(newName, defaultVal);
            }   
                
            bool hasVal = Objects.TryGetValue(name, out object o);

            if (hasVal && o is T val)
            {
                return val;
            } else if (hasVal)
            {
                if (o is List<float> bagOfFloats) {

                } else if (o is List<int> bagOfInts) {
                    
                }
                return defaultVal;
            }
            return defaultVal;
        }
        public Vector3 GetVector3(string name, Vector3 defaultVal = default(Vector3)) {
            List<float> vec = Get<List<float>>(name, null);
            if (vec != null && vec.Count == 3) {
                return new Vector3(vec[0], vec[1], vec[2]);
            } else {
                return defaultVal;
            }
        }
        public Color GetColor(string name, Color defaultVal = default(Color)) {
            List<float> vec = Get<List<float>>(name, null);
            if (vec != null && vec.Count == 4) {
                return new Color(vec[0], vec[1], vec[2], vec[3]);
            } else if (vec != null && vec.Count == 3)  {
                return new Color(vec[0], vec[1], vec[2]);
            } else {
                return defaultVal;
            }
        }
        public Quaternion GetQuaternion(string name, Quaternion defaultVal = default(Quaternion)) {
            List<float> vec = Get<List<float>>(name, null);
            if (vec != null && vec.Count == 4) {
                return new Quaternion(vec[0], vec[1], vec[2], vec[3]);
            } else if (vec != null && vec.Count == 3) {
                return Quaternion.Euler(vec[0], vec[1], vec[2]);
            } else {
                return defaultVal;
            }
        }
        public void Set<T>(string name, T val)
        {
            if (Objects == null)
            {
                Objects = new Dictionary<string, object>();
            }

            if (name.Contains(":"))
            {
                string[] nameComponents = name.Split(":");
                Lexicon m = Get<Lexicon>(nameComponents[0], null);
                if (m == null)
                {
                    m = new Lexicon();
                    Set<Lexicon>(nameComponents[0], m);
                }
                
                List<string> nameComponentsRemaining = new List<string>();
                for (int k = 1; k < nameComponents.Length; k++)
                {
                    nameComponentsRemaining.Add(nameComponents[k]);
                }
                string newName = FormName(nameComponentsRemaining);
                m.Set<T>(newName, val);

                return;
            }    

            bool hasVal = Objects.TryGetValue(name, out object o);
            if (hasVal)
            {
                Objects.Remove(name);
            }

            Objects.Add(name, val as object);

        }

        string FormName(List<string> nameComponents)
        {
            if (nameComponents.Count == 1)
                return nameComponents[0];

            string newName = "";
            int i = 0;
            foreach (string s in nameComponents)
            {
                newName += s;
                if (i < nameComponents.Count - 1)
                {
                    newName+=":";
                }
                i++;    
            }

            return newName;
        }

        public bool Has(string name)
        {
            bool hasVal = Objects.ContainsKey(name);
            return hasVal;
        }
        public bool Exists(string path) {
            if (!path.Contains(":")) {
                return Has(path);
            } else {
                Lexicon b = Get<Lexicon>(Utility.GetParentLexiconBin(path), null);
                if (b == null) return false;
                string[] s = path.Split(":");
                return b.Has(s[s.Length-1]);
            }
        }
        public void Remove(string name)
        {
            if (Objects == null)
                return;

            if (name.Contains(":"))
            {
                string[] nameComponents = name.Split(":");
                Lexicon m = Get<Lexicon>(nameComponents[0], null);
                if (m == null)
                    return;

                List<string> nameComponentsRemaining = new List<string>();
                for (int k = 1; k < nameComponents.Length; k++)
                {
                    nameComponentsRemaining.Add(nameComponents[k]);
                }
                string newName = FormName(nameComponentsRemaining);
                m.Remove(newName);
                return;
            }  

            Objects.Remove(name);
        }
        public void Remove(List<string> keys)
        {
            foreach (string k in keys) {
                Remove(k);
            }
        }
        public void Log(string identifierText = "")
        {
            if (Objects == null)
                return;

            Debug.Log($"::map - {Name}, {identifierText}::\n {LexiconConverter.GetString(this)}");
        }
        public void WriteSave(string path) {
            string filePathSave = UnityEngine.Application.persistentDataPath + "/saves/";
            Debug.Log($"dict write save! {filePathSave + path}");
            WriteToPath(filePathSave + path);
        }
        public void WriteToPath(string path) {
            LexiconConverter.Write(this, path);
        }

        public object GetObject(string name)
        {
            bool hasVal = Objects.TryGetValue(name, out object o);
            if (hasVal)
                return o;
            
            return default(object);
        }

        public void Clear()
        {
            Objects.Clear();
        }

        public void Sync(Lexicon other, List<string> exceptions = null)
        {
            foreach (var kvp in other.Objects)
            {
                if (exceptions != null && exceptions.Contains(kvp.Key))
                    continue;
                    
                if (this.Has(kvp.Key))
                {
                    object o = this.Objects[kvp.Key];
                    object objectOther = kvp.Value;
                    //Log.Console($"map sync found match '{kvp.Key}', this has type {o.GetType().Name}, other has type {objectOther.GetType().Name}");
                    if (o is Lexicon m)
                    {
                        if (objectOther is Lexicon mOther)
                        {
                            m.Sync(mOther);
                        } else
                        {
                            // other is not a map but ours is
                            this.Remove(kvp.Key);
                            this.Objects.Add(kvp.Key, objectOther);
                        }
                    } else
                    {
                        // not a map, so we remove our version
                        this.Remove(kvp.Key);
                        this.Objects.Add(kvp.Key, objectOther);
                    }
                } else
                {
                    //Log.Console($"map sync did not find match for key {kvp.Key}");
                    this.Objects.Add(kvp.Key, kvp.Value);
                }
            }
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        public static Lexicon FromLexicon(TextAsset text) {
            return LexiconConverter.Read(text);
        }
        public static Lexicon FromResourcesLexicon(string fileName, string typeName = "") {
            return FromLexicon(UnityEngine.Resources.Load<TextAsset>("Data/" + typeName + fileName));
        }
        public static Lexicon FromLexiconFilePath(string filepath) {
            return LexiconConverter.ReadFromFile(filepath);
        }
        public static Lexicon FromLexiconFileSave(string fileName) {
            string filePathSave = UnityEngine.Application.persistentDataPath + "/saves/";
            return LexiconConverter.ReadFromFile(filePathSave + fileName);
        }
        public static Lexicon FromLexiconString(string text) {
            return LexiconConverter.ReadFromString(text);
        }
    }
}