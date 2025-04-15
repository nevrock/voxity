using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;

[ScriptedImporter(1, "ugif")]
public class GifAssetImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        List<Texture2D> frames = new List<Texture2D>();
        using (var gifImage = Image.FromFile(ctx.assetPath))
        {
            var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
            int frameCount = gifImage.GetFrameCount(dimension);

            for (int i = 0; i < frameCount; i++)
            {
                gifImage.SelectActiveFrame(dimension, i);
                using (var ms = new MemoryStream())
                {
                    gifImage.Save(ms, ImageFormat.Png);
                    var frameTexture = new Texture2D(2, 2);
                    frameTexture.LoadImage(ms.ToArray());
                    frames.Add(frameTexture);
                }
            }
        }

        string folderPath = Path.Combine(Path.GetDirectoryName(ctx.assetPath), Path.GetFileNameWithoutExtension(ctx.assetPath));
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder("Assets", Path.GetFileNameWithoutExtension(ctx.assetPath));
        }

        for (int i = 0; i < frames.Count; i++)
        {
            var sprite = Sprite.Create(frames[i], new Rect(0, 0, frames[i].width, frames[i].height), new Vector2(0.5f, 0.5f));
            string spritePath = Path.Combine(folderPath, $"frame_{i}.png");
            File.WriteAllBytes(spritePath, frames[i].EncodeToPNG());
            AssetDatabase.ImportAsset(spritePath, ImportAssetOptions.ForceUpdate);
            ctx.AddObjectToAsset($"frame_{i}", sprite);
        }

        ctx.SetMainObject(frames[0]);
    }
}
