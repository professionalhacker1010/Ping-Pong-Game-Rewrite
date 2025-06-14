using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteAlways]
public class GameStartup : MonoBehaviour
{
    [SerializeField]
    [InspectorName("Scenes Loaded On Startup")]
    [Tooltip("Scenes to have loaded when this scene is first opened.")]
    private List<SceneReference> _scenesToLoad = new List<SceneReference>();

    private bool hasStartupHappened = false;

#if UNITY_EDITOR
    public void Editor_LoadSceneOnStartup()
    {
        foreach (var scene in _scenesToLoad)
        {
            if (!SceneManager.GetSceneByPath(scene.ScenePath).isLoaded)
            {
                EditorSceneManager.OpenScene(scene.ScenePath, OpenSceneMode.Additive);
            }
        }
    }

    public void Editor_CleanScenesToLoad()
    {
        CleanScenesToLoad();
    }
#endif

    private void LoadScenesOnStartup()
    {
        foreach (var scene in _scenesToLoad)
        {
            if (!SceneManager.GetSceneByPath(scene.ScenePath).isLoaded)
            {
                SceneManager.LoadScene(scene.ScenePath, LoadSceneMode.Additive);
            }
        }
    }

    private void CleanScenesToLoad(Scene scene = default, string path = "")
    {
        _scenesToLoad.RemoveAll((scene) =>
        {
            return scene.ScenePath.Length == 0;
        });

        _scenesToLoad = _scenesToLoad.Distinct().ToList();

        _scenesToLoad.Sort((a, b) =>
        {
            return a.ScenePath.CompareTo(b.ScenePath);
        });
    }

    private void Start()
    {
        CleanScenesToLoad();

        if (!Application.isPlaying)
        {
#if UNITY_EDITOR
            EditorSceneManager.sceneSaving += CleanScenesToLoad;

            if (!hasStartupHappened)
            {
                Editor_LoadSceneOnStartup();
            }
            hasStartupHappened = true;
#endif
        }
        else
        {
            if (!hasStartupHappened)
            {
                LoadScenesOnStartup();
            }
            hasStartupHappened = true;
        }
    }
}