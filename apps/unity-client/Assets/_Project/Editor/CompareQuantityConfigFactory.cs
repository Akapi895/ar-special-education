#if UNITY_EDITOR
using Core.Learning.Models;
using Features.Activities.CompareQuantity;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
    public static class CompareQuantityConfigFactory
    {
        private const string AssetPath =
            "Assets/Features/Activities/CompareQuantity/ScriptableObjects/SO_CompareQuantityConfig_Easy.asset";

        [MenuItem("AR Learning/Create Compare Quantity Easy Config")]
        public static void CreateEasyConfigAsset()
        {
            EnsureFolder("Assets/Features/Activities/CompareQuantity/ScriptableObjects");

            var config = ScriptableObject.CreateInstance<CompareQuantityConfig>();
            var so = new SerializedObject(config);

            so.FindProperty("activityId").stringValue = "CompareQuantity";
            so.FindProperty("displayName").stringValue = "Compare Quantity (Easy)";
            so.FindProperty("description").stringValue = "Compare two groups of objects to find which has more, fewer, or if they are equal.";
            so.FindProperty("numberOfRounds").intValue = 3;
            so.FindProperty("maxAttemptsPerQuestion").intValue = 3;
            so.FindProperty("maxHintsPerQuestion").intValue = 3;

            // Set up standard hints
            var hint1Prop = so.FindProperty("hintLevel1");
            hint1Prop.FindPropertyRelative("hintId").stringValue = "cq_hint1";
            hint1Prop.FindPropertyRelative("hintText").stringValue = "Count each group carefully.";
            hint1Prop.FindPropertyRelative("level").intValue = 1;

            var hint2Prop = so.FindProperty("hintLevel2");
            hint2Prop.FindPropertyRelative("hintId").stringValue = "cq_hint2";
            hint2Prop.FindPropertyRelative("hintText").stringValue = "The left group has X. Now count the right group.";
            hint2Prop.FindPropertyRelative("level").intValue = 2;

            var hint3Prop = so.FindProperty("hintLevel3");
            hint3Prop.FindPropertyRelative("hintId").stringValue = "cq_hint3";
            hint3Prop.FindPropertyRelative("hintText").stringValue = "Compare X and Y — which is bigger?";
            hint3Prop.FindPropertyRelative("level").intValue = 3;

            // Set up equality-specific hints
            var eqHint1Prop = so.FindProperty("equalityHintLevel1");
            eqHint1Prop.FindPropertyRelative("hintId").stringValue = "cq_eq_hint1";
            eqHint1Prop.FindPropertyRelative("hintText").stringValue = "Count both groups — are they the same?";
            eqHint1Prop.FindPropertyRelative("level").intValue = 1;

            var eqHint2Prop = so.FindProperty("equalityHintLevel2");
            eqHint2Prop.FindPropertyRelative("hintId").stringValue = "cq_eq_hint2";
            eqHint2Prop.FindPropertyRelative("hintText").stringValue = "The left group has X. The right group also has X.";
            eqHint2Prop.FindPropertyRelative("level").intValue = 2;

            var eqHint3Prop = so.FindProperty("equalityHintLevel3");
            eqHint3Prop.FindPropertyRelative("hintId").stringValue = "cq_eq_hint3";
            eqHint3Prop.FindPropertyRelative("hintText").stringValue = "X and X are the same number — they're EQUAL!";
            eqHint3Prop.FindPropertyRelative("level").intValue = 3;

            var questionsProp = so.FindProperty("questions");
            questionsProp.arraySize = 3;

            // Question 1: Left has 3, Right has 5 (Fewer)
            SetQuestion(questionsProp.GetArrayElementAtIndex(0),
                leftCount: 3,
                rightCount: 5,
                correctAnswer: (int)ComparisonAnswer.Fewer);

            // Question 2: Left has 6, Right has 4 (More)
            SetQuestion(questionsProp.GetArrayElementAtIndex(1),
                leftCount: 6,
                rightCount: 4,
                correctAnswer: (int)ComparisonAnswer.More);

            // Question 3: Left has 4, Right has 4 (Equal)
            SetQuestion(questionsProp.GetArrayElementAtIndex(2),
                leftCount: 4,
                rightCount: 4,
                correctAnswer: (int)ComparisonAnswer.Equal);

            so.ApplyModifiedPropertiesWithoutUndo();

            AssetDatabase.CreateAsset(config, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CompareQuantityConfigFactory] Created {AssetPath}");
        }

        private static void SetQuestion(SerializedProperty questionProp, int leftCount, int rightCount, int correctAnswer)
        {
            questionProp.FindPropertyRelative("leftGroupCount").intValue = leftCount;
            questionProp.FindPropertyRelative("rightGroupCount").intValue = rightCount;
            questionProp.FindPropertyRelative("correctAnswer").intValue = correctAnswer;
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
