using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Watermelon;

namespace Watermelon.JellyMerge.Editor
{
    [InitializeOnLoad]
    public static class SceneSwitcherTool
    {
        private const string SCENES_PATH = "Assets/ROOT/Scenes/";
        private const string INIT_SCENE = "Init";

        static SceneSwitcherTool()
        {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        // Khi bấm Play, nếu scene hiện tại không phải Init thì force về Init
        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode)
                return;

            var currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (currentScene.name == INIT_SCENE)
                return;

            SceneAsset initScene = AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SCENES_PATH}{INIT_SCENE}.unity");
            if (initScene == null)
            {
                Debug.LogError("[SceneSwitcher] Init scene not found — playing from current scene.");
                return;
            }

            // Ghi lại build index để GameLoading biết load scene nào sau Init
            GameLoading.LoadingSceneBuildIndex = currentScene.buildIndex;
            EditorSceneManager.playModeStartScene = initScene;
        }

        [MenuItem("Scenes/Init Scene _F1", priority = 0)]
        public static void OpenInitScene() => OpenScene("Init");

        [MenuItem("Scenes/Game Scene _F2", priority = 1)]
        public static void OpenGameScene() => OpenScene("Game");

        private static void OpenScene(string sceneName)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[SceneSwitcher] Stop Play Mode before switching scenes.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            string path = $"{SCENES_PATH}{sceneName}.unity";
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) == null)
            {
                Debug.LogError($"[SceneSwitcher] Scene not found at: {path}");
                return;
            }

            EditorSceneManager.OpenScene(path);
        }

        [MenuItem("Scenes/▶ Play from Init _F5", priority = 20)]
        public static void PlayFromInit()
        {
            if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            EditorSceneManager.playModeStartScene = null;
            GameLoading.LoadingSceneBuildIndex = -1;
            EditorSceneManager.OpenScene($"{SCENES_PATH}Init.unity");
            EditorApplication.isPlaying = true;
        }

        [MenuItem("Scenes/▶ Play from Game _F6", priority = 21)]
        public static void PlayFromGame()
        {
            if (EditorApplication.isPlaying) { EditorApplication.isPlaying = false; return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            // Mở Game scene nhưng Play sẽ bắt đầu từ Init, sau đó skip thẳng vào Game
            SceneAsset initScene = AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SCENES_PATH}Init.unity");
            SceneAsset gameScene = AssetDatabase.LoadAssetAtPath<SceneAsset>($"{SCENES_PATH}Game.unity");

            if (initScene == null || gameScene == null)
            {
                Debug.LogError("[SceneSwitcher] Scene not found.");
                return;
            }

            EditorSceneManager.OpenScene($"{SCENES_PATH}Game.unity");

            int gameIndex = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath($"{SCENES_PATH}Game.unity");
            GameLoading.LoadingSceneBuildIndex = gameIndex;
            EditorSceneManager.playModeStartScene = initScene;

            EditorApplication.isPlaying = true;
        }
    }
}
