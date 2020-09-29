using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Core;

namespace StarlightRiver.Items.Accessories.EarlyPreHardmode
{
    public class SojournersSash : SmartAccessory, IChestItem
    {
        public SojournersSash() : base("Sojourner's Sash", "20% increased max movement speed, with halved life regeneration while stationary") { }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.NaturalLifeRegenEvent += HealthRegenLoss;
            return true;
        }

        //This one is gonna require IL Editing due to how hardcoded af ManaRegen is, and that's above my level rn, sorry
        public override void SafeUpdateEquip(Player player)
        {
            player.maxRunSpeed *= 1.20f;
        }

        private void HealthRegenLoss(Player player,ref float regen)
        {
            if (Equipped(player))
            {
                if (player.velocity.Length() < 0.5)
                    regen /= 2f;
            }

        }

        public int ItemStack(Chest chest) => 1;
        public bool GenerateCondition(Chest chest)
        {
            return true;
        }

    }
}