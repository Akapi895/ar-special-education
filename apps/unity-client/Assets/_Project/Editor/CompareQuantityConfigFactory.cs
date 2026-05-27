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
            so.FindProperty("displayName").stringValue = "So s\u00e1nh s\u1ed1 l\u01b0\u1ee3ng (D\u1ec5)";
            so.FindProperty("description").stringValue = "Con so s\u00e1nh hai nh\u00f3m con v\u1eadt xem b\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n, \u00edt h\u01a1n hay b\u1eb1ng b\u00ean ph\u1ea3i.";
            so.FindProperty("numberOfRounds").intValue = 10;
            so.FindProperty("maxAttemptsPerQuestion").intValue = 3;
            so.FindProperty("maxHintsPerQuestion").intValue = 3;
            so.FindProperty("groupSpacing").floatValue = 3.0f;
            so.FindProperty("correctMoreFeedback").stringValue = "\u0110\u00fang r\u1ed3i! Nh\u00f3m b\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n!";
            so.FindProperty("correctFewerFeedback").stringValue = "\u0110\u00fang r\u1ed3i! Nh\u00f3m b\u00ean tr\u00e1i \u00edt h\u01a1n!";
            so.FindProperty("correctEqualFeedback").stringValue = "\u0110\u00fang r\u1ed3i! Hai nh\u00f3m b\u1eb1ng nhau!";
            so.FindProperty("incorrectMoreFeedback").stringValue = "Ch\u01b0a ph\u1ea3i nhi\u1ec1u h\u01a1n. Con h\u00e3y \u0111\u1ebfm l\u1ea1i nh\u00e9!";
            so.FindProperty("incorrectFewerFeedback").stringValue = "Ch\u01b0a ph\u1ea3i \u00edt h\u01a1n. Con h\u00e3y \u0111\u1ebfm l\u1ea1i nh\u00e9!";
            so.FindProperty("incorrectEqualFeedback").stringValue = "Ch\u01b0a b\u1eb1ng nhau. Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m nh\u00e9!";
            so.FindProperty("genericIncorrectFeedback").stringValue = "Ch\u01b0a \u0111\u00fang r\u1ed3i. Con th\u1eed l\u1ea1i nh\u00e9!";
            so.FindProperty("failedFeedback").stringValue = "Con \u0111\u00e3 c\u1ed1 g\u1eafng r\u1ea5t t\u1ed1t. M\u00ecnh th\u1eed c\u00e2u kh\u00e1c nh\u00e9.";

            // Set up standard hints
            SetHint(so.FindProperty("hintLevel1"), "cq_hint1", "Con h\u00e3y \u0111\u1ebfm t\u1eebng nh\u00f3m th\u1eadt ch\u1eadm nh\u00e9.", 1);
            SetHint(so.FindProperty("hintLevel2"), "cq_hint2", "Nh\u00f3m b\u00ean tr\u00e1i c\u00f3 X con. B\u00e2y gi\u1edd con \u0111\u1ebfm nh\u00f3m b\u00ean ph\u1ea3i.", 2);
            SetHint(so.FindProperty("hintLevel3"), "cq_hint3", "So s\u00e1nh X v\u00e0 Y: s\u1ed1 n\u00e0o l\u1edbn h\u01a1n?", 3);

            // Set up equality-specific hints
            SetHint(so.FindProperty("equalityHintLevel1"), "cq_eq_hint1", "Con \u0111\u1ebfm c\u1ea3 hai nh\u00f3m: hai nh\u00f3m c\u00f3 b\u1eb1ng nhau kh\u00f4ng?", 1);
            SetHint(so.FindProperty("equalityHintLevel2"), "cq_eq_hint2", "Nh\u00f3m b\u00ean tr\u00e1i c\u00f3 X con. Nh\u00f3m b\u00ean ph\u1ea3i c\u0169ng c\u00f3 X con.", 2);
            SetHint(so.FindProperty("equalityHintLevel3"), "cq_eq_hint3", "X v\u00e0 X l\u00e0 c\u00f9ng m\u1ed9t s\u1ed1, n\u00ean hai nh\u00f3m b\u1eb1ng nhau.", 3);

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
