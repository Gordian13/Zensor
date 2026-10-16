using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scenes
{
    /**
     * Script to load all the Scenes together into the main Scene
     *
     * It Searches the Scenes Folder and takes every Scene in it except for the folders in ExcludeFolders
     */
    [InitializeOnLoad]
    public static class AutoLoadScenes
    {
        /** Name of the main Scene, the other Scenes get loaded into it */
        private const string MainSceneName = "main";
        /** Folder that gets searched for Scenes */
        private const string ScenesFolder = "Assets/Scenes";
        /** Folders with these names get skipped when searching for Scenes */
        private static readonly List<string> ExcludeFolders = new List<string> { "test" };

        /**
         * Runs when the Editor loads (because of InitializeOnLoad).
         *
         * Subscribes OnSceneOpened so it gets called every time a Scene is opened.
         */
        static AutoLoadScenes()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        /**
         * Gets called when a Scene is opened in the Editor.
         *
         * If the main Scene is opened on its own, all other Scenes from the Scenes Folder get loaded into it.
         *
         * @param openedScene The Scene that was opened.
         * @param mode How the Scene was opened, only Single loads the other Scenes.
         */
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
         * 
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
         * 
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
         * 
         * @param filePath The path of the file to check.
         * @return True if the file is a scene, false otherwise.
         */
        private static bool IsScene(string filePath)
        {
            return filePath.EndsWith(".unity");
        }

        /**
         * Checks whether a folder should be included in the scene scan.
         * 
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