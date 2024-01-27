using BigBootyMod.Common;
using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace BigBootyMod
{
    public class BigBootyConfig : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ClientSide;

        [Header("Bakery")]
        [DefaultValue(false)]
        [Tooltip("Whether or not these bodacious cheeks are also rendered as armor after effects.")]
        public bool MultiSampling;

        public override void OnChanged() => BigBootySystem.MultiSample = MultiSampling;
    }
}