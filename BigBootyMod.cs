using ReLogic.Content;
using System.Diagnostics.CodeAnalysis;
using Terraria.ModLoader;

namespace BigBootyMod
{
	public class BigBootyMod : Mod
	{
        [NotNull]
        public static BigBootyMod? Mod { get; private set; }

        public BigBootyMod() => Mod = this;

        public static Asset<T> Request<T>(string assetName, AssetRequestMode mode = AssetRequestMode.AsyncLoad) where T : class => Mod.Assets.Request<T>(assetName, mode);
    }
}