using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;
using System.IO;
using Ngin;
using System.Collections.Generic;

[ScriptedImporter(1, "nmat")]
public class MaterialAssetImporter : ScriptedImporter {
    public override void OnImportAsset(AssetImportContext ctx) {
        // Read the file content as a Lexicon
        Lexicon lexicon = Lexicon.FromLexiconFilePath(ctx.assetPath);

        // Debugging shader name
        Debug.Log(lexicon.Get<string>("shader"));

        string shaderName = lexicon.Get<string>("shader");
        shaderName = "Shader Graphs/" + shaderName;

        Shader shader = Shader.Find(shaderName);

        // Debugging shader object
        Debug.Log(shader);

        // Validate shader
        if (shader == null) {
            Debug.Log($"Shader '{shaderName}' not found.");
            return;
        }

        // Create a new material
        Material material = new Material(shader);
        material.name = lexicon.Get<string>("name");

        // Load textures and assign to material
        Lexicon properties = lexicon.Get<Lexicon>("properties");
        foreach (var kvp in properties) {
            Debug.Log($"Property: {kvp.Key}, Value: {kvp.Value}");
            if (kvp.Value is float) {
                material.SetFloat(kvp.Key, (float)kvp.Value);
            } else if (kvp.Value is int) {
                material.SetInt(kvp.Key, (int)kvp.Value);
            } else if (kvp.Value is List<float> && ((List<float>)kvp.Value).Count == 3) {
                List<float> list = properties.Get<List<float>>(kvp.Key);
                material.SetColor(kvp.Key, new Color(list[0], list[1], list[2]));
            } else if (kvp.Value is string) {
                Texture texture = Resources.Load<Texture>("Texture/" + kvp.Value.ToString());
                if (texture != null) {
                    Debug.Log($"Texture '{kvp.Value}' found for property '{kvp.Key}'.");
                    material.SetTexture(kvp.Key, texture);
                } else {
                    Debug.Log($"Texture '{kvp.Value}' not found for property '{kvp.Key}'.");
                }
            } else {
                Debug.Log($"Unsupported property type for '{kvp.Key}'.");
            }
        }

        // Add the material to the asset
        ctx.AddObjectToAsset("Main Material", material);
        ctx.SetMainObject(material);

        // Ensure the material is serialized correctly
        EditorUtility.SetDirty(material);
    }

}
