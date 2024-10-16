using System.Collections.Generic;
using UnityEngine;
using net.rs64.TexTransCoreEngineForUnity.Island;
using System;

namespace net.rs64.TexTransTool.TextureAtlas
{

    public struct IslandRect : IIslandRect
    {
        public Vector2 Pivot;
        public Vector2 Size;
        public bool Is90Rotation;

        Vector2 IIslandRect.Pivot { get => Pivot; set => Pivot = value; }
        Vector2 IIslandRect.Size { get => Size; set => Size = value; }
        bool IIslandRect.Is90Rotation { get => Is90Rotation; set => Is90Rotation = value; }

        public IslandRect(Vector2 pivot, Vector2 size, bool is90Rotation)
        {
            Pivot = pivot;
            Size = size;
            Is90Rotation = is90Rotation;
        }

        public IslandRect(Island island)
        {
            Pivot = island.Pivot;
            Size = island.Size;
            Is90Rotation = island.Is90Rotation;
        }

        internal IslandRect(IIslandRect island)
        {
            Pivot = island.Pivot;
            Size = island.Size;
            Is90Rotation = island.Is90Rotation;
        }
        internal void Rotate90()
        {
            Is90Rotation = !Is90Rotation;
            (Size.x, Size.y) = (Size.y, Size.x);
        }
    }

}
