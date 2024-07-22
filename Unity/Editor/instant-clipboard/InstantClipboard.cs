using UnityEngine;
using UnityEditor;
using System.IO;
using System.Reflection;
using System;
using System.Text.RegularExpressions;

public class InstantClipboard : EditorWindow
{
    private const string appVersion = "1.0.0";
    public static string pathToSaveIn = "";
    const string githubURL = "https://github.com/saadnkhawaja";
    const string twitterURL = "https://www.x.com/saadskhawaja";
    const string websiteURL = "https://www.saadkhawaja.com";


    private static void GetProjectWindowPath()
    {
        string pathToCurrentFolder = GetActiveFolderPath();

        if (!string.IsNullOrEmpty(pathToCurrentFolder))
        {
            // Construct full file path
            pathToSaveIn = Path.Combine(Application.dataPath, pathToCurrentFolder);

            Debug.Log("Path to current folder: " + pathToSaveIn);
        }
        else
        {
            pathToSaveIn = Path.Combine(Application.dataPath);

           Debug.LogError("No folder selected in the Project window.");
        }
    }

    [MenuItem("Tools/Saad Khawaja/Instant Clipboard/Follow on Twitter(X)", false, 13)]
    private static void OpenX()
    {
        Application.OpenURL(twitterURL);
    }

    [MenuItem("Tools/Saad Khawaja/Instant Clipboard/More on Github", false, 12)]
    private static void OpenGitHub()
    {
        Application.OpenURL(githubURL);
    }

    [MenuItem("Tools/Saad Khawaja/Instant Clipboard/More Developer Tools", false, 11)]
    private static void OpenWebsite()
    {
        Application.OpenURL(websiteURL);
    }


    [MenuItem("Tools/Saad Khawaja/Instant Clipboard/Paste", false, 1)]
    private static void Paste()
    {
        GetProjectWindowPath();
        string clipboardJsonPath = Path.Combine(GetDocumentsPath(), "instant-clipboard/clipboard.json");

        if (File.Exists(clipboardJsonPath))
        {
            // Read and parse clipboard.json
            string jsonContent = File.ReadAllText(clipboardJsonPath);
            ClipboardInfo clipboardInfo = JsonUtility.FromJson<ClipboardInfo>(jsonContent);

            if (clipboardInfo.version == appVersion)
            {
                if (clipboardInfo.lastAction == "image")
                {
                    PasteImage();
                }
                else if (clipboardInfo.lastAction == "text")
                {
                    ProcessText();
                }
            }
            else
            {
                Debug.LogError("Version mismatch. Clipboard.json version: " + clipboardInfo.version + ", App version: " + appVersion);
            }
        }
        else
        {
            Debug.LogError("No clipboard.json found in instant-clipboard folder.");
        }
    }


    private static void ProcessText()
    {
        GetProjectWindowPath();

        string textFilePath = Path.Combine(GetDocumentsPath(), "instant-clipboard/text.txt");

        if (File.Exists(textFilePath))
        {
            string textContent = File.ReadAllText(textFilePath);

            if (IsCSharpClass(textContent, out string className))
            {
                string classFilePath = Path.Combine(pathToSaveIn, className + ".cs");

                if (!File.Exists(classFilePath))
                {
                    File.WriteAllText(classFilePath, textContent);
                    AssetDatabase.Refresh();
                    Debug.Log("C# class file created: " + classFilePath);
                }
                else
                {
                    Debug.LogError("File already exists: " + classFilePath);
                }
            }
            else if (IsJson(textContent))
            {
                // Prompt user for filename
                string fileName = EditorUtility.SaveFilePanel("Save JSON", pathToSaveIn, "data", "json");

                if (!string.IsNullOrEmpty(fileName))
                {
                    File.WriteAllText(fileName, textContent);
                    AssetDatabase.Refresh();
                    Debug.Log("JSON file saved: " + fileName);
                }
            }
            else if (IsXml(textContent))
            {
                SaveXml(textContent);
            }
            else if (IsShaderCode(textContent))
            {
                SaveShaderCode(textContent);
            }
            else if (IsMarkdown(textContent))
            {
                SaveMarkdown(textContent);
            }
            else
            {
                Debug.LogError("Text is not a recognized format.");
            }
        }
        else
        {
            Debug.LogError("No text file found in instant-clipboard folder.");
        }
    }

    private static bool IsCSharpClass(string text, out string className)
    {
        className = null;

        // Regular expression to match class declaration outside of comments
        string pattern = @"^\s*(public|private|protected|internal)?\s*(abstract|sealed|static)?\s*class\s+(\w+)";
        Regex regex = new Regex(pattern, RegexOptions.Multiline);

        // Split the text by lines
        string[] lines = text.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        bool inCommentBlock = false;

        foreach (string line in lines)
        {
            string trimmedLine = line.Trim();

            // Skip single-line comments
            if (trimmedLine.StartsWith("//"))
            {
                continue;
            }

            // Handle multi-line comments
            if (trimmedLine.StartsWith("/*"))
            {
                inCommentBlock = true;
                continue;
            }

            if (inCommentBlock)
            {
                if (trimmedLine.EndsWith("*/"))
                {
                    inCommentBlock = false;
                }
                continue;
            }

            // Match the regular expression in the non-comment line
            Match match = regex.Match(trimmedLine);
            if (match.Success && match.Groups.Count > 3)
            {
                className = match.Groups[3].Value;
                return true;
            }
        }

        return false;
    }

