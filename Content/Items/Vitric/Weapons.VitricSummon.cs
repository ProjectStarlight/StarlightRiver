using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricSummon : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            item.damage = 45;
            item.knockBack = 3f;
            item.mana = 10;
            item.width = 32;
            item.height = 32;
            item.useTime = 36;
            item.useAnimation = 36;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.value = Item.buyPrice(0, 5, 0, 0);
            item.rare = ItemRarityID.Green;
            item.UseSound = SoundID.Item44;

            item.noMelee = true;
            item.summon = true;
            item.buffType = mod.BuffType("VitricSummonBuff");
            item.shoot = mod.ProjectileType("VitricSummonOrb");
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            player.AddBuff(item.buffType, 2);
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

            Projectile.NewProjectile(position, Vector2.Zero, type, damage, knockBack, player.whoAmI, index);

            return false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glassweaver's Punty");
            Tooltip.SetDefault("Summons the Glassweaver's arsenal");
        }
    }
}