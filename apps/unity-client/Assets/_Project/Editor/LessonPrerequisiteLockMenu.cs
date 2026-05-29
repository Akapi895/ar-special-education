#if UNITY_EDITOR
using Core.Data;
using UnityEditor;
using UnityEngine;

namespace Project.Editor
{
    public static class LessonPrerequisiteLockMenu
    {
        private const string ToggleMenuPath = "AR Learning/Dev/Enforce Lesson Prerequisites";
        private const string ResetMenuPath = "AR Learning/Dev/Reset Lesson Prerequisite Default";

        [MenuItem(ToggleMenuPath)]
        public static void ToggleLessonPrerequisiteLock()
        {
            UserPreferences.EnforceLessonPrerequisites = !UserPreferences.EnforceLessonPrerequisites;
            LogCurrentState();
        }

        [MenuItem(ToggleMenuPath, true)]
        public static bool ValidateToggleLessonPrerequisiteLock()
        {
            Menu.SetChecked(ToggleMenuPath, UserPreferences.EnforceLessonPrerequisites);
            return true;
        }

        [MenuItem(ResetMenuPath)]
        public static void ResetLessonPrerequisiteDefault()
        {
            UserPreferences.ResetLessonPrerequisitePreference();
            LogCurrentState();
        }

        private static void LogCurrentState()
        {
            string state = UserPreferences.EnforceLessonPrerequisites
                ? "enabled: learners must follow lesson prerequisites."
                : "disabled: all playable activities are unlocked for testing.";

            Debug.Log($"[LessonPrerequisiteLockMenu] Sequential lesson lock is {state}");
        }
    }
}
#endif
