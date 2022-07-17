using StarlightRiver.Core;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using StarlightRiver.Core.Systems;
using StarlightRiver.Helpers;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using System.IO;

namespace StarlightRiver.Content.Items.Vitric
{
    public class VitricBulletItem : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Bullet"); //needs a better name I think
            Tooltip.SetDefault("Causes crystals to grow out of hit enemies\nStrike crystals to shatter them, dealing damage");
        }

        public override void SetDefaults()
        {
            Item.width = Item.height = 14;

            Item.value = Item.sellPrice(copper: 12);
            Item.rare = ItemRarityID.Green;

            Item.maxStack = 999;
            Item.damage = 6;
            Item.knockBack = 1f;

            Item.ammo = AmmoID.Bullet;
            Item.consumable = true;

            Item.DamageType = DamageClass.Ranged;
            Item.shoot = ModContent.ProjectileType<VitricBulletProjectile>();
            Item.shootSpeed = 2f;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe(70);
            recipe.AddIngredient(ModContent.ItemType<VitricOre>(), 4);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }
    }
    internal class VitricBulletProjectile : ModProjectile
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Bullet");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 6;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;

            Projectile.timeLeft = 240;
            Projectile.penetrate = 1;

            Projectile.extraUpdates = 2;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (projectile.active && projectile.owner == Projectile.owner && projectile.type == ModContent.ProjectileType<VitricBulletCrystalProjectile>() && projectile.ModProjectile is VitricBulletCrystalProjectile && projectile.Distance(Projectile.Center) < 20f && projectile.timeLeft < 240)
                {
                    if (projectile.ModProjectile is VitricBulletCrystalProjectile crystal)
                        crystal.hitByBullet = true;

                    projectile.Kill();
                    return;
                }
            }

            if (Main.rand.NextBool(2) && Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<VitricBulletCrystalProjectile>()] < 6)
            {
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), target.position, Vector2.Zero, ModContent.ProjectileType<VitricBulletCrystalProjectile>(), Projectile.damage, 0f, Projectile.owner);

                proj.rotation = Projectile.rotation + 3.14f + Main.rand.NextFloat(-1f, 1f);

                if (proj.ModProjectile is VitricBulletCrystalProjectile Crystal)
                {
                    Vector2 Offset = Projectile.direction == -1 ? Projectile.velocity * 0.5f : -Projectile.velocity * 1.25f;
                    Crystal.offset = Projectile.position - target.position;
                    Crystal.offset += Offset;
                    Crystal.enemyID = target.whoAmI;
                }
            }
        }
    }

    internal class VitricBulletCrystalProjectile : ModProjectile
    {
        internal bool hitByBullet = false;

        internal int enemyID;

        internal Vector2 offset = Vector2.Zero;

        public override string Texture => AssetDirectory.VitricItem + Name;
        public override bool? CanDamage() => false;

        public override void Load()
        {
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore1");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore2");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore3");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore4");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore5");
            GoreLoader.AddGoreFromTexture<SimpleModGore>(Mod, Texture + "_Gore6");
        }
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Vitric Crystal");
            Main.projFrames[Projectile.type] = 3;
        }

        public override void SetDefaults()
        {
            Projectile.penetrate = -1;
            Projectile.width = Projectile.height = 12;

            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;

            Projectile.tileCollide = false;
            Projectile.netUpdate = true;

            Projectile.frame = Main.rand.Next(3);

            Projectile.timeLeft = 300;

            Projectile.scale = 0.4f;
        }

        public override void AI()
        {
            NPC target = Main.npc[enemyID];
            Projectile.position = target.position + offset;

            if (!target.active)
                Projectile.Kill();

            if (Projectile.timeLeft < 100)
                Projectile.alpha += 5;

            if (Projectile.alpha >= 255)
                Projectile.Kill();
            if (Projectile.timeLeft > 240)
                Projectile.scale += 0.01f;
        }

        public override void Kill(int timeLeft)
        {
            if (hitByBullet && Projectile.timeLeft < 240)
            {
                switch (Projectile.frame)
                {
                    case 0:
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore1").Type).timeLeft = 60;
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore2").Type).timeLeft = 60;
                        break;
                    case 1:
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore3").Type).timeLeft = 60;
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore4").Type).timeLeft = 60;
                        break;
                    case 2:
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore5").Type).timeLeft = 60;
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore6").Type).timeLeft = 60;
                        break;
                }

                SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<VitricBulletCrystalExplosion>(), Projectile.damage, 0f, Projectile.owner);
                CameraSystem.Shake += 1;
            }     
        }
        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.WritePackedVector2(offset);
            writer.Write(enemyID);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            offset = reader.ReadPackedVector2();
            enemyID = reader.ReadInt32();
        }
    }

    internal class VitricBulletCrystalExplosion : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        private float Progress => 1 - (Projectile.timeLeft / 5f);

        private float Radius => 35 * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 5;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosion");
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 line = targetHitbox.Center.ToVector2() - Projectile.Center;
            line.Normalize();
            line *= Radius;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + line))
            {
                return true;
            }
            return false;
        }
    }
}
