using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes
{
    [InitializeOnLoad]
    public static class AutoLoadScenes
    {
        private const string MainSceneName = "main";
        private const string ScenesFolder = "Assets/Scenes";
        private static readonly List<string> ExcludeFolders = new List<string> { "test" };

        static AutoLoadScenes()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene openedScene, OpenSceneMode mode)
        {
            if (openedScene.name.ContainsInsensitive(MainSceneName) && mode == OpenSceneMode.Single)
            {
                List<string> subScenePaths = ScanFolderForFiles(ScenesFolder);
                foreach (string scenePath in subScenePaths)
                {
                    AddSceneToMainScene(scenePath);
                }
            }
        }

        /**
         * Loads a scene additively into the currently open main scene.
         * @param scenePath The path of the scene to load.
         */
        private static void AddSceneToMainScene(string scenePath)
        {
            Debug.Log($"Trying to add scene to main scene: {scenePath}");
            try
            {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Additive);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to open {scenePath}: {e}");
            }
        }

        /**
         * Recursively scans a folder and its subfolders for scene files.
         * @param folderPath The path of the folder to scan.
         * @return A list of paths to all scene files found.
         */
        private static List<string> ScanFolderForFiles(string folderPath)
        {
            List<string> foundScenes = new List<string>();

            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"Path doesn't exist: {folderPath}");
                return foundScenes;
            }

            Debug.Log($"Scanning folder: {folderPath}");
            string[] filePaths = Directory.GetFiles(folderPath);

            foreach (string filePath in filePaths)
            {
                if (IsScene(filePath) && Path.GetFileNameWithoutExtension(filePath) != MainSceneName)
                {
                    foundScenes.Add(filePath);
                    Debug.Log($"Found file: {filePath}");
                }
            }

            string[] subFolders = Directory.GetDirectories(folderPath);
            foreach (string subFolder in subFolders)
            {
                if (IsSceneFolder(subFolder))
                {
                    foundScenes.AddRange(ScanFolderForFiles(subFolder));
                }
            }

            return foundScenes;
        }

        /**
         * Checks whether a file is a Unity scene.
         * @param filePath The path of the file to check.
         * @return True if the file is a scene, false otherwise.
         */
        private static bool IsScene(string filePath)
        {
            return filePath.EndsWith(".unity");
        }

        /**
         * Checks whether a folder should be included in the scene scan.
         * @param folderPath The path of the folder to check.
         * @return True if the folder should be scanned, false if it is excluded.
         */
        private static bool IsSceneFolder(string folderPath)
        {
            foreach (var exclude in ExcludeFolders)
            {
                if (folderPath.ContainsInsensitive(exclude))
                {
                    return false;
                }
            }

            return true;
        }
    }
}