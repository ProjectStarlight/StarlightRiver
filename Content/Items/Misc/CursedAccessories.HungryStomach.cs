using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.Items.Misc
{
    internal class HungryStomach : CursedAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public HungryStomach() : base(GetTexture(AssetDirectory.MiscItem + "HungryStomachGlow")) { }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Restore stamina by damaging foes\nMelee weapons are twice as effective\nDisables natural stamina regenration");
            DisplayName.SetDefault("Hungry Stomach");
        }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Red;

        public override bool Autoload(ref string name)
        {
            StarlightPlayer.PostUpdateEquipsEvent += DisableRegen;
            StarlightPlayer.ModifyHitNPCEvent += LeechStaminaMelee;
            StarlightProjectile.ModifyHitNPCEvent += LeechStaminaRanged;
            return true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) => GUI.Stam.overrideTexture = GetTexture("StarlightRiver/Assets/GUI/StaminaBlood");

        private void DisableRegen(StarlightPlayer player)
        {
            if (Equipped(player.player))
                player.player.GetHandler().StaminaRegenRate = 0;
        }

        private void LeechStaminaRanged(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Equipped(Main.player[projectile.owner]))
                Main.player[projectile.owner].GetHandler().Stamina += damage / (projectile.melee ? 100f : 200f);
        }

        private void LeechStaminaMelee(Player player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (Equipped(player))
                player.GetHandler().Stamina += damage / 100f;
        }
    }
}