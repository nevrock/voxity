using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;
using System.Collections.Generic;
using Ngin;

[ScriptedImporter(1, "nobj")]
public class PrefabAssetImporter : ScriptedImporter {
    public override void OnImportAsset(AssetImportContext ctx) {
        // Read the file content as a Lexicon
        Lexicon lexicon = Lexicon.FromLexiconFilePath(ctx.assetPath);
        string name = Path.GetFileNameWithoutExtension(ctx.assetPath);
        nObjectBuilder obj = new nObjectBuilder(name, lexicon);
        obj.Setup();
        obj.Build();

        List<GameObject> allObjects = obj.GetAllGameObjects();
        foreach (var go in allObjects) {
            ctx.AddObjectToAsset(go.name, go);
        }
        ctx.SetMainObject(obj.GameObject);
    }
}
