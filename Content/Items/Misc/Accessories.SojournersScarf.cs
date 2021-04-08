using StarlightRiver.Core;
using Terraria;
using StarlightRiver.Content.Items.BaseTypes;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.WorldGeneration;

namespace StarlightRiver.Content.Items.Misc
{
    public class SojournersScarf : SmartAccessory, IChestItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public int Stack => 1;

        public ChestRegionFlags Regions => ChestRegionFlags.Underground | ChestRegionFlags.Surface | ChestRegionFlags.Granite | ChestRegionFlags.Marble;

        public SojournersScarf() : base("Sojourner's Scarf", "20% increased movement speed\n50% decreased life regeneration while stationary") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.NaturalLifeRegenEvent += HealthRegenLoss;
            return true;
        }

        public override void SafeUpdateEquip(Player player) => player.maxRunSpeed *= 1.20f;

        private void HealthRegenLoss(Player player, ref float regen)
        {
            if (Equipped(player) && player.velocity == Vector2.Zero)
            {
                regen /= 2;
            }
        }
    }
}