/***********************************************************

  Copyright (c) 2020-present Clicked, Inc.

  Licensed under the license found in the LICENSE file 
  in the Docs folder of the distributed package.

 ***********************************************************/

using System.Collections.Generic;
using System.IO;
using UnityEditor;

public class AirViewPackageBuilder  {
    private const string Version = "2.4.1";

    [MenuItem("onAirView/Export...")]
    public static void Export() {
        exportPackage("Export onAirView...", "onairview_" + Version, new string[] { "Assets/onAirView" });
    }

    private static void exportPackage(string dialogTitle, string defaultName, string[] assetPaths) {
        var targetPath = EditorUtility.SaveFilePanel(dialogTitle, "", defaultName, "unitypackage");
        if (string.IsNullOrEmpty(targetPath)) { return; }

        if (File.Exists(targetPath)) {
            File.Delete(targetPath);
        }

        var assetids = AssetDatabase.FindAssets("", assetPaths);
        var assets = new List<string>();
        foreach (var assetid in assetids) {
            assets.Add(AssetDatabase.GUIDToAssetPath(assetid));
        }

        AssetDatabase.ExportPackage(assets.ToArray(), targetPath);

        EditorUtility.DisplayDialog("Exported", "Exported successfully.\n\n" + targetPath, "Close");
    }
}
