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
            so.FindProperty("displayName").stringValue = "Nh\u1ea3y tr\u00ean tr\u1ee5c s\u1ed1 (D\u1ec5)";
            so.FindProperty("description").stringValue = "Con nh\u1ea3y tr\u00ean tr\u1ee5c s\u1ed1 \u0111\u1ec3 \u0111\u1ebfn \u0111\u00fang s\u1ed1 c\u1ea7n t\u00ecm.";
            so.FindProperty("numberOfRounds").intValue = 10;
            so.FindProperty("maxAttemptsPerQuestion").intValue = 3;
            so.FindProperty("maxHintsPerQuestion").intValue = 3;
            so.FindProperty("tileSpacing").floatValue = 0.32f;
            so.FindProperty("correctFeedback").stringValue = "Ch\u00ednh x\u00e1c! Con \u0111\u00e3 \u0111\u1ebfn \u0111\u00fang s\u1ed1 r\u1ed3i!";
            so.FindProperty("perfectFeedback").stringValue = "Tuy\u1ec7t v\u1eddi! Con nh\u1ea3y \u0111\u00fang s\u1ed1 b\u01b0\u1edbc c\u1ea7n thi\u1ebft!";
            so.FindProperty("incorrectFeedback").stringValue = "Ch\u01b0a \u0111\u00fang r\u1ed3i. Con h\u00e3y \u0111\u1ebfm l\u1ea1i s\u1ed1 b\u01b0\u1edbc nh\u1ea3y nh\u00e9!";
            so.FindProperty("overshootFeedback").stringValue = "Con \u0111i qu\u00e1 xa r\u1ed3i. M\u00ecnh b\u1eaft \u0111\u1ea7u l\u1ea1i nh\u00e9!";
            so.FindProperty("boundaryFeedback").stringValue = "Con \u0111\u00e3 t\u1edbi m\u00e9p r\u1ed3i, kh\u00f4ng \u0111i xa h\u01a1n \u0111\u01b0\u1ee3c n\u1eefa.";
            so.FindProperty("maxJumpsFeedback").stringValue = "Con nh\u1ea3y h\u01a1i nhi\u1ec1u r\u1ed3i. M\u00ecnh th\u1eed l\u1ea1i ch\u1eadm h\u01a1n nh\u00e9!";
            so.FindProperty("failedFeedback").stringValue = "Con \u0111\u00e3 c\u1ed1 g\u1eafng r\u1ea5t t\u1ed1t. M\u00ecnh th\u1eed c\u00e2u kh\u00e1c nh\u00e9.";
            so.FindProperty("maxJumpsWarning").stringValue = "Con s\u1eafp h\u1ebft l\u01b0\u1ee3t nh\u1ea3y r\u1ed3i. H\u00e3y ngh\u0129 th\u1eadt k\u1ef9 nh\u00e9!";

            // Set up default hints
            SetHint(so.FindProperty("hintLevel1"), "nlj_hint1", "Con h\u00e3y di chuy\u1ec3n nh\u00e2n v\u1eadt v\u00e0 \u0111\u1ebfm t\u1eebng b\u01b0\u1edbc nh\u1ea3y.", 1);
            SetHint(so.FindProperty("hintLevel2"), "nlj_hint2", "Con b\u1eaft \u0111\u1ea7u \u1edf X v\u00e0 c\u1ea7n \u0111\u1ebfn Y. V\u1eady c\u1ea7n nh\u1ea3y m\u1ea5y b\u01b0\u1edbc?", 2);
            SetHint(so.FindProperty("hintLevel3"), "nlj_hint3", "H\u00e3y nh\u1ea3y [direction] [N] b\u01b0\u1edbc t\u1eeb v\u1ecb tr\u00ed hi\u1ec7n t\u1ea1i.", 3);

            var questionsProp = so.FindProperty("questions");
            questionsProp.arraySize = 10;

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

            SetQuestion(questionsProp.GetArrayElementAtIndex(3),
                startNumber: 9,
                targetNumber: 6,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.LeftOnly,
                maxJumps: 10);

            SetQuestion(questionsProp.GetArrayElementAtIndex(4),
                startNumber: 1,
                targetNumber: 4,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.RightOnly,
                maxJumps: 10);

            SetQuestion(questionsProp.GetArrayElementAtIndex(5),
                startNumber: 2,
                targetNumber: 6,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.RightOnly,
                maxJumps: 10);

            SetQuestion(questionsProp.GetArrayElementAtIndex(6),
                startNumber: 8,
                targetNumber: 3,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.LeftOnly,
                maxJumps: 10);

            SetQuestion(questionsProp.GetArrayElementAtIndex(7),
                startNumber: 1,
                targetNumber: 9,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.RightOnly,
                maxJumps: 10);

            SetQuestion(questionsProp.GetArrayElementAtIndex(8),
                startNumber: 10,
                targetNumber: 4,
                numberLineMin: 0,
                numberLineMax: 10,
                jumpDirection: (int)JumpDirection.LeftOnly,
                maxJumps: 10);

            SetQuestion(questionsProp.GetArrayElementAtIndex(9),
                startNumber: 0,
                targetNumber: 7,
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
