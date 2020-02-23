using Gimic.Interfaces;
using Gimic.Models;

namespace Gimic.Extensions
{
    public static class MapExtensions
    {
        /// <summary>
        /// Closes a nested map value if the tab count of the current line is less than the current map nesting level.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="line"></param>
        /// <returns></returns>
        public static IMap<string, dynamic> CloseIfComplete(this IMap<string, dynamic> map, Line line)
        {
            if (map is null || line is null) return null;
            if (line.TabCount < map.NestingLevel)
            {
                line.NormalizeOutdentRelativeTo(map);
                map = map.Parent;
            }
            return map;
        }
    }
}
