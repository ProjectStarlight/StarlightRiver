using System;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Terraria.ID;
using Terraria.DataStructures;
using StarlightRiver.Projectiles.WeaponProjectiles;
using Microsoft.Xna.Framework.Graphics;

namespace StarlightRiver.Items.BossDrops.VitricBossDrops
{
    class VitricBossBow : ModItem,IGlowingItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Greatbow");
            Tooltip.SetDefault("Charges a volley of cystal shards that fire in an arc\nCharging increases the arc size, shot count, and overall power of shots");
        }

        public override void SetDefaults()
        {
            item.damage = 15;
            item.ranged = true;
            item.width = 16;
            item.height = 64;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.knockBack = 1;
            item.rare = ItemRarityID.Orange;
            item.channel = true;
            item.shoot = ModContent.ProjectileType<Projectiles.WeaponProjectiles.VitricBowProjectile>();
            item.shootSpeed = 1f;
            item.useAmmo = AmmoID.Arrow;
            item.useTurn = true;
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            Projectile.NewProjectile(position, new Vector2(speedX, speedY)/4f,item.shoot,damage,knockBack,player.whoAmI);
            return false;
        }

        public void DrawGlowmask(PlayerDrawInfo info)
        {
            Player player = info.drawPlayer;

            if (player.heldProj > -1 && Main.projectile[player.heldProj].type==mod.ProjectileType("VitricBowProjectile"))
            {
                Projectile held = Main.projectile[player.heldProj];
                Vector2 off = Vector2.Normalize(held.velocity) * 16;

                var data = new DrawData(GetTexture(Texture), player.Center + off - Main.screenPosition, null, Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16), off.ToRotation(), item.Size / 2, 1, 0, 0);
                Main.playerDrawData.Add(data);

                if (held.modProjectile != null && held.modProjectile is VitricBowProjectile)
                {
                    float fraction = held.ai[0] / VitricBowProjectile.MaxCharge;
                    Color colorz = Color.Lerp(Lighting.GetColor((int)player.Center.X / 16, (int)player.Center.Y / 16), Color.Aquamarine, fraction)*Math.Min(held.timeLeft/20f,1f);

                    var data2 = new DrawData(GetTexture(Texture), player.Center + off - Main.screenPosition, null, colorz, off.ToRotation(), item.Size / 2, 1, 0, 0);
                    Main.playerDrawData.Add(data2);
                }
                
            }
        }
    }
}
