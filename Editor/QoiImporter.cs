using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.AssetImporters;


[ScriptedImporter(1, "qoi")]
public class QoiImporter : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        if (ctx.assetPath.EndsWith(".qoi", StringComparison.InvariantCultureIgnoreCase))
        {
            FileStream stream = File.OpenRead(ctx.assetPath);
            if (stream == null)
            {
                ctx.LogImportError($"Failed to open QOI image file '{ctx.assetPath}'!");
                return;
            }

            byte[] data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            QoiSharp.QoiImage img = QoiSharp.QoiDecoder.Decode(data);
            if (img == null)
            {
                ctx.LogImportError($"Failed to decode QOI image '{ctx.assetPath}'!");
                return;
            }

            TextureFormat format;
            switch (img.Channels)
            {
                case QoiSharp.Codec.Channels.Rgb:
                {
                    format = TextureFormat.RGB24;
                    break;
                }
                case QoiSharp.Codec.Channels.RgbWithAlpha:
                {
                    format = TextureFormat.RGBA32;
                    break;
                }
                default:
                {
                    ctx.LogImportError($"Unhandled QOI channel format '{img.Channels}'!");
                    return;
                }
            }

            bool bIsLinear = img.ColorSpace == QoiSharp.Codec.ColorSpace.Linear;
            Texture2D tex = new Texture2D(img.Width, img.Height, format, true, bIsLinear);

            int slashIdx = ctx.assetPath.LastIndexOf('/');
            if (slashIdx < 0)
            {
                slashIdx = 0;
            }
            
            int dotIdx = ctx.assetPath.LastIndexOf('.');
            Debug.Assert(dotIdx >= 0);

            tex.name = ctx.assetPath.Substring(slashIdx, dotIdx - slashIdx);

            tex.SetPixelData(img.Data, 0);
            if (format == TextureFormat.RGBA32)
            {
                tex.alphaIsTransparency = true;
            }
            tex.Apply();

            stream.Close();

            ctx.AddObjectToAsset(tex.name, tex, tex);
            ctx.SetMainObject(tex);
        }
    }
}
