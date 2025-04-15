using UnityEngine;
using UnityEditor;

using System.IO;

[UnityEditor.AssetImporters.ScriptedImporter( 1, "lexf" )]
public class LexiconAssetImporter : UnityEditor.AssetImporters.ScriptedImporter {
    public override void OnImportAsset( UnityEditor.AssetImporters.AssetImportContext ctx ) {
        TextAsset subAsset = new TextAsset( File.ReadAllText( ctx.assetPath ) );
        ctx.AddObjectToAsset( "text", subAsset );
        ctx.SetMainObject( subAsset );
    }
}