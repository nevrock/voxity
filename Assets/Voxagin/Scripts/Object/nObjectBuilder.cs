namespace Ngin {
    using UnityEngine;
    using System.Collections.Generic;
    using System;
    public class nObjectBuilder {
        // -- properties -- //
        public GameObject GameObject { get { return _gameObject; } }

        // -- privates -- //
        private string _name;
        private Lexicon _data;
        private nObjectBuilder _parent;
        private List<nObjectBuilder> _children;
        private GameObject _gameObject;

        // -- constructor -- //
        public nObjectBuilder() {
            _data = new Lexicon();
            _children = new List<nObjectBuilder>();
        }
        public nObjectBuilder(string name, string lexiconFile) {
            _name = name;
            _data = Lexicon.FromResourcesLexicon(lexiconFile);
            _children = new List<nObjectBuilder>();
        }
        public nObjectBuilder(string name, Lexicon data, nObjectBuilder parent = null) {
            _name = name;
            _data = data;
            _parent = parent;
            _children = new List<nObjectBuilder>();
        }
        
        // -- setup -- //
        public void Setup() {
            _SetupChildren(_data.Get<Lexicon>("children", new Lexicon()));
        }

        void _SetupChildren(Lexicon children) {
            _children =new List<nObjectBuilder>();
            foreach (var child in children.Objects) {
                var childData = child.Value as Lexicon;
                var childName = child.Key;

                nObjectBuilder nChild = new nObjectBuilder(childName, childData, this);
                nChild.Setup();

                _children.Add(nChild);
            }
        }

        // -- build -- //
        public void Build() {
            _BuildGameObject(); 
            _BuildComponents(_data.Get<Lexicon>("components", new Lexicon()));

            _BuildChildren();
        }
        
        void _BuildChildren() {
            foreach (var child in _children) {
                child.Build();
            }
        }

        void _BuildComponents(Lexicon components) {
            List<nComponent> componentsNew = new List<nComponent>();
            foreach (var component in components.Objects) {
                var componentName = component.Key;
                var componentData = component.Value as Lexicon;
                string type = componentData.Get<string>("type");
                componentsNew.Add(_AddComponent(type, componentData));
            }
            foreach (var component in componentsNew) {
                component.EditorRefresh();
            }
        }

        nComponent _AddComponent(string componentTypeName, Lexicon vars = null) {
            Debug.Log("Adding component: " + componentTypeName);
            
            Type classType = Utility.GetNginType(componentTypeName);

            object o = null;

            Component component = _gameObject.GetComponent(classType);
            if (component == null)
                component = _gameObject.AddComponent(classType);

            o = component as object;

            nComponent a = o as nComponent;

            if (vars == null)
                vars = new Lexicon();
                
            a.Configure(vars);

            return a;
        }

        void _BuildGameObject() {
            if (_data.Has("FBXFile")) {
                _BuildFBX();
            } else {
                if (_parent != null) {
                    GameObject parentObject = _parent.GameObject;
                    Transform child = parentObject.transform.Find(_name);
                    if (child != null) {
                        _gameObject = child.gameObject;
                    }
                }
                if (_gameObject == null)
                    _BuildEmpty();
            }

            if (_data.Has("transform")) {
                TransformData data = new TransformData();
                Debug.Log("Building TransformData object: " + _name);
                data.LoadFromLexicon(_data.Get<Lexicon>("transform", new Lexicon()));
                data.Link(_gameObject.transform);
                data.Apply();
            }
        }
        void _BuildFBX() {
            Debug.Log("Building fbx: " + _name);
            string fbxName = _data.Get<string>("FBXFile");
            GameObject pfb = Resources.Load<GameObject>("Mesh/" + fbxName);
            if (_parent != null) {
                _gameObject = GameObject.Instantiate(pfb, _parent._gameObject.transform);
            } else {
                _gameObject = GameObject.Instantiate(pfb);
            }
        }
        void _BuildEmpty() {
            Debug.Log("Building empty: " + _name);
            _gameObject = new GameObject(_name);
            if (_parent != null) {
                _gameObject.transform.SetParent(_parent._gameObject.transform);
            }
        }

        // -- gets -- //
        public List<GameObject> GetAllGameObjects(List<GameObject> currentList = null) {
            if (currentList == null)
                currentList = new List<GameObject>();
            currentList.Add(_gameObject);
            foreach (var child in _children) {
                child.GetAllGameObjects(currentList);
            }
            return currentList;
        }
    }
}