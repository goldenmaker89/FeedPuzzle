using UnityEditor;
using UnityEditor.SceneManagement;

public class SaveCurrentScene
{
    public static string Execute()
    {
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        return "Scene saved.";
    }
}
