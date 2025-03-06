using UnityEditor;
using UnityEditor.SceneManagement;

public class EditorPlay
{
    [MenuItem("Edit/Play-Stop, But From Prelaunch Scene %0")]
    public static void PlayFromPrelaunchScene()
    {
        if (EditorApplication.isPlaying == true)
        {
            EditorApplication.isPlaying = false;
            return;
        }

        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene("Assets/_game/Scenes/EntryScene.unity");
        EditorApplication.isPlaying = true;
    }
}
