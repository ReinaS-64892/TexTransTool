using UnityEditor;
using UnityEngine;

namespace net.rs64.TexTransTool
{
    public class PreviewContext : ScriptableSingleton<PreviewContext>
    {
        private TextureTransformer previweing = null;

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
                    target.PreviewApply();
                    EditorUtility.SetDirty(target);
                }
            }
            else if (previweing == target)
            {
                EditorGUI.BeginDisabledGroup(!target.IsPreviewApply);
                if (GUILayout.Button("Revert"))
                {
                    target.PreviewRevert();
                    previweing = null;
                    EditorUtility.SetDirty(target);
                }
                EditorGUI.EndDisabledGroup();
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