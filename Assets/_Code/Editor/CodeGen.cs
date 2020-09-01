using System.Collections;
using BeauUtil;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System.Collections.Generic;

namespace ProtoAqua.Editor
{
    static internal class CodeGen
    {
        static private readonly string TargetFolder = "Assets/_Code/_Generated/";

        [MenuItem("Aqualab/CodeGen/Regen Layers")]
        static private void GenerateLayerConsts()
        {
            StringBuilder builder = new StringBuilder(1024);
            builder.Append("static public class GameLayers")
                .Append("\n{");

            HashSet<string> usedNames = new HashSet<string>();

            for(int i = 0; i < 32; ++i)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                {
                    if (!usedNames.Add(layerName))
                    {
                        Debug.LogWarningFormat("[CodeGen] Duplicate Unity layer name '{0}'", layerName);
                        continue;
                    }

                    int index = i;
                    int mask = 1 << i;

                    string safeName = ObjectNames.NicifyVariableName(layerName).Replace("-", "_").Replace(" ", "");

                    if (usedNames.Count > 1)
                    {
                        builder.Append('\n');
                    }

                    builder.Append("\n\t// Layer ").Append(index).Append(": ").Append(layerName);
                    builder.Append("\n\tpublic const int ").Append(safeName).Append("_Index = ").Append(index).Append(";");
                    builder.Append("\n\tpublic const int ").Append(safeName).Append("_Mask = ").Append(mask).Append(";");
                }
            }

            builder.Append("\n}");

            string outputPath = Path.Combine(TargetFolder, "GameLayers.cs");
            File.WriteAllText(outputPath, builder.Flush());
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceUpdate);
        }
    }
}