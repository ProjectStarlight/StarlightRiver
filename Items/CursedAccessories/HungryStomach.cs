using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Abilities;
using System;

namespace StarlightRiver.Items.CursedAccessories
{
    internal class HungryStomach : CursedAccessory
    {
        public HungryStomach() : base(GetTexture("StarlightRiver/Items/CursedAccessories/AnthemDaggerGlow")) { }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Restore stamina by damaging foes\nMelee weapons are twice as effective\nDisables natural stamina regeneration");
            DisplayName.SetDefault("Hungry Stomach");
        }

        public override void SafeSetDefaults()
        {
            item.rare = ItemRarityID.Red;
        }

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PostUpdateEquipsEvent += DisableRegen;
            StarlightPlayer.ModifyHitNPCEvent += LeechStaminaMelee;
            StarlightProjectile.ModifyHitNPCEvent += LeechStaminaRanged;
            return true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            GUI.Stam.overrideTexture = GetTexture("StarlightRiver/GUI/Assets/StaminaBlood");
        }

        private void DisableRegen(StarlightPlayer player)
        {
            if (Equipped(player.player))
            {
                player.player.GetHandler().StaminaRegenRate = 0;
            }
        }

        private void LeechStaminaRanged(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Equipped(Main.player[projectile.owner]))
            {
                Main.player[projectile.owner].GetHandler().Stamina += damage / (projectile.melee ? 100f : 200f);
            }
        }

        private void LeechStaminaMelee(Player player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (Equipped(player))
            {
                player.GetHandler().Stamina += damage / 100f;
            }
        }
    }
}