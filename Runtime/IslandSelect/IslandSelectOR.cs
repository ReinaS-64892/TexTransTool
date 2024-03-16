using System.Collections.Generic;
using net.rs64.TexTransCore.Island;

namespace net.rs64.TexTransTool.IslandSelector
{
    public class IslandSelectOR : AbstractIslandSelector
    {
        public List<AbstractIslandSelector> IslandSelectors;
        internal override HashSet<Key> IslandSelect<Key>(Dictionary<Key, Island> islands, Dictionary<Key, IslandDescription> islandDescription)
        {
            var islandHash = new HashSet<Key>(islands.Count);
            foreach (var selector in IslandSelectors)
            {
                islandHash.UnionWith(selector.IslandSelect(islands, islandDescription));
            }
            return islandHash;
        }
    }
}
