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

      var config = AssetDatabase.LoadAssetAtPath<QuantityMatchConfig>(AssetPath);
      bool created = false;
      if (config == null)
      {
        config = ScriptableObject.CreateInstance<QuantityMatchConfig>();
        created = true;
      }

      var so = new SerializedObject(config);

      so.FindProperty("activityId").stringValue = "QuantityMatch";
      so.FindProperty("displayName").stringValue = "Quantity Match (Easy)";
      so.FindProperty("description").stringValue = "Match the number to the correct group of objects.";
      so.FindProperty("numberOfRounds").intValue = 10;
      so.FindProperty("maxAttemptsPerQuestion").intValue = 3;
      so.FindProperty("maxHintsPerQuestion").intValue = 3;
      so.FindProperty("defaultObjectSpacing").floatValue = 0.68f;
      so.FindProperty("defaultGroupSpacing").floatValue = 2.1f;
      so.FindProperty("groupArrangement").enumValueIndex = (int)GroupArrangementPattern.Horizontal;

      var questionsProp = so.FindProperty("questions");
      questionsProp.arraySize = 10;

      SetQuestion(questionsProp.GetArrayElementAtIndex(0), target: 2, groups: 3,
        counts: new[] { 1, 2, 3 }, correctIndex: 1);
      SetQuestion(questionsProp.GetArrayElementAtIndex(1), target: 4, groups: 3,
        counts: new[] { 4, 2, 5 }, correctIndex: 0);
      SetQuestion(questionsProp.GetArrayElementAtIndex(2), target: 6, groups: 3,
        counts: new[] { 5, 6, 7 }, correctIndex: 1);
      SetQuestion(questionsProp.GetArrayElementAtIndex(3), target: 3, groups: 3,
        counts: new[] { 1, 3, 5 }, correctIndex: 1);
      SetQuestion(questionsProp.GetArrayElementAtIndex(4), target: 5, groups: 3,
        counts: new[] { 4, 6, 5 }, correctIndex: 2);
      SetQuestion(questionsProp.GetArrayElementAtIndex(5), target: 7, groups: 1,
        counts: new[] { 7 }, correctIndex: 0);
      SetQuestion(questionsProp.GetArrayElementAtIndex(6), target: 8, groups: 1,
        counts: new[] { 8 }, correctIndex: 0);
      SetQuestion(questionsProp.GetArrayElementAtIndex(7), target: 9, groups: 1,
        counts: new[] { 9 }, correctIndex: 0);
      SetQuestion(questionsProp.GetArrayElementAtIndex(8), target: 10, groups: 1,
        counts: new[] { 10 }, correctIndex: 0);
      SetQuestion(questionsProp.GetArrayElementAtIndex(9), target: 6, groups: 1,
        counts: new[] { 6 }, correctIndex: 0);

      SetHint(so.FindProperty("hintLevel1"), "qm_hint1", "Look carefully at the groups.", 1);
      SetHint(so.FindProperty("hintLevel2"), "qm_hint2", "The number shown is X, count each group.", 2);
      SetHint(so.FindProperty("hintLevel3"), "qm_hint3", "One group has exactly X objects.", 3);

      so.ApplyModifiedPropertiesWithoutUndo();

      if (created)
      {
        AssetDatabase.CreateAsset(config, AssetPath);
      }
      else
      {
        EditorUtility.SetDirty(config);
      }

      AssetDatabase.SaveAssets();
      Debug.Log($"[QuantityMatchConfigFactory] Saved {AssetPath}");
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

    private static void SetHint(SerializedProperty hintProp, string hintId, string hintText, int level)
    {
      hintProp.FindPropertyRelative("HintId").stringValue = hintId;
      hintProp.FindPropertyRelative("HintText").stringValue = hintText;
      hintProp.FindPropertyRelative("Level").intValue = level;
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
