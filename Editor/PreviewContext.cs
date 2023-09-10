using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public class PreviewContext : ScriptableSingleton<PreviewContext>
    {
        [SerializeField]
        private TextureTransformer previweing = null;
        [SerializeField]
        private PreviewDomain previewDomain;

        protected PreviewContext()
        {
        }

        public void DrawApplyAndRevert(TextureTransformer target)
        {
            if (target == null) return;
            if (previweing == null)
            {
                if (GUILayout.Button("Preview"))
                {
                    previweing = target;
                    previewDomain = new PreviewDomain(target.GetRenderers);
                    target.Apply(previewDomain);
                    previewDomain.EditFinish();
                    EditorUtility.SetDirty(target);
                }
            }
            else if (previweing == target)
            {
                if (GUILayout.Button("Revert"))
                {
                    previweing = null;
                    target.IsApply = false;
                    previewDomain.Dispose();
                    previewDomain = null;
                    EditorUtility.SetDirty(target);
                }
            }
            else
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button("Preview (Other Previewing)");
                EditorGUI.EndDisabledGroup();
            }
        }
    }
}