    private static bool IsJson(string text)
    {
        text = text.Trim();
        if ((text.StartsWith("{") && text.EndsWith("}")) || // For object
            (text.StartsWith("[") && text.EndsWith("]")))   // For array
        {
            try
            {
                var obj = JsonUtility.FromJson<object>(text);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        return false;
    }

    private static bool IsMarkdown(string text)
    {
        // Simple heuristic to identify Markdown content
        return text.Contains("#") || text.Contains("```") || text.Contains("*") || text.Contains("-");
    }

    private static void SaveMarkdown(string textContent)
    {
        // Prompt user for filename
        string fileName = EditorUtility.SaveFilePanel("Save Markdown", pathToSaveIn, "document", "md");

        if (!string.IsNullOrEmpty(fileName))
        {
            File.WriteAllText(fileName, textContent);
            AssetDatabase.Refresh();
            Debug.Log("Markdown file saved: " + fileName);
        }
    }

    private static bool IsXml(string text)
    {
        // Simple heuristic to identify XML content
        return text.TrimStart().StartsWith("<") && text.TrimEnd().EndsWith(">");
    }

    private static void SaveXml(string textContent)
    {
        // Prompt user for filename
        string fileName = EditorUtility.SaveFilePanel("Save XML", pathToSaveIn, "data", "xml");

        if (!string.IsNullOrEmpty(fileName))
        {
            File.WriteAllText(fileName, textContent);
            AssetDatabase.Refresh();
            Debug.Log("XML file saved: " + fileName);
        }
    }

    private static bool IsShaderCode(string text)
    {
        // Simple heuristic to identify shader code
        return text.Contains("Shader") || text.Contains("CGPROGRAM") || text.Contains("HLSLPROGRAM");
    }

    private static void SaveShaderCode(string textContent)
    {
        // Prompt user for filename
        string fileName = EditorUtility.SaveFilePanel("Save Shader", pathToSaveIn, "NewShader", "shader");

        if (!string.IsNullOrEmpty(fileName))
        {
            File.WriteAllText(fileName, textContent);
            AssetDatabase.Refresh();
            Debug.Log("Shader file saved: " + fileName);
        }
    }

    private static string GetActiveFolderPath()
    {
        // Use reflection to get the active folder path in Project window
        Type projectWindowUtilType = typeof(ProjectWindowUtil);
        MethodInfo getActiveFolderPath = projectWindowUtilType.GetMethod("GetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

        if (getActiveFolderPath != null)
        {
            object obj = getActiveFolderPath.Invoke(null, null);
            string pathToCurrentFolder = obj.ToString();

            // Remove "Assets" prefix from the path
            if (pathToCurrentFolder.StartsWith("Assets"))
            {
                pathToCurrentFolder = pathToCurrentFolder.Substring("Assets".Length);
            }

            // Trim any leading or trailing slashes
            pathToCurrentFolder = pathToCurrentFolder.Trim('/', '\\');

            return pathToCurrentFolder;
        }
        else
        {
            Debug.LogError("Failed to retrieve active folder path.");
            return null;
        }
    }


    private static void PasteImage()
    {
        string clipboardImagePath = Path.Combine(GetDocumentsPath(), "instant-clipboard/image.png");

        if (File.Exists(clipboardImagePath))
        {
            // Load the image from clipboard path
            byte[] bytes = File.ReadAllBytes(clipboardImagePath);

            // Prompt user for filename
            string fileName = EditorUtility.SaveFilePanel("Save Image", pathToSaveIn, "image", "png");

            if (!string.IsNullOrEmpty(fileName))
            {
                // Write image bytes to file
                File.WriteAllBytes(fileName, bytes);

                // Refresh Unity Editor to recognize the new asset
                AssetDatabase.Refresh();

                // Show a message
                Debug.Log("Image saved: " + fileName);
            }
        }
        else
        {
            Debug.LogError("No image found in instant-clipboard folder.");
        }
    }

    private static string GetDocumentsPath()
    {
        // Get the documents path based on the current OS
        return System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
    }

    // Register a hotkey for Ctrl+V in the project window
    [InitializeOnLoadMethod]
    private static void RegisterHotkey()
    {
        EditorApplication.projectWindowItemOnGUI += HandleProjectWindowItemOnGUI;
    }

    private static void HandleProjectWindowItemOnGUI(string guid, Rect selectionRect)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.control && e.keyCode == KeyCode.V && EditorGUIUtility.editingTextField == false)
        {
            Paste();
            e.Use();
        }
    }

    [Serializable]
    private class ClipboardInfo
    {
        public string lastAction;
        public string version;
    }
}
