#if UNITY_EDITOR
using System.Reflection;
using Core.AR;
using Core.AR.ARSession;
using Core.AR.Interaction;
using Core.AR.Placement;
using Features.Activities.QuantityMatch;
using Features.Activities;
using Project.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Project.Editor
{
  public static class ARGameplaySceneMenu
  {
    private const string GameplayScenePath = "Assets/_Project/Scenes/SC_ARGameplay.unity";
    private const string ConfigAssetPath =
      "Assets/Features/Activities/QuantityMatch/ScriptableObjects/SO_QuantityMatchConfig_Easy.asset";
    private const string XrOriginPrefabPath =
      "Assets/Samples/XR Interaction Toolkit/3.3.0/AR Starter Assets/Prefabs/XR Origin (AR Rig).prefab";

    [MenuItem("AR Learning/Setup AR Gameplay Scene (Quantity Match)")]
    public static void SetupARGameplayScene()
    {
      var config = AssetDatabase.LoadAssetAtPath<QuantityMatchConfig>(ConfigAssetPath);
      if (config == null)
      {
        QuantityMatchConfigFactory.CreateEasyConfigAsset();
        config = AssetDatabase.LoadAssetAtPath<QuantityMatchConfig>(ConfigAssetPath);
      }

      var xrPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(XrOriginPrefabPath);
      if (xrPrefab == null || config == null)
      {
        Debug.LogError("[ARGameplaySceneMenu] Missing XR prefab or config.");
        return;
      }

      Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

      GameObject xrOrigin = PrefabUtility.InstantiatePrefab(xrPrefab) as GameObject;
      xrOrigin.name = "XR Origin (AR Rig)";
      EnsureSingleAudioListener(xrOrigin);

      UnityEngine.XR.ARFoundation.ARSession arSession = EnsureARSession();
      ARSessionService sessionService = xrOrigin.GetComponent<ARSessionService>();
      if (sessionService == null) sessionService = xrOrigin.AddComponent<ARSessionService>();
      AssignARSession(sessionService, arSession);

      if (xrOrigin.GetComponent<ARPlacementService>() == null) xrOrigin.AddComponent<ARPlacementService>();
      if (xrOrigin.GetComponent<ARInteractionService>() == null) xrOrigin.AddComponent<ARInteractionService>();

      new GameObject("ARServiceBootstrap").AddComponent<ARServiceBootstrap>();
      new GameObject("LearningSceneServices").AddComponent<LearningSceneServices>();

      var activityRoot = new GameObject("QuantityMatchActivity");
      var presenter = activityRoot.AddComponent<QuantityMatchPresenter>();
      var view = activityRoot.AddComponent<QuantityMatchView>();
      activityRoot.AddComponent<ActivityPrefabSetup>();
      activityRoot.AddComponent<QuantityMatchRuntimeUI>();
      var bootstrap = activityRoot.AddComponent<QuantityMatchActivityBootstrap>();

      var bootstrapSo = new SerializedObject(bootstrap);
      AssignObjectReference(bootstrapSo, "presenter", presenter);
      AssignObjectReference(bootstrapSo, "view", view);
      AssignObjectReference(bootstrapSo, "config", config);
      bootstrapSo.ApplyModifiedProperties();
      SetPrivateField(bootstrap, "presenter", presenter);
      SetPrivateField(bootstrap, "view", view);
      SetPrivateField(bootstrap, "config", config);
      EditorUtility.SetDirty(bootstrap);

      EditorSceneManager.MarkSceneDirty(scene);
      EditorSceneManager.SaveScene(scene, GameplayScenePath);
      ARTestSandboxMenu.AddSceneToBuildSettingsIfMissing(GameplayScenePath);
      Debug.Log($"[ARGameplaySceneMenu] Saved {GameplayScenePath}");
    }

    private static void AssignObjectReference(SerializedObject serializedObject, string fieldName, Object reference)
    {
      SerializedProperty property = serializedObject.FindProperty(fieldName);
      if (property == null)
      {
        Debug.LogError($"[ARGameplaySceneMenu] Could not find serialized field '{fieldName}'.");
        return;
      }

      property.objectReferenceValue = reference;
    }

    private static void SetPrivateField(object target, string fieldName, object value)
    {
      FieldInfo field = target.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
      if (field == null)
      {
        Debug.LogError($"[ARGameplaySceneMenu] Could not find field '{fieldName}'.");
        return;
      }

      field.SetValue(target, value);
    }

    private static void EnsureSingleAudioListener(GameObject preferredRoot)
    {
      AudioListener[] listeners = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
      bool keptPreferred = false;

      foreach (AudioListener listener in listeners)
      {
        bool isPreferred = preferredRoot != null && listener.transform.IsChildOf(preferredRoot.transform);
        if (isPreferred && !keptPreferred)
        {
          keptPreferred = true;
          continue;
        }

        Object.DestroyImmediate(listener);
      }
    }

    private static UnityEngine.XR.ARFoundation.ARSession EnsureARSession()
    {
      UnityEngine.XR.ARFoundation.ARSession arSession =
        Object.FindAnyObjectByType<UnityEngine.XR.ARFoundation.ARSession>();
      if (arSession != null)
      {
        return arSession;
      }

      var sessionGo = new GameObject("AR Session");
      return sessionGo.AddComponent<UnityEngine.XR.ARFoundation.ARSession>();
    }

    private static void AssignARSession(ARSessionService service, UnityEngine.XR.ARFoundation.ARSession arSession)
    {
      var serialized = new SerializedObject(service);
      serialized.FindProperty("arSession").objectReferenceValue = arSession;
      serialized.ApplyModifiedPropertiesWithoutUndo();
    }
  }
}
#endif
