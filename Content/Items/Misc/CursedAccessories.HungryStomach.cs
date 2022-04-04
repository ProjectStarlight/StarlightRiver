using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	internal class HungryStomach : CursedAccessory
    {
        public override string Texture => AssetDirectory.MiscItem + Name;
        public HungryStomach() : base(Request<Texture2D>(AssetDirectory.MiscItem + "HungryStomachGlow").Value) { }

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Restore stamina by damaging foes\nMelee weapons are twice as effective\nDisables natural stamina regenration");
            DisplayName.SetDefault("Hungry Stomach");
        }

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Red;

        public override void Load()
        {
            StarlightPlayer.PostUpdateEquipsEvent += DisableRegen;
            StarlightPlayer.ModifyHitNPCEvent += LeechStaminaMelee;
            StarlightProjectile.ModifyHitNPCEvent += LeechStaminaRanged;
            return true;
        }

        public override void SafeUpdateAccessory(Player Player, bool hideVisual) => GUI.Stam.overrideTexture = Request<Texture2D>("StarlightRiver/Assets/GUI/StaminaBlood").Value;

        private void DisableRegen(StarlightPlayer Player)
        {
            if (Equipped(Player.Player))
                Player.Player.GetHandler().StaminaRegenRate = 0;
        }

        private void LeechStaminaRanged(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (Equipped(Main.player[Projectile.owner]))
                Main.player[Projectile.owner].GetHandler().Stamina += damage / (Projectile.DamageType.CountsAs(DamageClass.Melee) ? 100f : 200f);
        }

        private void LeechStaminaMelee(Player Player, Item Item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
            if (Equipped(Player))
                Player.GetHandler().Stamina += damage / 100f;
        }
    }
}