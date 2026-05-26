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
            { "app_title", "Hoc Toan Vui" },
            { "btn_start", "Hoc tiep" },
            { "btn_progress", "Tien do" },
            { "btn_hint", "Goi y" },
            { "btn_confirm", "Xac nhan" },
            { "btn_retry", "Thu lai" },
            { "btn_home", "Ve menu" },
            { "btn_next", "Tiep tuc" },
            { "btn_listen", "Nghe lai" },
            { "btn_reset", "Lam lai" },
            { "activity_quantity_match", "Ghep so voi luong" },
            { "activity_compare_quantity", "So sanh so luong" },
            { "activity_number_line", "Nhay tren truc so" },
            { "feedback_correct", "Con lam dung roi!" },
            { "feedback_incorrect", "Chua dung, thu lai nhe!" },
            { "feedback_success", "Con lam tot lam!" },
            { "feedback_failed", "Con da co gang rat tot!" },
            { "feedback_hint", "Cung xem goi y nhe." },
            { "compare_question", "Nhom ben trai nhieu hon, it hon hay bang nhom ben phai?" },
            { "compare_more", "Nhieu hon" },
            { "compare_fewer", "It hon" },
            { "compare_equal", "Bang nhau" },
            { "quantity_choose_group", "Chon nhom co dung {0} con" },
            { "quantity_enter_number", "Nhap so con vat con dem duoc" },
            { "numberline_question", "Nhay tu {0} den {1}" },
            { "numberline_question_equation", "{0} {1} {2} = ?" },
            { "numberline_overshoot", "Con di qua roi! Dang o {0}, dich la {1}." },
            { "instruction_quantity_match", "Hay chon nhom co dung so con vat." },
            { "instruction_compare_quantity", "Hay so sanh hai nhom vat." },
            { "instruction_number_line", "Hay nhay tren truc so den dap an." }
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
