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

            var config = AssetDatabase.LoadAssetAtPath<CompareQuantityConfig>(AssetPath);
            bool created = false;
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<CompareQuantityConfig>();
                created = true;
            }

            var so = new SerializedObject(config);

            so.FindProperty("activityId").stringValue = "CompareQuantity";
            so.FindProperty("displayName").stringValue = "Compare Quantity (Easy)";
            so.FindProperty("description").stringValue = "Compare two groups of objects to find which has more, fewer, or if they are equal.";
            so.FindProperty("numberOfRounds").intValue = 10;
            so.FindProperty("maxAttemptsPerQuestion").intValue = 3;
            so.FindProperty("maxHintsPerQuestion").intValue = 3;
            so.FindProperty("groupSpacing").floatValue = 3.0f;

            // Set up standard hints
            SetHint(so.FindProperty("hintLevel1"), "cq_hint1", "Count each group carefully.", 1);
            SetHint(so.FindProperty("hintLevel2"), "cq_hint2", "The left group has X. Now count the right group.", 2);
            SetHint(so.FindProperty("hintLevel3"), "cq_hint3", "Compare X and Y - which is bigger?", 3);

            // Set up equality-specific hints
            SetHint(so.FindProperty("equalityHintLevel1"), "cq_eq_hint1", "Count both groups - are they the same?", 1);
            SetHint(so.FindProperty("equalityHintLevel2"), "cq_eq_hint2", "The left group has X. The right group also has X.", 2);
            SetHint(so.FindProperty("equalityHintLevel3"), "cq_eq_hint3", "X and X are the same number - they're EQUAL!", 3);

            var questionsProp = so.FindProperty("questions");
            questionsProp.arraySize = 10;

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

            SetQuestion(questionsProp.GetArrayElementAtIndex(3),
                leftCount: 2,
                rightCount: 7,
                correctAnswer: (int)ComparisonAnswer.Fewer);

            SetQuestion(questionsProp.GetArrayElementAtIndex(4),
                leftCount: 8,
                rightCount: 3,
                correctAnswer: (int)ComparisonAnswer.More);

            SetQuestion(questionsProp.GetArrayElementAtIndex(5),
                leftCount: 5,
                rightCount: 5,
                correctAnswer: (int)ComparisonAnswer.Equal);

            SetQuestion(questionsProp.GetArrayElementAtIndex(6),
                leftCount: 9,
                rightCount: 6,
                correctAnswer: (int)ComparisonAnswer.More);

            SetQuestion(questionsProp.GetArrayElementAtIndex(7),
                leftCount: 4,
                rightCount: 8,
                correctAnswer: (int)ComparisonAnswer.Fewer);

            SetQuestion(questionsProp.GetArrayElementAtIndex(8),
                leftCount: 7,
                rightCount: 7,
                correctAnswer: (int)ComparisonAnswer.Equal);

            SetQuestion(questionsProp.GetArrayElementAtIndex(9),
                leftCount: 10,
                rightCount: 6,
                correctAnswer: (int)ComparisonAnswer.More);

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
            Debug.Log($"[CompareQuantityConfigFactory] Saved {AssetPath}");
        }

        private static void SetQuestion(SerializedProperty questionProp, int leftCount, int rightCount, int correctAnswer)
        {
            questionProp.FindPropertyRelative("LeftGroupCount").intValue = leftCount;
            questionProp.FindPropertyRelative("RightGroupCount").intValue = rightCount;
            questionProp.FindPropertyRelative("CorrectAnswer").intValue = correctAnswer;
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
