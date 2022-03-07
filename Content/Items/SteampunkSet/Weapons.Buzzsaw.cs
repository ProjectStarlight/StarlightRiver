using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using System.IO;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class Buzzsaw : ModItem
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override Vector2? HoldoutOffset() => new Vector2(-15, 0);

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Steamsaw");
            Tooltip.SetDefault("Strike enemies to build up pressure\nRelease to vent the pressure, launching the sawblade\n'The right tool for the wrong job'");
        }

        public override void SetDefaults()
        {
            item.damage = 34;
            item.width = 65;
            item.height = 21;
            item.useTime = 65;
            item.useAnimation = 65;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.knockBack = 1.5f;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = 3;
            item.autoReuse = false;
            item.shoot = ModContent.ProjectileType<BuzzsawProj>();
            item.shootSpeed = 2f;
            item.melee = true;
            item.channel = true;
            item.noUseGraphic = true;
            //item.UseSound = SoundID.DD2_SkyDragonsFuryShot;
        }
    }

    //TODO this would probably be cleaner with less data required for netcode if this is changed to no longer use vanilla ai to not need the phantom saw
    public class BuzzsawProj : ModProjectile
    {
        private const int OFFSET = 30;
        private const int MAXCHARGE = 20;

        public ref float Charge => ref projectile.ai[0];
        public ref float Angle => ref projectile.ai[1];

        float oldAngle = 0f;

        public Vector2 direction = Vector2.Zero;

        private int counter;
        private float bladeRotation;
        private bool released = false;
        private float flickerTime = 0;

        //we keep track of when the saw hits so that we can show the gores in multiplayer
        private bool justHit = false;

        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Steamsaw");

        public override void SetDefaults()
        {
            projectile.hostile = false;
            projectile.melee = true;
            projectile.width = 32;
            projectile.height = 32;
            projectile.aiStyle = -1;
            projectile.friendly = true;
            projectile.penetrate = -1;
            projectile.tileCollide = false;
            projectile.timeLeft = 999999;
            projectile.ignoreWater = true;
            projectile.alpha = 255;
            Main.projFrames[projectile.type] = 5;
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (Charge >= MAXCHARGE)
            {
                if (flickerTime == 0)
                    Main.PlaySound(SoundID.NPCDeath7, projectile.Center);
                flickerTime++;
            }

            projectile.velocity = Vector2.Zero;
            projectile.timeLeft = 2;
            player.itemTime = 5; // Set item time to 2 frames while we are used
            player.itemAnimation = 5; // Set item animation time to 2 frames while we are used

            float shake = 0;

            if (player.channel && !released)
            {
                if (projectile.owner == Main.myPlayer)
                {
                    Angle = (Main.MouseWorld - (player.Center)).ToRotation();

                    if (Math.Abs(oldAngle - Angle) > 0.1f) //only send a netupdate if the buzzsaw has rotated visibly
                    {
                        oldAngle = Angle;
                        projectile.netUpdate = true;
                    }
                }

                direction = Angle.ToRotationVector2();

                bladeRotation += 1.2f;
                player.ChangeDir(direction.X > 0 ? 1 : -1);
                shake = MathHelper.Lerp(0.04f, 0.15f, Charge / (float)MAXCHARGE);

                counter++;
                projectile.frame = ((counter / 5) % 2) + 2;

                if (counter % 30 == 1)
                    Main.PlaySound(2, projectile.Center, 22); //Chainsaw sound

                ReleaseSteam(player);
            }
            else
            {
                projectile.friendly = false;
                projectile.frame = 5;

                if (!released)
                    LaunchSaw(player);
                else if (player.ownedProjectileCounts[ModContent.ProjectileType<BuzzsawProj2>()] == 0)
                    projectile.active = false;
            }



            projectile.Center = player.Center + (direction * OFFSET * Main.rand.NextFloat(1 - shake, 1 + shake));
            projectile.velocity = Vector2.Zero;
            player.itemRotation = direction.ToRotation();

            if (player.direction != 1)
                player.itemRotation -= 3.14f;

            player.itemRotation = MathHelper.WrapAngle(player.itemRotation);

            player.heldProj = projectile.whoAmI;

            if (justHit && Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer != projectile.owner)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC npc = Main.npc[i];
                    if (npc.active && npc.Hitbox.Intersects(projectile.Hitbox))
                        hitGore(npc);
                }
            }

            justHit = false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Charge < MAXCHARGE)
                Charge++;
            projectile.netUpdate = true;
            justHit = true;
            hitGore(target);
        }

        public void hitGore(NPC target)
        {
            for (int i = 0; i < 2; i++)
            {
                if (!Helper.IsFleshy(target))
                {
                    for (int k = 0; k < 10; k++)
                    {
                        Dust.NewDustPerfect((projectile.Center + (direction * 10)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(3, 20), 0, new Color(255, 255, 60) * 0.8f, 1.6f);
                    }

                    Dust.NewDustPerfect((projectile.Center + (direction * 10)), ModContent.DustType<Dusts.Glow>(), direction.RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f) - 1.57f) * Main.rand.Next(3, 10), 0, new Color(150, 80, 30), 0.2f);
                }
                else
                {
                    for (int j = 0; j < 15; j++)
                    {
                        Dust.NewDustPerfect(projectile.Center + (direction * 15), DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * Main.rand.NextFloat(0f, 6f), 0, default, 1.5f);
                        Dust.NewDustPerfect(projectile.Center + (direction * 15), DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.NextFloat(0f, 3f), 0, default, 0.8f);
                    }
                }
            }
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(justHit);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            justHit = reader.ReadBoolean();
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) //extremely messy code I ripped from a weapon i made for spirit :trollge:
        {
            Color heatColor = new Color(255, 96, 0);
            lightColor = Color.Lerp(lightColor, heatColor, (Charge / (float)MAXCHARGE) * 0.6f);

            Player player = Main.player[projectile.owner];
            Texture2D texture = Main.projectileTexture[projectile.type];
            Texture2D texture2 = ModContent.GetTexture(Texture + "_Blade");
            int height1 = texture.Height;
            int height2 = texture2.Height / Main.projFrames[projectile.type];
            int y2 = height2 * projectile.frame;
            Vector2 origin = new Vector2((float)texture.Width / 2f, (float)height1 / 2f);
            Vector2 position = (projectile.position - (0.5f * (direction * OFFSET)) + new Vector2((float)projectile.width, (float)projectile.height) / 2f + Vector2.UnitY * projectile.gfxOffY - Main.screenPosition).Floor();

            if (!released)
                spriteBatch.Draw(texture2, projectile.Center - Main.screenPosition, new Rectangle(0, y2, texture2.Width, height2), lightColor, bladeRotation, new Vector2(15, 15), projectile.scale, SpriteEffects.None, 0.0f);

            if (player.direction == 1)
            {
                SpriteEffects effects1 = SpriteEffects.None;
                spriteBatch.Draw(texture, position, null, lightColor, direction.ToRotation(), origin, projectile.scale, effects1, 0.0f);

            }
            else
            {
                SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
                spriteBatch.Draw(texture, position, null, lightColor, direction.ToRotation() - 3.14f, origin, projectile.scale, effects1, 0.0f);
            }

            if (Charge >= MAXCHARGE && !released && flickerTime < 16)
            {
                texture = ModContent.GetTexture(Texture + "_White");
                texture2 = ModContent.GetTexture(Texture + "_Blade_White");
                Color color = Color.White;
                float flickerTime2 = (float)(flickerTime / 20f);
                float alpha = 1.5f - (((flickerTime2 * flickerTime2) / 2) + (2f * flickerTime2));

                if (alpha < 0)
                    alpha = 0;

                spriteBatch.Draw(texture2, projectile.Center - Main.screenPosition, new Rectangle(0, y2, texture2.Width, height2), color * alpha, bladeRotation, new Vector2(15, 15), projectile.scale, SpriteEffects.None, 0.0f);

                if (player.direction == 1)
                {
                    SpriteEffects effects1 = SpriteEffects.None;
                    spriteBatch.Draw(texture, position, null, color * alpha, direction.ToRotation(), origin, projectile.scale, effects1, 0.0f);

                }
                else
                {
                    SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
                    spriteBatch.Draw(texture, position, null, color * alpha, direction.ToRotation() - 3.14f, origin, projectile.scale, effects1, 0.0f);
                }
            }

            return false;
        }

        private void LaunchSaw(Player player)
        {
            released = true;
            if (Main.myPlayer == player.whoAmI)
            {
                float speed = MathHelper.Lerp(8f, 12f, Charge / (float)MAXCHARGE);
                float damageMult = MathHelper.Lerp(0.85f, 2f, Charge / (float)MAXCHARGE);
                Projectile.NewProjectile(projectile.Center, direction * speed, ModContent.ProjectileType<BuzzsawProj2>(), (int)(projectile.damage * damageMult), projectile.knockBack, projectile.owner);
            }
        }

        private void ReleaseSteam(Player player)
        {
            float alphaMult = MathHelper.Lerp(0.75f, 3f, Charge / (float)MAXCHARGE);
            Dust.NewDustPerfect(Vector2.Lerp(projectile.Center, player.Center, 0.75f), ModContent.DustType<Dusts.BuzzsawSteam>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), (int)(Main.rand.Next(15) * alphaMult), Color.White, Main.rand.NextFloat(0.2f, 0.5f));
        }
    }

    public class BuzzsawProj2 : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private float rotationCounter;

        private Vector2 oldVel;

        private Player player => Main.player[projectile.owner];

        public bool justLaunched = true;

        public bool justHit = false;
        public short pauseTimer = -1;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Buzzsaw");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 30;
            projectile.height = 30;
            projectile.aiStyle = 3;
            projectile.friendly = false;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 700;
            Main.projFrames[projectile.type] = 2;
            projectile.extraUpdates = 1;
        }

        public override bool PreAI()
        {
            if (--pauseTimer > 0)
            {
                if (projectile.velocity != Vector2.Zero)
                    oldVel = projectile.velocity;

                projectile.velocity = Vector2.Zero;
                return false;
            }

            if (pauseTimer == 0)
                projectile.velocity = oldVel;

            return true;
        }

        public override void AI()
        {
            if (justLaunched && Main.myPlayer == projectile.owner)
            {
                int proj = Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<PhantomBuzzsaw>(), projectile.damage, projectile.knockBack, projectile.owner);
                ((PhantomBuzzsaw)Main.projectile[proj].modProjectile).parent = projectile;
                justLaunched = false;
            }

            projectile.frameCounter += 1;
            projectile.frame = (projectile.frameCounter / 5) % 2;
            rotationCounter += 0.6f;
            projectile.rotation = rotationCounter;

        }

        public void hitGore(NPC target)
        {
            Vector2 direction = target.Center - projectile.Center;
            direction.Normalize();
            for (int i = 0; i < 2; i++)
            {

                if (!Helper.IsFleshy(target))
                    Dust.NewDustPerfect((projectile.Center + (direction * 10)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f) + 1.57f) * Main.rand.Next(15, 20), 0, new Color(255, 230, 60) * 0.8f, 1.6f);
                else
                {
                    Helper.PlayPitched("Impacts/StabTiny", 0.8f, Main.rand.NextFloat(-0.3f, 0.3f), target.Center);

                    for (int j = 0; j < 2; j++)
                        Dust.NewDustPerfect(projectile.Center + (direction * 15), ModContent.DustType<GraveBlood>(), direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * Main.rand.NextFloat(0.5f, 5f));
                }

            }
        }

    }
    public class PhantomBuzzsaw : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public Projectile parent;

        private Player player => Main.player[projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Buzzsaw");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 30;
            projectile.height = 30;
            projectile.friendly = true;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 700;
            projectile.tileCollide = false;
            Main.projFrames[projectile.type] = 2;
            projectile.extraUpdates = 1;
            projectile.hide = true;
        }

        private void findIfHit()
        {
            foreach (NPC npc in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.immune[projectile.owner] <= 0 && n.Hitbox.Intersects(projectile.Hitbox)))
            {
                OnHitNPC(npc, 0, 0, false);
            }
        }

        public override void AI()
        {
            if (parent is null)
            {
                //have to find the parent in mp note that projectile arrays are NOT synced like npc and player arrays so we can't use the index in ai[0]
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == this.projectile.owner && proj.type == ModContent.ProjectileType<BuzzsawProj2>())
                    {
                        parent = proj;
                        break;
                    }
                }
            }

            if (parent is null || !parent.active)
            {
                projectile.active = false;
                return;
            }


            if (Main.myPlayer != projectile.owner)
                findIfHit();

            projectile.Center = parent.Center;
            projectile.velocity = parent.velocity;


        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {

            if (Main.myPlayer == projectile.owner)
            {
                Vector2 direction = target.Center - projectile.Center;
                if (Helper.IsFleshy(target))
                {
                    int bloodID = ModContent.ProjectileType<BuzzsawBlood1>();
                    int spriteDirection = Math.Sign(direction.X);

                    Projectile proj = Projectile.NewProjectileDirect(target.Center, Vector2.Zero, bloodID, 0, 0, projectile.owner);
                    proj.spriteDirection = -spriteDirection;
                }

                player.GetModPlayer<StarlightPlayer>().Shake += 6;
            }


            target.immune[projectile.owner] = 20;

            if (parent.modProjectile is BuzzsawProj2 modProj)
            {
                modProj.hitGore(target);
                modProj.pauseTimer = 16;
                modProj.justHit = true;
            }


        }
    }

    public class BuzzsawBlood1 : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Buzzsaw");
            ProjectileID.Sets.TrailCacheLength[projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            projectile.width = 90;
            projectile.height = 90;
            projectile.friendly = false;
            projectile.melee = true;
            projectile.penetrate = -1;
            projectile.timeLeft = 700;
            projectile.rotation = Main.rand.NextFloat(0.78f);
            SetFrames();
        }

        public override void AI()
        {
            if (projectile.ai[0]++ == 0)
                projectile.position -= new Vector2(-projectile.spriteDirection * 20, 28).RotatedBy(projectile.rotation);
            projectile.velocity = Vector2.Zero;
            projectile.frameCounter++;
            if (projectile.frameCounter > 4)
            {
                projectile.frameCounter = 0;
                projectile.frame++;
                if (projectile.frame >= Main.projFrames[projectile.type])
                    projectile.active = false;
            }
        }
        protected virtual void SetFrames()
        {
            Main.projFrames[projectile.type] = 3;
        }
    }
}