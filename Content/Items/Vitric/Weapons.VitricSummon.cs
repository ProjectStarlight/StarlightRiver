using Microsoft.Xna.Framework;
using StarlightRiver.Content.Buffs.Summon;
using StarlightRiver.Core;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
	public class VitricSummon : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            Item.damage = 20;
            Item.knockBack = 3f;
            Item.mana = 10;
            Item.width = 32;
            Item.height = 32;
            Item.useTime = 36;
            Item.useAnimation = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = Item.buyPrice(0, 5, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item44;

            Item.noMelee = true;
            Item.DamageType = DamageClass.Magic;
            Item.buffType = ModContent.BuffType<VitricSummonBuff>();
            Item.shoot = ModContent.ProjectileType<VitricSummonOrb>();
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            player.AddBuff(Item.buffType, 2);
            position = player.Center - new Vector2(player.direction * 64, 16);

            int index = 0;

            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile currentProjectile = Main.projectile[i];
                if (currentProjectile.active
                && currentProjectile.owner == player.whoAmI
                && currentProjectile.type == type)
                {
                    if (i == currentProjectile.whoAmI)
                        index += 1;
                    index %= 4;
                }
            }

            Projectile.NewProjectile(player.GetSource_ItemUse(Item), position, Vector2.Zero, type, damage, knockback, player.whoAmI, index);

            return false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glassweaver's Punty");
            Tooltip.SetDefault("Summons the Glassweaver's arsenal");
        }
    }
}