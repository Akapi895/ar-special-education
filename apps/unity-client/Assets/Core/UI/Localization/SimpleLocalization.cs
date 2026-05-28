using UnityEngine;
using System.Collections.Generic;

namespace Core.UI.Localization
{
    public static class SimpleLocalization
    {
        private static Dictionary<string, string> localizedStrings;
        private static bool initialized = false;

        private static readonly Dictionary<string, string> fallbackVietnamese = new Dictionary<string, string>
        {
            { "app_title", "H\u1ecdc To\u00e1n Vui" },
            { "btn_start", "B\u1eaft \u0111\u1ea7u h\u1ecdc" },
            { "btn_progress", "Ti\u1ebfn \u0111\u1ed9" },
            { "btn_hint", "G\u1ee3i \u00fd" },
            { "btn_confirm", "X\u00e1c nh\u1eadn" },
            { "btn_retry", "Th\u1eed l\u1ea1i" },
            { "btn_home", "Trang ch\u1ee7" },
            { "btn_next", "Ti\u1ebfp t\u1ee5c" },
            { "btn_listen", "Nghe l\u1ea1i" },
            { "btn_reset", "L\u00e0m l\u1ea1i" },
            { "activity_quantity_match", "Gh\u00e9p s\u1ed1 v\u1edbi l\u01b0\u1ee3ng" },
            { "activity_compare_quantity", "So s\u00e1nh s\u1ed1 l\u01b0\u1ee3ng" },
            { "activity_number_line", "Nh\u1ea3y tr\u00ean tr\u1ee5c s\u1ed1" },
            { "feedback_correct", "Con l\u00e0m \u0111\u00fang r\u1ed3i!" },
            { "feedback_incorrect", "Ch\u01b0a \u0111\u00fang, th\u1eed l\u1ea1i nh\u00e9!" },
            { "feedback_success", "Con l\u00e0m t\u1ed1t l\u1eafm!" },
            { "feedback_great", "Gi\u1ecfi qu\u00e1!" },
            { "feedback_failed", "Con \u0111\u00e3 c\u1ed1 g\u1eafng r\u1ea5t t\u1ed1t!" },
            { "feedback_hint", "C\u00f9ng xem g\u1ee3i \u00fd nh\u00e9." },
            { "compare_question", "Nh\u00f3m b\u00ean tr\u00e1i nhi\u1ec1u h\u01a1n, \u00edt h\u01a1n hay b\u1eb1ng nh\u00f3m b\u00ean ph\u1ea3i?" },
            { "compare_more", "Nhi\u1ec1u h\u01a1n" },
            { "compare_fewer", "\u00cdt h\u01a1n" },
            { "compare_equal", "B\u1eb1ng nhau" },
            { "quantity_choose_group", "Ch\u1ecdn nh\u00f3m c\u00f3 \u0111\u00fang {0} con" },
            { "quantity_enter_number", "Nh\u1eadp s\u1ed1 con v\u1eadt con \u0111\u1ebfm \u0111\u01b0\u1ee3c" },
            { "numberline_question", "Nh\u1ea3y t\u1eeb {0} \u0111\u1ebfn {1}" },
            { "numberline_question_equation", "{0} {1} {2} = ?" },
            { "numberline_overshoot", "Con \u0111i qu\u00e1 r\u1ed3i! \u0110ang \u1edf {0}, \u0111\u00edch l\u00e0 {1}." },
            { "instruction_quantity_match", "H\u00e3y ch\u1ecdn nh\u00f3m c\u00f3 \u0111\u00fang s\u1ed1 con v\u1eadt." },
            { "instruction_quantity_count", "Con h\u00e3y \u0111\u1ebfm s\u1ed1 con v\u1eadt r\u1ed3i ch\u1ecdn \u0111\u00e1p \u00e1n." },
            { "instruction_compare_quantity", "H\u00e3y so s\u00e1nh hai nh\u00f3m v\u1eadt." },
            { "instruction_number_line", "H\u00e3y nh\u1ea3y tr\u00ean tr\u1ee5c s\u1ed1 \u0111\u1ebfn \u0111\u00e1p \u00e1n." }
        };

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
                MergeFallbackStrings();
                Debug.Log("[SimpleLocalization] Successfully loaded localization strings.");
            }
            else
            {
                MergeFallbackStrings();
                Debug.LogWarning("[SimpleLocalization] Localization file SO_Strings_VI not found! Falling back to keys.");
            }

            initialized = true;
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

        public static string GetOrDefault(string key, string fallback)
        {
            string value = Get(key);
            return value == key ? fallback : value;
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

        private static void MergeFallbackStrings()
        {
            foreach (var kvp in fallbackVietnamese)
            {
                if (!localizedStrings.ContainsKey(kvp.Key))
                {
                    localizedStrings[kvp.Key] = kvp.Value;
                }
            }
        }
    }
}
