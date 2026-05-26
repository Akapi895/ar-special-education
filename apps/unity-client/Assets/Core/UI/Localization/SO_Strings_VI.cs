using UnityEngine;
using System.Collections.Generic;

namespace Core.UI.Localization
{
    [CreateAssetMenu(fileName = "SO_Strings_VI", menuName = "UI/Localization Strings", order = 2)]
    public class SO_Strings_VI : ScriptableObject
    {
        [System.Serializable]
        public class StringEntry
        {
            public string key;
            [TextArea(2, 6)]
            public string value;
        }

        [Header("Localization Entries")]
        [SerializeField] private List<StringEntry> entries = new List<StringEntry>();

        public void PopulateDictionary(Dictionary<string, string> dict)
        {
            if (dict == null) return;
            dict.Clear();
            foreach (var entry in entries)
            {
                if (entry != null && !string.IsNullOrEmpty(entry.key))
                {
                    dict[entry.key.ToLower()] = entry.value;
                }
            }
        }
    }
}
