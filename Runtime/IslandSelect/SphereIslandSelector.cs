using System.Collections.Generic;
using System.Linq;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.IslandSelector
{
    public class SphereIslandSelector : AbstractIslandSelector
    {
        public float SphereSize = 0.1f;
        internal override HashSet<Key> IslandSelect<Key>(Dictionary<Key, Island> islands, Dictionary<Key, IslandDescription> islandDescription)
        {
            var islandHash = new HashSet<Key>(islands.Count);

            var matrix = transform.worldToLocalMatrix;

            foreach (var island in islands)
            {
                var description = islandDescription[island.Key];
                foreach (var i in island.Value.triangles.SelectMany(i => i))
                {
                    if (matrix.MultiplyPoint3x4(description.Position[i]).magnitude < SphereSize) { islandHash.Add(island.Key); break; }
                }
            }

            return islandHash;
        }
    }
}
