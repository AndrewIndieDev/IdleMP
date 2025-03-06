using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace BayatGames.Utilities.Editor
{

    /// <summary>
    /// Scene switcher window, an editor window for switching between scenes.
    /// </summary>
    public class SceneSwitcherWindow : EditorWindow
    {

        public enum ScenesSource
        {
            Assets,
            BuildSettings,
            BuildAndTestScenes
        }

        protected Vector2 scrollPosition;
        protected ScenesSource scenesSource = ScenesSource.Assets;
        protected OpenSceneMode openSceneMode = OpenSceneMode.Single;
        protected int selectedTab = 0;
        protected string[] tabs = new string[] {
            "Scenes",
            "Settings"
        };

        List<EditorBuildSettingsScene> buildScenes;
        string[] guids;

        [MenuItem("Tools/Scene Switcher")]
        public static void Init()
        {
            var window = EditorWindow.GetWindow<SceneSwitcherWindow>("Scene Switcher");
            window.minSize = new Vector2(250f, 200f);
            window.Show();
        }

        protected virtual void OnEnable()
        {
            this.scenesSource = (ScenesSource)EditorPrefs.GetInt("SceneSwitcher.scenesSource", (int)ScenesSource.Assets);
            this.openSceneMode = (OpenSceneMode)EditorPrefs.GetInt(
                "SceneSwitcher.openSceneMode",
                (int)OpenSceneMode.Single);
            UpdateSceneList();
        }

        protected virtual void OnDisable()
        {
            EditorPrefs.SetInt("SceneSwitcher.scenesSource", (int)this.scenesSource);
            EditorPrefs.SetInt("SceneSwitcher.openSceneMode", (int)this.openSceneMode);
        }

        protected virtual void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            this.selectedTab = GUILayout.Toolbar(this.selectedTab, this.tabs, EditorStyles.toolbarButton);
            EditorGUILayout.EndHorizontal();
            this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
            EditorGUILayout.BeginVertical();
            switch (this.selectedTab)
            {
                case 0:
                    ScenesTabGUI();
                    break;
                case 1:
                    SettingsTabGUI();
                    break;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndScrollView();
        }

        protected virtual void SettingsTabGUI()
        {
            EditorGUI.BeginChangeCheck();
            this.scenesSource = (ScenesSource)EditorGUILayout.EnumPopup("Scenes Source", this.scenesSource);
            if (EditorGUI.EndChangeCheck())
                UpdateSceneList();
            this.openSceneMode = (OpenSceneMode)EditorGUILayout.EnumPopup("Open Scene Mode", this.openSceneMode);
        }

        protected virtual void ScenesTabGUI()
        {
            if (guids.Length == 0)
            {
                GUILayout.Label("No Scenes Found", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Label("Create New Scenes", EditorStyles.centeredGreyMiniLabel);
                GUILayout.Label("And Switch Between them here", EditorStyles.centeredGreyMiniLabel);
            }
            List<System.Tuple<string, string>> primaryLevels = new List<System.Tuple<string, string>>();
            List<System.Tuple<string, string>> secondaryLevels = new List<System.Tuple<string, string>>();
            List<System.Tuple<string, string>> tertiaryLevels = new List<System.Tuple<string, string>>();
            List<System.Tuple<string, string>> quartieryLevels = new List<System.Tuple<string, string>>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(path);
                if (sceneAsset == null)
                {
                    UpdateSceneList();
                    break;
                }
                EditorBuildSettingsScene buildScene = buildScenes.Find((editorBuildScene) =>
                {
                    return editorBuildScene.path == path;
                });
                Scene scene = SceneManager.GetSceneByPath(path);
                bool isOpen = scene.IsValid() && scene.isLoaded;
                EditorGUI.BeginDisabledGroup(isOpen);

                switch (scenesSource)
                {
                    case ScenesSource.Assets:
                    {
                        primaryLevels.Add(new System.Tuple<string, string>(sceneAsset.name, path));
                        break;
                    }
                    case ScenesSource.BuildSettings:
                    {
                        if (buildScene != null)
                        {
                            primaryLevels.Add(new System.Tuple<string, string>(sceneAsset.name, path));
                        }
                        break;
                    }
                    case ScenesSource.BuildAndTestScenes:
                    {
                        if (buildScene != null)
                        {
                            if (path.Contains("/Maps/"))
                                tertiaryLevels.Add(new System.Tuple<string, string>(sceneAsset.name, path));
                            else if (path.Contains("/Menu/"))
                                quartieryLevels.Add(new System.Tuple<string, string>(sceneAsset.name, path));
                            else
                                primaryLevels.Add(new System.Tuple<string, string>(sceneAsset.name, path));
                            }
                        else
                        {
                            secondaryLevels.Add(new System.Tuple<string, string>(sceneAsset.name, path));
                        }
                        break;
                    }
                }
                EditorGUI.EndDisabledGroup();
            }
            Color c = GUI.backgroundColor;
            if (scenesSource == ScenesSource.BuildAndTestScenes)
            {
                GUILayout.BeginHorizontal();
                GUILayout.BeginVertical(GUILayout.Width(Screen.width / 2.0f));
            }
            GUILayout.Label("Scenes");
            foreach (var tuple in primaryLevels)
            {
                GUI.backgroundColor = new Color(0.5f, 1.0f, 0.5f);
                if (GUILayout.Button(tuple.Item1))
                    Open(tuple.Item2);
            }
            if (quartieryLevels.Count > 0)
            {
                GUILayout.Space(5);
                switch (scenesSource)
                {
                    case ScenesSource.BuildAndTestScenes:
                        {
                            GUILayout.Label("Menu");
                            GUI.backgroundColor = new Color(1.0f, 0.5f, 1.0f);
                            break;
                        }
                }
                foreach (var tuple in quartieryLevels)
                {
                    if (GUILayout.Button(tuple.Item1))
                        Open(tuple.Item2);
                }
            }
            if (scenesSource == ScenesSource.BuildAndTestScenes)
            {
                GUILayout.EndVertical();
                GUILayout.BeginVertical();
            }
            if (tertiaryLevels.Count > 0)
            {
                switch (scenesSource)
                {
                    case ScenesSource.BuildAndTestScenes:
                        {
                            GUILayout.Label("Maps");
                            GUI.backgroundColor = new Color(0.5f, 0.5f, 1.0f);
                            break;
                        }
                }
                foreach (var tuple in tertiaryLevels)
                {
                    if (GUILayout.Button(tuple.Item1))
                        Open(tuple.Item2);
                }
            }
            if (secondaryLevels.Count > 0)
            {
                GUILayout.Space(5);
                switch (scenesSource)
                {
                    case ScenesSource.BuildAndTestScenes:
                        {
                            GUILayout.Label("Testing scenes");
                            GUI.backgroundColor = new Color(1.0f, 1.0f, 0.5f);
                            break;
                        }
                }
                foreach (var tuple in secondaryLevels)
                {
                    if (GUILayout.Button(tuple.Item1))
                        Open(tuple.Item2);
                }
            }
            GUI.backgroundColor = c;
            if (scenesSource == ScenesSource.BuildAndTestScenes)
            {
                GUILayout.EndVertical();
                GUILayout.EndHorizontal();
            }
            GUI.backgroundColor = c;
            GUILayout.Space(5);
            GUILayout.Label("Options");
            GUI.backgroundColor = new Color(0.5f, 1.0f, 1.0f);
            if (GUILayout.Button("Refresh List"))
            {
                UpdateSceneList();
            }
            GUI.backgroundColor = c;
            GUI.backgroundColor = new Color(0.5f, 1.0f, 0.5f);
            if (GUILayout.Button("Create New Scene"))
            {
                Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
                EditorSceneManager.SaveScene(newScene);
            }
            GUI.backgroundColor = c;
        }

        public virtual void Open(string path)
        {
            if (EditorSceneManager.EnsureUntitledSceneHasBeenSaved("You don't have saved the Untitled Scene, Do you want to leave?"))
            {
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(path, this.openSceneMode);
            }
        }

        void UpdateSceneList()
        {
            buildScenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            if (scenesSource == ScenesSource.BuildAndTestScenes)
                guids = AssetDatabase.FindAssets("t:Scene", new[] { "Assets/Scenes" });
            else
                guids = AssetDatabase.FindAssets("t:Scene");
        }
    }

}