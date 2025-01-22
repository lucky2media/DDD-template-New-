using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class GitSubmoduleManager : EditorWindow
{
    private string gitToken = "";
    private bool showToken = false;
    private Vector2 scrollPosition;
    private List<SubmoduleInfo> submodules = new List<SubmoduleInfo>();
    private bool isRefreshing = false;
    private Dictionary<string, string> commitMessages = new Dictionary<string, string>();

    [MenuItem("Tools/Git Submodule Manager")]
    public static void ShowWindow()
    {
        GetWindow<GitSubmoduleManager>("Git Submodule Manager");
    }

    private class SubmoduleInfo
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string CurrentCommit { get; set; }
        public bool NeedsPull { get; set; }
        public bool HasLocalChanges { get; set; }
    }

    private void OnGUI()
    {
        GUILayout.Label("Git Submodule Manager", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (showToken)
        {
            gitToken = EditorGUILayout.TextField("Git Token:", gitToken);
        }
        else
        {
            gitToken = EditorGUILayout.PasswordField("Git Token:", gitToken);
        }
        if (GUILayout.Button(showToken ? "Hide" : "Show", GUILayout.Width(50)))
        {
            showToken = !showToken;
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Save Token"))
        {
            EditorPrefs.SetString("GitSubmoduleManager_Token", gitToken);
            ShowNotification(new GUIContent("Token saved!"));
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Refresh Submodules"))
        {
            RefreshSubmodules();
        }

        EditorGUILayout.Space();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        
        if (isRefreshing)
        {
            EditorGUILayout.HelpBox("Refreshing submodules...", MessageType.Info);
        }
        else if (submodules.Count == 0)
        {
            EditorGUILayout.HelpBox("No submodules found in the project.", MessageType.Info);
        }
        else
        {
            foreach (var submodule in submodules)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField($"Name: {submodule.Name}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Path: {submodule.Path}");
                EditorGUILayout.LabelField($"Current Commit: {submodule.CurrentCommit}");
                
                if (submodule.HasLocalChanges)
                {
                    EditorGUILayout.HelpBox("Has local changes", MessageType.Info);
                    
                    // Commit message field
                    if (!commitMessages.ContainsKey(submodule.Path))
                    {
                        commitMessages[submodule.Path] = "";
                    }
                    commitMessages[submodule.Path] = EditorGUILayout.TextField("Commit Message:", commitMessages[submodule.Path]);

                    if (GUILayout.Button("Push Changes"))
                    {
                        PushSubmodule(submodule, commitMessages[submodule.Path]);
                    }
                }

                GUI.enabled = submodule.NeedsPull;
                if (GUILayout.Button("Pull Latest"))
                {
                    PullSubmodule(submodule);
                }
                GUI.enabled = true;

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void OnEnable()
    {
        gitToken = EditorPrefs.GetString("GitSubmoduleManager_Token", "");
        RefreshSubmodules();
    }

    private void RefreshSubmodules()
    {
        isRefreshing = true;
        submodules.Clear();

        string projectPath = Path.GetDirectoryName(Application.dataPath);
        
        // Check if .gitmodules exists
        string gitmodulesPath = Path.Combine(projectPath, ".gitmodules");
        if (!File.Exists(gitmodulesPath))
        {
            isRefreshing = false;
            return;
        }

        // Read .gitmodules file
        string[] lines = File.ReadAllLines(gitmodulesPath);
        SubmoduleInfo currentSubmodule = null;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();
            
            if (trimmedLine.StartsWith("[submodule"))
            {
                if (currentSubmodule != null)
                {
                    submodules.Add(currentSubmodule);
                }
                currentSubmodule = new SubmoduleInfo();
            }
            else if (currentSubmodule != null)
            {
                string[] parts = trimmedLine.Split('=').Select(p => p.Trim()).ToArray();
                if (parts.Length == 2)
                {
                    if (parts[0] == "path")
                    {
                        currentSubmodule.Path = parts[1];
                        currentSubmodule.Name = Path.GetFileName(parts[1]);
                    }
                }
            }
        }

        if (currentSubmodule != null)
        {
            submodules.Add(currentSubmodule);
        }

        // Get current commits and check for updates
        foreach (var submodule in submodules)
        {
            UpdateSubmoduleStatus(submodule, projectPath);
        }

        isRefreshing = false;
        Repaint();
    }

    private void UpdateSubmoduleStatus(SubmoduleInfo submodule, string projectPath)
    {
        // Get current commit
        Process process = new Process();
        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = $"rev-parse HEAD";
        process.StartInfo.WorkingDirectory = Path.Combine(projectPath, submodule.Path);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        try
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            submodule.CurrentCommit = output.Trim();

            // Check if updates are available
            process.StartInfo.Arguments = $"remote update";
            process.Start();
            process.WaitForExit();

            process.StartInfo.Arguments = $"status -uno";
            process.Start();
            output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            submodule.NeedsPull = output.Contains("behind");

            // Check for local changes
            process.StartInfo.Arguments = "status --porcelain";
            process.Start();
            output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            submodule.HasLocalChanges = !string.IsNullOrWhiteSpace(output);
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"Error checking submodule status: {e.Message}");
            submodule.CurrentCommit = "Error";
            submodule.NeedsPull = false;
            submodule.HasLocalChanges = false;
        }
    }

    private void PullSubmodule(SubmoduleInfo submodule)
    {
        string projectPath = Path.GetDirectoryName(Application.dataPath);
        
        Process process = new Process();
        process.StartInfo.FileName = "git";
        process.StartInfo.Arguments = $"pull";
        process.StartInfo.WorkingDirectory = Path.Combine(projectPath, submodule.Path);
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;

        if (!string.IsNullOrEmpty(gitToken))
        {
            process.StartInfo.EnvironmentVariables["GIT_ASKPASS"] = "git-credential-manager";
            process.StartInfo.EnvironmentVariables["GCM_CREDENTIAL"] = gitToken;
        }

        try
        {
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                ShowNotification(new GUIContent($"Successfully pulled {submodule.Name}"));
                RefreshSubmodules();
            }
            else
            {
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to pull submodule {submodule.Name}. Check the console for details.", "OK");
                UnityEngine.Debug.LogError($"Git pull error: {output}");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", 
                $"Failed to execute git pull: {e.Message}", "OK");
        }
    }

    private void PushSubmodule(SubmoduleInfo submodule, string commitMessage)
    {
        if (string.IsNullOrWhiteSpace(commitMessage))
        {
            EditorUtility.DisplayDialog("Error", "Please enter a commit message", "OK");
            return;
        }

        string projectPath = Path.GetDirectoryName(Application.dataPath);
        string submodulePath = Path.Combine(projectPath, submodule.Path);

        // Series of git commands to execute
        var commands = new[]
        {
            new { Args = "add .", Msg = "Adding changes..." },
            new { Args = $"commit -m \"{commitMessage}\"", Msg = "Committing changes..." },
            new { Args = "push", Msg = "Pushing changes..." }
        };

        foreach (var cmd in commands)
        {
            Process process = new Process();
            process.StartInfo.FileName = "git";
            process.StartInfo.Arguments = cmd.Args;
            process.StartInfo.WorkingDirectory = submodulePath;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.CreateNoWindow = true;

            if (!string.IsNullOrEmpty(gitToken))
            {
                process.StartInfo.EnvironmentVariables["GIT_ASKPASS"] = "git-credential-manager";
                process.StartInfo.EnvironmentVariables["GCM_CREDENTIAL"] = gitToken;
            }

            try
            {
                EditorUtility.DisplayProgressBar("Pushing Changes", cmd.Msg, 0.5f);
                
                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    EditorUtility.ClearProgressBar();
                    EditorUtility.DisplayDialog("Error", 
                        $"Failed during {cmd.Msg}\n{error}", "OK");
                    UnityEngine.Debug.LogError($"Git error: {error}");
                    return;
                }
            }
            catch (System.Exception e)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("Error", 
                    $"Failed to execute git command: {e.Message}", "OK");
                return;
            }
        }

        EditorUtility.ClearProgressBar();
        ShowNotification(new GUIContent($"Successfully pushed changes for {submodule.Name}"));
        commitMessages[submodule.Path] = ""; // Clear the commit message
        RefreshSubmodules();
    }
}