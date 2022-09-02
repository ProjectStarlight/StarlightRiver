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
        private List<Vector2> cache;
        private Trail trail;
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

            Projectile.extraUpdates = 1;

            Projectile.aiStyle = 1;
            AIType = ProjectileID.Bullet;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();

            if (!Main.dedServ)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile projectile = Main.projectile[i];

                if (projectile.active && projectile.owner == Projectile.owner && projectile.type == ModContent.ProjectileType<VitricBulletCrystalProjectile>() && projectile.ModProjectile is VitricBulletCrystalProjectile && projectile.Distance(Projectile.Center) < 25f && projectile.timeLeft < 240)
                {
                    if (projectile.ModProjectile is VitricBulletCrystalProjectile crystal)
                        crystal.hitByBullet = true;

                    projectile.Kill();
                    return;
                }
            }

            if (Main.rand.NextBool(2) && Main.player[Projectile.owner].ownedProjectileCounts[ModContent.ProjectileType<VitricBulletCrystalProjectile>()] < 6 && Main.myPlayer == Projectile.owner)
            {
                Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromAI(), target.position, Vector2.Zero, ModContent.ProjectileType<VitricBulletCrystalProjectile>(), Projectile.damage, 0f, Projectile.owner);

                proj.rotation = Projectile.rotation + 3.14f + Main.rand.NextFloat(-1f, 1f);

                if (proj.ModProjectile is VitricBulletCrystalProjectile Crystal)
                {
                    Vector2 Offset = Projectile.direction == -1 ? Projectile.velocity * 0.5f : -Projectile.velocity * 0.5f;
                    Crystal.offset = Projectile.position - target.position;
                    Crystal.offset += Offset;
                    Crystal.enemyID = target.whoAmI;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Projectile.timeLeft * -0.06f);
            effect.Parameters["repeats"].SetValue(1);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/LightningTrail").Value);

            trail?.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            return true;
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 13; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            cache.Add(Projectile.Center);

            while (cache.Count > 13)
            {
                cache.RemoveAt(0);
            }

        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 13, new TriangularTip(4), factor => 3, factor =>
            {
                return new Color(70, 178, 201) * 0.5f * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Projectile.velocity;
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
            Projectile.width = 12;
            Projectile.height = 8;

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
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore1").Type).timeLeft = 45;
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore2").Type).timeLeft = 45;
                        break;
                    case 1:
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore3").Type).timeLeft = 45;
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore4").Type).timeLeft = 45;
                        break;
                    case 2:
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore5").Type).timeLeft = 45;
                        Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, Mod.Find<ModGore>(Name + "_Gore6").Type).timeLeft = 45;
                        break;
                }

                SoundEngine.PlaySound(SoundID.Shatter, Projectile.position);

                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<VitricBulletCrystalExplosion>(), Projectile.damage, 0f, Projectile.owner, enemyID);
                CameraSystem.Shake += 1;

                for (int i = 0; i < 6; i++)
                {
                    Dust.NewDustDirect(Projectile.Center, 2, 2, ModContent.DustType<VitricShardDust>(), 0f, 0f, 0, default, Main.rand.NextFloat(0.6f, 0.85f));
                }
            }     
        }

        /*public override bool PreDraw(ref Color lightColor) //this drawcode draws offset and weird so if someone better with custom drawing could fix dis plz <3
        {
            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;

            SpriteEffects spriteEffects = Projectile.direction == 1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

            int frameHeight = texture.Height / Main.projFrames[Projectile.type];
            int startY = frameHeight * Projectile.frame;

            Rectangle frameRect = new Rectangle(0, startY, texture.Width, frameHeight);

            Color drawColor = Projectile.GetAlpha(lightColor) * (1 - (Projectile.alpha / 255));

            float rotationOffset = -1.25f;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frameRect, drawColor, Projectile.rotation + rotationOffset, texture.Size() / 2f,
                Projectile.scale * 0.55f, spriteEffects, 0);

            rotationOffset = -0.75f;

            Main.EntitySpriteDraw(texture, Projectile.Center - Main.screenPosition, frameRect, drawColor, Projectile.rotation + rotationOffset, texture.Size() / 2f,
                Projectile.scale * 0.65f, spriteEffects, 0);
            return true;
        }*/

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
        private ref float NPCWhoAmI => ref Projectile.ai[0];

        private float Progress => 1 - (Projectile.timeLeft / 5f);

        private float Radius => 35 * (float)Math.Sqrt(Math.Sqrt(Progress));

        public override bool? CanHitNPC(NPC target) => target.whoAmI == NPCWhoAmI; //only hit the npc that the crystal was on

        public override string Texture => AssetDirectory.Invisible;

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

    public class VitricShardDust : ModDust
    {
        public override string Texture => AssetDirectory.Dust + "VitricShard";

        public override void OnSpawn(Dust dust)
        {
            dust.noGravity = true;
            dust.noLight = false;
        }

        public override bool Update(Dust dust)
        {
            dust.position += dust.velocity;
            dust.velocity.Y += 0.15f;
            dust.rotation += 0.1f;
            dust.scale *= 0.98f;
            if (dust.scale <= 0.2)
                dust.active = false;
            return false;
        }
    }
}
