using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    [FilePath("TexTransTool/TTTGlobalConfig.asset", FilePathAttribute.Location.PreferencesFolder)]
    internal sealed class TTTGlobalConfig : ScriptableSingleton<TTTGlobalConfig>
    {
        [SerializeField] string language = "ja-JP";

        public string Language
        {
            get => language;
            set
            {
                if (language == value) { return; }
                language = value;
                Save();
            }
        }











        private void Save()
        {
            Save(true);
        }
    }
}
