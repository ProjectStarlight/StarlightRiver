using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Hell;
using StarlightRiver.Core;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Vitric
{
    public class MagmiteVacpack : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            Tooltip.SetDefault("Blasts out Magmites that stick to enemies\nFor each Magmite an enemy has stuck on them, they take 10 damage per second, and 3 summon tag damage, up to a maximum of three Magmites\nMagmites bounce off, and deal 50% more damage to enemies with the max amount of Magmites");
        }

        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 20;

            Item.rare = ItemRarityID.Orange;
            Item.value = Item.sellPrice(gold: 4, silver: 75);

            Item.damage = 28;
            Item.DamageType = DamageClass.Summon;
            Item.useTime = Item.useAnimation = 35;

            Item.useStyle = ItemUseStyleID.Shoot;
            Item.shoot = ModContent.ProjectileType<MagmiteVacpackHoldout>();
            Item.shootSpeed = 17f;

            Item.noMelee = true;
            Item.autoReuse = true;

            Item.noUseGraphic = true;
            Item.channel = true;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10f, 0f);
        }
    }

    //literally just for tank drawing lol
    internal class MagmiteVacpackPlayer : ModPlayer
    {
        public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
        {
            if (drawInfo.shadow != 0f)
                return;

            Texture2D tankTexture = ModContent.Request<Texture2D>(AssetDirectory.VitricItem + "MagmiteVacpack_Tank").Value;

            Player drawplayer = drawInfo.drawPlayer;

            Item heldItem = drawplayer.HeldItem;

            if (drawplayer.HeldItem.type == ModContent.ItemType<MagmiteVacpack>() && !drawplayer.frozen && !drawplayer.dead && (!drawplayer.wet || !heldItem.noWet) && drawplayer.wings <= 0)
            {
                SpriteEffects spriteEffects = drawplayer.direction == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

                Vector2 drawPos = new Vector2(((int)(drawplayer.position.X - Main.screenPosition.X + (drawplayer.width / 2) - (9 * drawplayer.direction))),
                    ((int)(drawplayer.position.Y - Main.screenPosition.Y + (drawplayer.height / 2) + 2f * drawplayer.gravDir - 2f * drawplayer.gravDir)));

                DrawData tankData = new DrawData(tankTexture, drawPos, new Rectangle?(new Rectangle(0, 0, tankTexture.Width, tankTexture.Height)),
                    drawInfo.colorArmorBody, drawplayer.bodyRotation, tankTexture.Size() / 2f, 1f, spriteEffects, 0);

                drawInfo.DrawDataCache.Add(tankData);
            }
        }
    }

    internal class MagmiteVacpackHoldout : ModProjectile
    {
        private int time;
        public ref float MaxFramesTillShoot => ref Projectile.ai[1];

        public ref float ShootDelay => ref Projectile.ai[0];

        public Player owner => Main.player[Projectile.owner];

        public bool CanHold => owner.channel && !owner.CCed && !owner.noItems;

        public override string Texture => AssetDirectory.VitricItem + "MagmiteVacpack";

        public override bool? CanDamage() => false;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Magmite Vacpack");
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Summon;
            Projectile.width = 54;
            Projectile.height = 26;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            time++;

            ShootDelay++;

            Vector2 armPos = owner.RotatedRelativePoint(owner.MountedCenter, true);
            armPos += Utils.SafeNormalize(Projectile.velocity, owner.direction * Vector2.UnitX) * 15f;

            Vector2 barrelPos = armPos + Projectile.velocity * Projectile.width * 0.5f;
            barrelPos.Y -= 8;
            barrelPos.X += -8;

            if (MaxFramesTillShoot == 0f)
                MaxFramesTillShoot = owner.HeldItem.useAnimation;

            if (!CanHold)
                Projectile.Kill();

            if (ShootDelay >= MaxFramesTillShoot)
            {
                Item heldItem = owner.HeldItem;
                int damage = Projectile.damage;
                float shootSpeed = heldItem.shootSpeed;
                float knockBack = owner.GetWeaponKnockback(heldItem, heldItem.knockBack);
                Vector2 shootVelocity = Utils.SafeNormalize(Projectile.velocity, Vector2.UnitY) * shootSpeed;
                if (Main.myPlayer == Projectile.owner)
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), barrelPos, shootVelocity, ModContent.ProjectileType<MagmiteVacpackProjectile>(), damage, knockBack, owner.whoAmI);
                
                for (int i = 0; i < 10; i++)
                {
                    Gore.NewGore(Projectile.GetSource_FromThis(), barrelPos, (shootVelocity * Main.rand.NextFloat(0.15f, 0.25f)).RotatedByRandom(MathHelper.ToRadians(20f)),
                        Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.8f, 0.95f));

                    Dust.NewDustPerfect(barrelPos, ModContent.DustType<Dusts.FireDust2>(), (shootVelocity * Main.rand.NextFloat(0.25f, 0.35f)).RotatedByRandom(MathHelper.ToRadians(15f)), 0, Color.DarkOrange, Main.rand.NextFloat(0.5f, 0.75f));
                }
                SoundEngine.PlaySound(SoundID.Splash with { PitchRange = (-0.1f, 0.1f) } with { Volume = (0.8f) }, Projectile.position);
                ShootDelay = 0;
            }

            owner.ChangeDir(Projectile.direction);
            owner.heldProj = Projectile.whoAmI;
            owner.itemTime = 2;
            owner.itemAnimation = 2;

            Projectile.timeLeft = 2;
            Projectile.rotation = Utils.ToRotation(Projectile.velocity);
            owner.itemRotation = Utils.ToRotation(Projectile.velocity * Projectile.direction);

            if (Projectile.spriteDirection == -1)
                Projectile.rotation += 3.1415927f;

            Projectile.position = armPos - Projectile.Size * 0.5f;

            Projectile.spriteDirection = Projectile.direction;

            if (Main.myPlayer == Projectile.owner)
            {
                float interpolant = Utils.GetLerpValue(5f, 25f, Projectile.Distance(Main.MouseWorld), true);

                Vector2 oldVelocity = Projectile.velocity;

                Projectile.velocity = Vector2.Lerp(Projectile.velocity, owner.DirectionTo(Main.MouseWorld), interpolant);
                if (Projectile.velocity != oldVelocity)
                {
                    Projectile.netSpam = 0;
                    Projectile.netUpdate = true;
                }
            }

            if (time < 4)
                return;

            if (Main.rand.NextBool(10))
            {
                Gore.NewGore(Projectile.GetSource_FromThis(), barrelPos, (Vector2.UnitY * Main.rand.NextFloat(0.4f, 0.5f)).RotatedByRandom(MathHelper.ToRadians(35f)), Mod.Find<ModGore>("MagmiteGore").Type, Main.rand.NextFloat(0.6f, 0.7f));
                if (Main.rand.NextBool(2))
                    Dust.NewDustPerfect(barrelPos, ModContent.DustType<Dusts.MagmaSmoke>(), (Vector2.UnitY * Main.rand.NextFloat(-3f, -2f)).RotatedByRandom(MathHelper.ToRadians(25f)), 100, Color.Black, Main.rand.NextFloat(0.7f, 0.9f));
            }

            if (Projectile.soundDelay == 0)
            {
                SoundEngine.PlaySound(SoundID.SplashWeak with { PitchRange = (-0.1f, 0.1f) }, Projectile.position);

                Projectile.soundDelay = 6;
            }
        }
    }

    internal class MagmiteVacpackProjectile : ModProjectile
    {
        internal int MaxBounces = 3;

        internal int enemyID;
        internal bool stuck = false;
        internal Vector2 offset = Vector2.Zero;

        internal Vector2 oldVelocity;

        public override Color? GetAlpha(Color lightColor) => Color.White;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bouncy Magmite");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 7;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.friendly = true;

            Projectile.Size = new Vector2(20);

            Projectile.timeLeft = 300;
            Projectile.penetrate = -1;
        }

        public override bool PreAI()
        {
            if (stuck)
            {
                NPC target = Main.npc[enemyID];
                Projectile.position = target.position + offset;
            }
            return base.PreAI();
        }

        public override void AI()
        {
            if (stuck)
            {
                Projectile.scale -= 0.0025f;    

                if (Main.npc[enemyID].active)
                    Projectile.timeLeft = 2;    
                else
                    Projectile.Kill();

                if (Main.rand.NextBool(4))
                    Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + oldVelocity + new Vector2(-5f, -5f), Projectile.velocity * 0.1f, Mod.Find<ModGore>("MagmiteGore").Type).scale = Projectile.scale;

                if (Main.rand.NextBool(20))
                    Dust.NewDustPerfect(Projectile.Center + oldVelocity, ModContent.DustType<Dusts.MagmaSmoke>(), (Vector2.UnitY * Main.rand.NextFloat(-3f, -2f)).RotatedByRandom(MathHelper.ToRadians(5f)), 100, Color.Black, Projectile.scale);
            }
            else
            {
                Projectile.rotation += 0.1f + Projectile.direction;

                Projectile.velocity.Y += 0.65f;
                if (Projectile.velocity.Y > 16f)
                    Projectile.velocity.Y = 16f;

                if (Main.rand.NextBool(4))
                    Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0.1f, Mod.Find<ModGore>("MagmiteGore").Type).scale = Main.rand.NextFloat(0.4f, 0.9f);
            }

            if (Projectile.scale < 0.3f)
                Projectile.Kill();
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            MagmiteVacpackGlobalNPC globalNPC = target.GetGlobalNPC<MagmiteVacpackGlobalNPC>();
            if (globalNPC.MagmiteAmount < 3)
            {
                stuck = true;
                Projectile.friendly = false;
                Projectile.tileCollide = false;
                enemyID = target.whoAmI;
                offset = Projectile.position - target.position;
                Projectile.netUpdate = true;

                globalNPC.MagmiteAmount++;
                globalNPC.MagmiteOwner = Projectile.owner;

                Main.player[Projectile.owner].MinionAttackTargetNPC = target.whoAmI;

                oldVelocity = Projectile.velocity;
            }
            else
            {
                MaxBounces--;

                Projectile.velocity.X *= -1f;

                Projectile.scale -= 0.1f;

                for (int i = 0; i < 10; i++)
                {
                    Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(0.1f, 0.2f), Mod.Find<ModGore>("MagmiteGore").Type);
                }

                SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.position);
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            MaxBounces--;
            if (MaxBounces <= 0)
                Projectile.Kill();

            if (Math.Abs(Projectile.velocity.X - oldVelocity.X) > float.Epsilon)
                Projectile.velocity.X = -oldVelocity.X;

            if (Math.Abs(Projectile.velocity.Y - oldVelocity.Y) > float.Epsilon)
                Projectile.velocity.Y = -oldVelocity.Y;

            Projectile.scale -= 0.1f;

            for (int i = 0; i < 10; i++)
            {
                Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.UnitY.RotatedByRandom(MathHelper.ToRadians(25f)) * Main.rand.NextFloat(4f, 5f), Mod.Find<ModGore>("MagmiteGore").Type, 1.35f);

                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.AmberBolt, 0, 0, 0, default, 0.5f);
            }

            SoundEngine.PlaySound(SoundID.SplashWeak, Projectile.position);
            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (stuck)
                return true;

            Main.instance.LoadProjectile(Projectile.type);
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;

            Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, Projectile.height * 0.5f);
            for (int k = 0; k < Projectile.oldPos.Length; k++)
            {
                Vector2 drawPos = (Projectile.oldPos[k] - Main.screenPosition) + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, null, color, Projectile.rotation, drawOrigin, Projectile.scale, SpriteEffects.None, 0);
            }

            return true;
        }

        public override void Kill(int timeLeft)
        {
            if (!stuck)
            {
                for (int i = 0; i < 15; i++)
                {
                    Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.UnitY * Main.rand.NextFloat(-8, -1)).RotatedByRandom(0.5f), Mod.Find<ModGore>("MagmiteGore").Type).scale = Main.rand.NextFloat(0.5f, 0.8f);
                }
            }
            else
            {
                for (int i = 0; i < 15; i++)
                {
                    Gore.NewGoreDirect(Projectile.GetSource_FromThis(), Projectile.Center + oldVelocity, (Vector2.UnitY * Main.rand.NextFloat(-8, -1)).RotatedByRandom(0.3f), Mod.Find<ModGore>("MagmiteGore").Type).scale = Main.rand.NextFloat(0.5f, 0.8f);
                }
                MagmiteVacpackGlobalNPC globalNPC = Main.npc[enemyID].GetGlobalNPC<MagmiteVacpackGlobalNPC>();
                globalNPC.MagmiteAmount--;
            }

            SoundEngine.PlaySound(SoundID.DD2_GoblinHurt, Projectile.Center);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            MagmiteVacpackGlobalNPC globalNPC = target.GetGlobalNPC<MagmiteVacpackGlobalNPC>();
            if (globalNPC.MagmiteAmount >= 3)
                damage = (int)(damage * 1.5);
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(stuck);
            writer.WritePackedVector2(offset);
            writer.Write(enemyID);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            stuck = reader.ReadBoolean();
            offset = reader.ReadPackedVector2();
            enemyID = reader.ReadInt32();
        }
    }

    internal class MagmiteVacpackGlobalNPC : GlobalNPC
    {
        public int MagmiteAmount;

        public int MagmiteOwner;

        public override bool InstancePerEntity => true;

        public override void ResetEffects(NPC npc)
        {
            MagmiteAmount = Utils.Clamp(MagmiteAmount, 0, 3);
        }
            
        public override void UpdateLifeRegen(NPC npc, ref int damage)
        {
            if (MagmiteAmount > 0)
            {
                if (npc.lifeRegen > 0)
                    npc.lifeRegen = 0;

                npc.lifeRegen -= 10 * MagmiteAmount;
                if (damage < 1)
                    damage = 1;
            }
        }

        public override void ModifyHitByProjectile(NPC npc, Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            Player player = Main.player[projectile.owner];

            bool IsSummoner = (projectile.minion || projectile.DamageType == DamageClass.Summon || ProjectileID.Sets.MinionShot[projectile.type] == true);

            if (projectile.owner == MagmiteOwner && projectile.friendly && IsSummoner && npc.whoAmI == player.MinionAttackTargetNPC && MagmiteAmount > 0 && player.HasMinionAttackTargetNPC)
                damage = damage + (MagmiteAmount * 3);
        }
    }
}
