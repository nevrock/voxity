using System;
using UnityEngine;
using System.IO;

namespace Ngin
{
    public static class Assets
    {
        public static Lexicon GetLexicon(string path)
        {
            return Lexicon.FromResourcesLexicon(path);
        }
        public static Sprite GetSprite(string imageName, string spriteName)
        {
            Sprite[] all = Resources.LoadAll<Sprite>("Sprite/"+imageName);
            foreach( var s in all)
            {
                if (s.name == spriteName)
                {
                    return s;
                }
            }
            return null;
        }
        public static Texture2D GetTexture(string textureName)
        {
            return Resources.Load<Texture2D>("Texture/"+textureName);
        }
        public static GameObject GetObject(string objectName)
        {
            return Resources.Load<GameObject>("Object/"+objectName);
        }
        public static Material GetMaterial(string materialName)
        {
            return Resources.Load<Material>("Material/"+materialName);
        }
        public static Mesh GetMesh(string meshName)
        {
            return Resources.Load<Mesh>("Mesh/"+meshName);
        }
    }
}