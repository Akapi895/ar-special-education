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
      so.FindProperty("displayName").stringValue = "Gh\u00e9p s\u1ed1 v\u1edbi s\u1ed1 l\u01b0\u1ee3ng (D\u1ec5)";
      so.FindProperty("description").stringValue = "Con ch\u1ecdn nh\u00f3m con v\u1eadt c\u00f3 \u0111\u00fang s\u1ed1 l\u01b0\u1ee3ng \u0111\u01b0\u1ee3c y\u00eau c\u1ea7u.";
      so.FindProperty("numberOfRounds").intValue = 10;
      so.FindProperty("maxAttemptsPerQuestion").intValue = 3;
      so.FindProperty("maxHintsPerQuestion").intValue = 3;
      so.FindProperty("correctFeedback").stringValue = "Gi\u1ecfi l\u1eafm! Con \u0111\u00e3 ch\u1ecdn \u0111\u00fang nh\u00f3m!";
      so.FindProperty("incorrectFeedback").stringValue = "Ch\u01b0a \u0111\u00fang r\u1ed3i. Con h\u00e3y th\u1eed \u0111\u1ebfm l\u1ea1i nh\u00e9!";
      so.FindProperty("failedFeedback").stringValue = "Con \u0111\u00e3 c\u1ed1 g\u1eafng r\u1ea5t t\u1ed1t. M\u00ecnh th\u1eed c\u00e2u kh\u00e1c nh\u00e9.";
      so.FindProperty("defaultObjectSpacing").floatValue = 0.68f;
      so.FindProperty("defaultGroupSpacing").floatValue = 1.6f;
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

      SetHint(so.FindProperty("hintLevel1"), "qm_hint1", "Con nh\u00ecn k\u1ef9 t\u1eebng nh\u00f3m con v\u1eadt nh\u00e9.", 1);
      SetHint(so.FindProperty("hintLevel2"), "qm_hint2", "S\u1ed1 c\u1ea7n t\u00ecm l\u00e0 X. Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m.", 2);
      SetHint(so.FindProperty("hintLevel3"), "qm_hint3", "C\u00f3 m\u1ed9t nh\u00f3m c\u00f3 \u0111\u00fang X con v\u1eadt.", 3);

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
