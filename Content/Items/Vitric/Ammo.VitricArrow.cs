using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

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
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 8;
            Item.height = 8;
            Item.maxStack = 999;
            Item.consumable = true;
            Item.knockBack = 0.5f;
            Item.value = 10;
            Item.rare = ItemRarityID.Green;
            Item.shoot = ProjectileType<VitricArrowProjectile>();
            Item.shootSpeed = 1f;
            Item.ammo = AmmoID.Arrow;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(50);
            recipe.AddIngredient(ItemType<VitricOre>(), 4);
            recipe.AddTile(TileID.WorkBenches);
        }
    }

    internal class VitricArrowProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 270;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + (float)Math.PI / 2;
            Projectile.ai[0]++;

            if (Projectile.ai[0] >= 25f)
                Projectile.velocity.Y += 0.05f;
        }

        public void MakeDusts(int dustcount = 10)
        {
            for (int k = 0; k <= dustcount; k++)
            {
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustType<Dusts.Air>());
                Dust.NewDustPerfect(Projectile.position, DustType<Dusts.GlassGravity>(), Projectile.velocity.RotatedByRandom(0.3f));
            }

            if (dustcount > 5)
                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item27);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            for (int k = 0; k < 3; k++)
            {
                Projectile.NewProjectile(Projectile.GetProjectileSource_FromThis(), target.Center, Projectile.velocity.RotatedByRandom(0.3f), ProjectileType<VitricArrowShattered>(), Projectile.damage / 3, 0, Projectile.owner);
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
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 120;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Arrow");
        }

        public override void AI()
        {
            Projectile.rotation += 0.3f;
            Projectile.velocity.Y += 0.15f;
        }
    }
}