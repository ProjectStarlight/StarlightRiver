using System;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Projectiles.Ammo;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Items.Accessories.EarlyPreHardmode
{
    public class GlassArrowhead : SmartAccessory
    {
        public GlassArrowhead() : base("Glass Arrowhead", "Arrows shatter into glass shards on critical hit\nsome shard may pass through the initial hit target") { }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemID.SilverBar, 5);
            recipe.AddIngredient(ItemID.JungleSpores, 10);
            recipe.AddTile(TileID.Furnaces);
            recipe.SetResult(this);
            recipe.AddRecipe();
        }
        public override bool Autoload(ref string name)
        {
            StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
            return true;
        }

        private void OnHitNPCWithProjAccessory(Player player,Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
            if (Equipped(player) && proj.arrow && crit)
            {
                VitricArrow.MakeDusts(proj, 6);
                for (int i = 0; i < 3; i += 1)
                {
                    Vector2 velocity = proj.velocity.RotatedByRandom(MathHelper.Pi / 6f);
                    velocity *= Main.rand.NextFloat(0.5f, 0.75f);
                    int newproj = Projectile.NewProjectile(proj.Center, velocity, ModContent.ProjectileType<GlassheadShard>(), (int)((float)damage * 0.20f), knockback * 0.15f, player.whoAmI);
                    if (Main.rand.NextBool())
                    {
                        Main.projectile[newproj].ai[0] = target.whoAmI + 1000;
                        Main.projectile[newproj].netUpdate = true;
                    }
                }
            }
            
        }

    }
}