using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility for managing manifest.json copying operations in both directions
/// </summary>
public class ManifestCopyUtility : EditorWindow
{
    private const string MenuItemCopyFromTemplate = "DDD/Copy Manifest From Template";
    private const string MenuItemUpdateTemplate = "DDD/Update Template Manifest";
    private const string SourcePath = "Assets/CoreProject/Packages/manifest.json";
    
    [MenuItem(MenuItemCopyFromTemplate)]
    private static void CopyFromTemplate()
    {
        string sourcePath = Path.GetFullPath(SourcePath);
        string destinationPath = Path.GetFullPath("Packages/manifest.json");
        
        CopyManifestFile(sourcePath, destinationPath, "template", "project");
    }

    [MenuItem(MenuItemUpdateTemplate)]
    private static void UpdateTemplate()
    {
        string sourcePath = Path.GetFullPath("Packages/manifest.json");
        string destinationPath = Path.GetFullPath(SourcePath);
        
        CopyManifestFile(sourcePath, destinationPath, "project", "template");
    }
    
    private static void CopyManifestFile(string sourcePath, string destinationPath, string sourceDesc, string destDesc)
    {
        // Check if source exists
        if (!File.Exists(sourcePath))
        {
            EditorUtility.DisplayDialog(
                "Error",
                $"Source manifest not found at path: {sourcePath}",
                "OK"
            );
            return;
        }
        
        // Warn user about overwrite
        bool shouldProceed = EditorUtility.DisplayDialog(
            "Warning",
            $"This will completely replace the contents of your {destDesc} manifest.json file with the {sourceDesc} manifest. This operation cannot be undone. Are you sure you want to proceed?",
            "Yes, Replace",
            "Cancel"
        );
        
        if (!shouldProceed)
        {
            return;
        }
        
        try
        {
            // Create directory if it doesn't exist
            string destinationDir = Path.GetDirectoryName(destinationPath);
            if (!Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }
            
            // Perform the copy operation
            File.Copy(sourcePath, destinationPath, true);
            
            // Refresh the AssetDatabase to ensure Unity picks up the changes
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Success",
                $"Manifest file has been successfully copied from {sourceDesc} to {destDesc}.",
                "OK"
            );
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to copy manifest: {e.Message}");
            EditorUtility.DisplayDialog(
                "Error",
                $"Failed to copy manifest: {e.Message}",
                "OK"
            );
        }
    }
}