using UnityEditor;
using UnityEditor.SceneManagement;

public static class OnPlayState
{
    [InitializeOnLoadMethod]
    static void Initialize()
    {
        EditorApplication.playModeStateChanged  -= OnChangedPlayMode;
        EditorApplication.playModeStateChanged  += OnChangedPlayMode;
    }

    static void OnChangedPlayMode(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // 再生前にシーンセーブ
            EditorSceneManager.SaveOpenScenes();
        }
    }
}
