using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using Terraria;
using System;
using Microsoft.Xna.Framework;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricArrow : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
            Tooltip.SetDefault("Shatters on impact");
        }

        public override void SetDefaults()
        {
            item.damage = 8;
            item.ranged = true;
            item.width = 8;
            item.height = 8;
            item.maxStack = 999;
            item.consumable = true;
            item.knockBack = 0.5f;
            item.value = 10;
            item.rare = ItemRarityID.Green;
            item.shoot = ProjectileType<VitricArrowProjectile>();
            item.shootSpeed = 1f;
            item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            ModRecipe recipe = new ModRecipe(mod);
            recipe.AddIngredient(ItemType<VitricOre>(), 4);
            recipe.AddTile(TileID.WorkBenches);
            recipe.SetResult(this, 50);
            recipe.AddRecipe();
        }
    }

    internal class VitricArrowProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 270;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
        }

        public override void AI()
        {
            projectile.rotation = projectile.velocity.ToRotation() + (float)Math.PI / 2;
            projectile.ai[0]++;

            if (projectile.ai[0] >= 25f)
                projectile.velocity.Y += 0.05f;
        }

        public void MakeDusts(int dustcount = 10)
        {
            for (int k = 0; k <= dustcount; k++)
            {
                Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.Air>());
                Dust.NewDustPerfect(projectile.position, DustType<Dusts.GlassGravity>(), projectile.velocity.RotatedByRandom(0.3f));
            }

            if (dustcount > 5)
                Main.PlaySound(SoundID.Item27);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            for (int k = 0; k < 3; k++)
            {
                Projectile.NewProjectile(target.Center, projectile.velocity.RotatedByRandom(0.3f), ProjectileType<VitricArrowShattered>(), projectile.damage / 3, 0, projectile.owner);
            }
            MakeDusts();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            MakeDusts();
            return true;
        }
    }

    internal class VitricArrowShattered : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 2;
            projectile.timeLeft = 120;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
        }

        public override void AI()
        {
            projectile.rotation += 0.3f;
            projectile.velocity.Y += 0.15f;
        }
    }
}