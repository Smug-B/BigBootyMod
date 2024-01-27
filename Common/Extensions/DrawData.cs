using Terraria.DataStructures;

namespace BigBootyMod.Common.Extensions
{
    public static class DrawDataExtensions
    {
        public static void Draw(this DrawData drawData, DisposableSpriteDrawBuffer sb)
        {
            if (drawData.useDestinationRectangle)
            {
                sb.Draw(drawData.texture, drawData.destinationRectangle, drawData.sourceRect, drawData.color, drawData.rotation, drawData.origin, drawData.effect);
            }
            else
            {
                sb.Draw(drawData.texture, drawData.position, drawData.sourceRect, drawData.color, drawData.rotation, drawData.origin, drawData.scale, drawData.effect);
            }
        }
    }
}
