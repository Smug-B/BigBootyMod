using Microsoft.Xna.Framework;
using Terraria;

namespace LunacyMod.Core.Utils
{
    public static class GraphicsUtils
    {
        /// <returns>A normalized <paramref name="relativeCoordinates"/> which ranges from 0 - 1.</returns>
        public static Vector2 GetUVCoordinates(Vector2 relativeCoordinates, int width, int height)
        {
            return new Vector2(relativeCoordinates.X / width, relativeCoordinates.Y / height);
        }

        public static Vector2 ScreenToNormalizedDeviceCoordinates(Vector2 screenCoordinates)
        {
            Vector2 normalizedCoordinates = screenCoordinates / Main.ScreenSize.ToVector2(); 
            normalizedCoordinates.Y = 1 - normalizedCoordinates.Y;
            Vector2 adjustedCoordinates = normalizedCoordinates * 2 - Vector2.One;
            return adjustedCoordinates;
        }
    }
}