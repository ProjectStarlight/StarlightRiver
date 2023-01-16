using Microsoft.Xna.Framework;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.WorldGeneration;
using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.Items.Misc
{
	public class SojournersScarf : SmartAccessory, IChestItem
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public int Stack => 1;

        public ChestRegionFlags Regions => ChestRegionFlags.Underground | ChestRegionFlags.Surface | ChestRegionFlags.Granite | ChestRegionFlags.Marble;

        public SojournersScarf() : base("Sojourner's Scarf", "20% increased movement speed\n50% decreased life regeneration while stationary") { }

        public override void Load()
        {
            StarlightPlayer.NaturalLifeRegenEvent += HealthRegenLoss;
        }

		public override void Unload()
		{
            StarlightPlayer.NaturalLifeRegenEvent -= HealthRegenLoss;
        }

		public override void SafeUpdateEquip(Player Player) => Player.maxRunSpeed *= 1.20f;

        private void HealthRegenLoss(Player Player, ref float regen)
        {
            if (Equipped(Player) && Player.velocity == Vector2.Zero)
            {
                regen /= 2;
            }
        }
    }
}