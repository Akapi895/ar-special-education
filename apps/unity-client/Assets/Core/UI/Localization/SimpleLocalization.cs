using UnityEngine;
using System.Collections.Generic;

namespace Core.UI.Localization
{
    public static class SimpleLocalization
    {
        private static Dictionary<string, string> localizedStrings;
        private static bool initialized = false;

        public static void Initialize()
        {
            if (initialized) return;

            localizedStrings = new Dictionary<string, string>();
            
            // Try to load SO_Strings_VI from Resources
            SO_Strings_VI stringsSO = Resources.Load<SO_Strings_VI>("SO_Strings_VI");

            #if UNITY_EDITOR
            if (stringsSO == null)
            {
                // In Editor, try to load from AssetDatabase
                string[] guids = UnityEditor.AssetDatabase.FindAssets("t:SO_Strings_VI");
                if (guids.Length > 0)
                {
                    string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                    stringsSO = UnityEditor.AssetDatabase.LoadAssetAtPath<SO_Strings_VI>(path);
                }
            }
            #endif

            if (stringsSO != null)
            {
                stringsSO.PopulateDictionary(localizedStrings);
                initialized = true;
                Debug.Log("[SimpleLocalization] Successfully loaded localization strings.");
            }
            else
            {
                Debug.LogWarning("[SimpleLocalization] Localization file SO_Strings_VI not found! Falling back to keys.");
            }
        }

        public static string Get(string key)
        {
            if (!initialized)
            {
                Initialize();
            }

            if (localizedStrings != null && localizedStrings.TryGetValue(key.ToLower(), out string val))
            {
                return val;
            }

            return key;
        }

        public static string Get(string key, params object[] args)
        {
            string format = Get(key);
            try
            {
                return string.Format(format, args);
            }
            catch (System.Exception)
            {
                return format;
            }
        }
    }
}
