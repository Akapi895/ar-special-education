#if UNITY_EDITOR
using Core.Learning.Models;
using Features.Activities.NumberLineJump;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
    public static class NumberLineJumpConfigFactory
    {
        private const string AssetPath =
            "Assets/Features/Activities/NumberLineJump/ScriptableObjects/SO_NumberLineJumpConfig_Easy.asset";

        [MenuItem("AR Learning/Create Number Line Jump Easy Config")]
        public static void CreateEasyConfigAsset()
        {
            EnsureFolder("Assets/Features/Activities/NumberLineJump/ScriptableObjects");

            var config = AssetDatabase.LoadAssetAtPath<NumberLineJumpConfig>(AssetPath);
            bool created = false;
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<NumberLineJumpConfig>();
                created = true;
            }

            var so = new SerializedObject(config);

            so.FindProperty("activityId").stringValue = "NumberLineJump";
            so.FindProperty("displayName").stringValue = "Number Line Jump (Easy)";
            so.FindProperty("description").stringValue = "Jump along the number line to reach the target number.";
            so.FindProperty("numberOfRounds").intValue = 3;
            so.FindProperty("maxAttemptsPerQuestion").intValue = 3;
            so.FindProperty("maxHintsPerQuestion").intValue = 3;
            so.FindProperty("tileSpacing").floatValue = 0.32f;

            // Set up default hints
            SetHint(so.FindProperty("hintLevel1"), "nlj_hint1", "Move the character and count your steps.", 1);
            SetHint(so.FindProperty("hintLevel2"), "nlj_hint2", "You started at X. You need to reach Y. How many steps is that?", 2);
            SetHint(so.FindProperty("hintLevel3"), "nlj_hint3", "Try jumping [direction] [N] times from where you are.", 3);

            var questionsProp = so.FindProperty("questions");
            questionsProp.arraySize = 3;

            // Question 1: Start at 3, target at 5 (2 jumps right)
            SetQuestion(questionsProp.GetArrayElementAtIndex(0),
                startNumber: 3,
                targetNumber: 5,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.RightOnly,
                maxJumps: 10);

            // Question 2: Start at 7, target at 4 (3 jumps left)
            SetQuestion(questionsProp.GetArrayElementAtIndex(1),
                startNumber: 7,
                targetNumber: 4,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.LeftOnly,
                maxJumps: 10);

            // Question 3: Start at 2, target at 8 (6 jumps right)
            SetQuestion(questionsProp.GetArrayElementAtIndex(2),
                startNumber: 2,
                targetNumber: 8,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.RightOnly,
                maxJumps: 10);

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
            Debug.Log($"[NumberLineJumpConfigFactory] Saved {AssetPath}");
        }

        private static void SetQuestion(SerializedProperty questionProp, int startNumber, int targetNumber,
            int numberLineMin, int numberLineMax, int jumpDirection, int maxJumps)
        {
            questionProp.FindPropertyRelative("startNumber").intValue = startNumber;
            questionProp.FindPropertyRelative("targetNumber").intValue = targetNumber;
            questionProp.FindPropertyRelative("numberLineMin").intValue = numberLineMin;
            questionProp.FindPropertyRelative("numberLineMax").intValue = numberLineMax;
            questionProp.FindPropertyRelative("jumpDirection").intValue = jumpDirection;
            questionProp.FindPropertyRelative("maxJumpsAllowed").intValue = maxJumps;
            questionProp.FindPropertyRelative("showEquationDuringJumps").boolValue = true;
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
