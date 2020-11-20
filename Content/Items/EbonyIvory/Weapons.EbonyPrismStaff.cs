using Microsoft.Xna.Framework;
using StarlightRiver.Projectiles.WeaponProjectiles.Summons;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items.EbonyIvory
{
    public class EbonyPrismStaff : ModItem
    {
        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("");
            DisplayName.SetDefault("P");
        }

        public override void SetDefaults()
        {
            item.mana = 10;
            item.damage = 8;
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.width = 26;
            item.height = 28;
            item.UseSound = SoundID.Item77;
            item.useAnimation = 36;
            item.useTime = 36;
            item.rare = ItemRarityID.Green;
            item.noMelee = true;
            item.knockBack = 2f;
            item.value = 10000;
            item.summon = true;
            item.shoot = ProjectileType<EbonyPrismSummon>();
            item.shootSpeed = 0f;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (player.ownedProjectileCounts[ProjectileType<EbonyPrismSummon>()] > 0)
            {
                for (int i = 0; i < Main.projectile.Length; i++)
                {
                    if (Main.projectile[i].active)
                    {
                        if (Main.projectile[i].type == type)
                        {
                            if (Main.projectile[i].owner == player.whoAmI)
                            {

                                Main.projectile[i].minionSlots += 1;

                                if (Main.projectile[i].minionSlots < player.maxMinions)
                                {
                                    Main.projectile[i].minionSlots += 1;
                                }

                            }
                        }
                    }
                }
                return false;
            }
            Projectile.NewProjectile(position, Vector2.Zero, type, damage, knockBack, player.whoAmI, 0, 0);
            return false;
        }
    }
}