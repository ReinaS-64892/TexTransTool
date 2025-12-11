using UnityEngine;

namespace net.rs64.TexTransTool
{
    internal class MaterialSelectorAttribute : PropertyAttribute
    {
        public enum Side
        {
            Left,
            Right
        }

        public Side Button { get; }
        public Side Popup { get; }

        public MaterialSelectorAttribute(Side side = Side.Right, Side popup = Side.Left)
        {
            Button = side;
            Popup = popup;
        }
    }
}