#if UNITY_EDITOR
using System.Collections.Generic;
using Core.Learning.Models;
using Features.Activities.QuantityMatch;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
  public static class QuantityMatchConfigFactory
  {
    private const string AssetPath =
      "Assets/Features/Activities/QuantityMatch/ScriptableObjects/SO_QuantityMatchConfig_Easy.asset";

    [MenuItem("AR Learning/Create Quantity Match Easy Config")]
    public static void CreateEasyConfigAsset()
    {
      EnsureFolder("Assets/Features/Activities/QuantityMatch/ScriptableObjects");

      var config = ScriptableObject.CreateInstance<QuantityMatchConfig>();
      var so = new SerializedObject(config);

      so.FindProperty("activityId").stringValue = "QuantityMatch";
      so.FindProperty("displayName").stringValue = "Quantity Match (Easy)";
      so.FindProperty("description").stringValue = "Match the number to the correct group of objects.";
      so.FindProperty("numberOfRounds").intValue = 2;
      so.FindProperty("maxAttemptsPerQuestion").intValue = 3;
      so.FindProperty("maxHintsPerQuestion").intValue = 3;

      var questionsProp = so.FindProperty("questions");
      questionsProp.arraySize = 2;

      SetQuestion(questionsProp.GetArrayElementAtIndex(0), target: 3, groups: 3,
        counts: new[] { 2, 3, 4 }, correctIndex: 1);
      SetQuestion(questionsProp.GetArrayElementAtIndex(1), target: 5, groups: 3,
        counts: new[] { 4, 5, 6 }, correctIndex: 1);

      so.ApplyModifiedPropertiesWithoutUndo();

      AssetDatabase.CreateAsset(config, AssetPath);
      AssetDatabase.SaveAssets();
      Debug.Log($"[QuantityMatchConfigFactory] Created {AssetPath}");
    }

    private static void SetQuestion(SerializedProperty questionProp, int target, int groups,
      int[] counts, int correctIndex)
    {
      questionProp.FindPropertyRelative("TargetNumber").intValue = target;
      questionProp.FindPropertyRelative("NumberOfGroups").intValue = groups;
      questionProp.FindPropertyRelative("CorrectGroupIndex").intValue = correctIndex;

      var countsProp = questionProp.FindPropertyRelative("ObjectCountsPerGroup");
      countsProp.arraySize = counts.Length;
      for (int i = 0; i < counts.Length; i++)
      {
        countsProp.GetArrayElementAtIndex(i).intValue = counts[i];
      }
    }

    private static void EnsureFolder(string path)
    {
      if (!AssetDatabase.IsValidFolder(path))
      {
        System.IO.Directory.CreateDirectory(path);
        AssetDatabase.Refresh();
      }
    }
  }
}
#endif
