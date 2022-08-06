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
using Terraria.GameContent;

namespace StarlightRiver.Content.Items.SteampunkSet
{
    public class Buzzsaw : ModItem //PORTTODO: Graydee rework this to be... not this.
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
            Item.damage = 34;
            Item.width = 65;
            Item.height = 21;
            Item.useTime = 65;
            Item.useAnimation = 65;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Green;
            Item.autoReuse = false;
            Item.shoot = ModContent.ProjectileType<BuzzsawProj>();
            Item.shootSpeed = 2f;
            Item.DamageType = DamageClass.Melee;
            Item.channel = true;
            Item.noUseGraphic = true;
            //Item.UseSound = SoundID.DD2_SkyDragonsFuryShot;
        }
        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ItemID.DemoniteBar, 10);
            recipe.AddIngredient(ModContent.ItemType<AncientGear>(), 8);
            recipe.AddTile(TileID.Anvils);

            Recipe recipe2 = CreateRecipe();
            recipe2.AddIngredient(ItemID.CrimtaneBar, 10);
            recipe2.AddIngredient(ModContent.ItemType<AncientGear>(), 8);
            recipe2.AddTile(TileID.Anvils);
        }
    }

    //TODO this would probably be cleaner with less data required for netcode if this is changed to no longer use vanilla ai to not need the phantom saw
    public class BuzzsawProj : ModProjectile
    {
        private const int OFFSET = 30;
        private const int MAXCHARGE = 20;

        public ref float Charge => ref Projectile.ai[0];
        public ref float Angle => ref Projectile.ai[1];

        float oldAngle = 0f;

        public Vector2 direction = Vector2.Zero;

        private int counter;
        private float bladeRotation;
        private bool released = false;
        private float flickerTime = 0;

        //we keep track of when the saw hits so that we can show the gores in multiPlayer
        private bool justHit = false;

        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Steamsaw");

        public override void SetDefaults()
        {
            Projectile.hostile = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 999999;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Main.projFrames[Projectile.type] = 5;
        }

        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];

            if (Charge >= MAXCHARGE)
            {
                if (flickerTime == 0)
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.NPCDeath7, Projectile.Center);
                flickerTime++;
            }
            else
                Charge += 0.1f;

            Projectile.velocity = Vector2.Zero;
            Projectile.timeLeft = 2;
            Player.itemTime = 5; // Set Item time to 2 frames while we are used
            Player.itemAnimation = 5; // Set Item animation time to 2 frames while we are used

            float shake = 0;

            if (Player.channel && !released)
            {
                if (Projectile.owner == Main.myPlayer)
                {
                    Angle = (Main.MouseWorld - (Player.Center)).ToRotation();

                    if (Math.Abs(oldAngle - Angle) > 0.1f) //only send a netupdate if the buzzsaw has rotated visibly
                    {
                        oldAngle = Angle;
                        Projectile.netUpdate = true;
                    }
                }

                direction = Angle.ToRotationVector2();

                bladeRotation += 1.2f;
                Player.ChangeDir(direction.X > 0 ? 1 : -1);
                shake = MathHelper.Lerp(0.04f, 0.15f, Charge / (float)MAXCHARGE);

                counter++;
                Projectile.frame = ((counter / 5) % 2) + 2;

                if (counter % 30 == 1)
                    Terraria.Audio.SoundEngine.PlaySound(SoundID.Item22, Projectile.Center); //Chainsaw sound

                ReleaseSteam(Player);
            }
            else
            {
                Projectile.friendly = false;
                Projectile.frame = 5;

                if (!released)
                    LaunchSaw(Player);
                else if (Player.ownedProjectileCounts[ModContent.ProjectileType<BuzzsawProj2>()] == 0)
                    Projectile.active = false;
            }



            Projectile.Center = Player.Center + (direction * OFFSET * Main.rand.NextFloat(1 - shake, 1 + shake));
            Projectile.velocity = Vector2.Zero;
            Player.itemRotation = direction.ToRotation();

            if (Player.direction != 1)
                Player.itemRotation -= 3.14f;

            Player.itemRotation = MathHelper.WrapAngle(Player.itemRotation);

            Player.heldProj = Projectile.whoAmI;

            if (justHit && Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer != Projectile.owner)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC NPC = Main.npc[i];
                    if (NPC.active && NPC.Hitbox.Intersects(Projectile.Hitbox))
                        hitGore(NPC);
                }
            }

            justHit = false;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (Charge < MAXCHARGE)
                Charge++;
            Projectile.netUpdate = true;
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
                        Dust.NewDustPerfect((Projectile.Center + (direction * 10)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(3, 20), 0, new Color(255, 255, 60) * 0.8f, 1.6f);
                    }

                    Dust.NewDustPerfect((Projectile.Center + (direction * 10)), ModContent.DustType<Dusts.Glow>(), direction.RotatedBy(Main.rand.NextFloat(-0.35f, 0.35f) - 1.57f) * Main.rand.Next(3, 10), 0, new Color(150, 80, 30), 0.2f);
                }
                else
                {
                    for (int j = 0; j < 15; j++)
                    {
                        Dust.NewDustPerfect(Projectile.Center + (direction * 15), DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * Main.rand.NextFloat(0f, 6f), 0, default, 1.5f);
                        Dust.NewDustPerfect(Projectile.Center + (direction * 15), DustID.Blood, direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.NextFloat(0f, 3f), 0, default, 0.8f);
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

        public override bool PreDraw(ref Color lightColor) //extremely messy code I ripped from a weapon i made for spirit :trollge:
        {
            Color heatColor = new Color(255, 96, 0);
            lightColor = Color.Lerp(lightColor, heatColor, (Charge / (float)MAXCHARGE) * 0.6f);

            Player Player = Main.player[Projectile.owner];
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Texture2D texture2 = ModContent.Request<Texture2D>(Texture + "_Blade").Value;
            int height1 = texture.Height;
            int height2 = texture2.Height / Main.projFrames[Projectile.type];
            int y2 = height2 * Projectile.frame;
            Vector2 origin = new Vector2((float)texture.Width / 2f, (float)height1 / 2f);
            Vector2 position = (Projectile.position - (0.5f * (direction * OFFSET)) + new Vector2((float)Projectile.width, (float)Projectile.height) / 2f + Vector2.UnitY * Projectile.gfxOffY - Main.screenPosition).Floor();

            if (!released)
                Main.spriteBatch.Draw(texture2, Projectile.Center - Main.screenPosition, new Rectangle(0, y2, texture2.Width, height2), lightColor, bladeRotation, new Vector2(15, 15), Projectile.scale, SpriteEffects.None, 0.0f);

            if (Player.direction == 1)
            {
                SpriteEffects effects1 = SpriteEffects.None;
                Main.spriteBatch.Draw(texture, position, null, lightColor, direction.ToRotation(), origin, Projectile.scale, effects1, 0.0f);

            }
            else
            {
                SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
                Main.spriteBatch.Draw(texture, position, null, lightColor, direction.ToRotation() - 3.14f, origin, Projectile.scale, effects1, 0.0f);
            }

            if (Charge >= MAXCHARGE && !released && flickerTime < 16)
            {
                texture = ModContent.Request<Texture2D>(Texture + "_White").Value;
                texture2 = ModContent.Request<Texture2D>(Texture + "_Blade_White").Value;
                Color color = Color.White;
                float flickerTime2 = (float)(flickerTime / 20f);
                float alpha = 1.5f - (((flickerTime2 * flickerTime2) / 2) + (2f * flickerTime2));

                if (alpha < 0)
                    alpha = 0;

                Main.spriteBatch.Draw(texture2, Projectile.Center - Main.screenPosition, new Rectangle(0, y2, texture2.Width, height2), color * alpha, bladeRotation, new Vector2(15, 15), Projectile.scale, SpriteEffects.None, 0.0f);

                if (Player.direction == 1)
                {
                    SpriteEffects effects1 = SpriteEffects.None;
                    Main.spriteBatch.Draw(texture, position, null, color * alpha, direction.ToRotation(), origin, Projectile.scale, effects1, 0.0f);

                }
                else
                {
                    SpriteEffects effects1 = SpriteEffects.FlipHorizontally;
                    Main.spriteBatch.Draw(texture, position, null, color * alpha, direction.ToRotation() - 3.14f, origin, Projectile.scale, effects1, 0.0f);
                }
            }

            return false;
        }

        private void LaunchSaw(Player Player)
        {
            released = true;
            if (Main.myPlayer == Player.whoAmI)
            {
                float speed = MathHelper.Lerp(8f, 12f, Charge / (float)MAXCHARGE);
                float damageMult = MathHelper.Lerp(0.85f, 2f, Charge / (float)MAXCHARGE);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, direction * speed, ModContent.ProjectileType<BuzzsawProj2>(), (int)(Projectile.damage * damageMult), Projectile.knockBack, Projectile.owner);
            }
        }

        private void ReleaseSteam(Player Player)
        {
            float alphaMult = MathHelper.Lerp(0.75f, 3f, Charge / (float)MAXCHARGE);
            Dust.NewDustPerfect(Vector2.Lerp(Projectile.Center, Player.Center, 0.75f), ModContent.DustType<Dusts.BuzzsawSteam>(), new Vector2(0.2f, -Main.rand.NextFloat(0.7f, 1.6f)), (int)(Main.rand.Next(15) * alphaMult), Color.White, Main.rand.NextFloat(0.2f, 0.5f));
        }
    }

    public class BuzzsawProj2 : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        private float rotationCounter;

        private Vector2 oldVel;

        private Player Player => Main.player[Projectile.owner];

        public bool justLaunched = true;

        public bool justHit = false;
        public short pauseTimer = -1;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Buzzsaw");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.aiStyle = 3;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 700;
            Main.projFrames[Projectile.type] = 2;
            Projectile.extraUpdates = 1;
        }

        public override bool PreAI()
        {
            if (--pauseTimer > 0)
            {
                if (Projectile.velocity != Vector2.Zero)
                    oldVel = Projectile.velocity;

                Projectile.velocity = Vector2.Zero;
                return false;
            }

            if (pauseTimer == 0)
                Projectile.velocity = oldVel;

            return true;
        }

        public override void AI()
        {
            if (justLaunched && Main.myPlayer == Projectile.owner)
            {
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<PhantomBuzzsaw>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
                ((PhantomBuzzsaw)Main.projectile[proj].ModProjectile).parent = Projectile;
                justLaunched = false;
            }

            Projectile.frameCounter += 1;
            Projectile.frame = (Projectile.frameCounter / 5) % 2;
            rotationCounter += 0.6f;
            Projectile.rotation = rotationCounter;

        }

        public void hitGore(NPC target)
        {
            Vector2 direction = target.Center - Projectile.Center;
            direction.Normalize();
            for (int i = 0; i < 2; i++)
            {

                if (!Helper.IsFleshy(target))
                    Dust.NewDustPerfect((Projectile.Center + (direction * 10)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.3f, 0.3f) + 1.57f) * Main.rand.Next(15, 20), 0, new Color(255, 230, 60) * 0.8f, 1.6f);
                else
                {
                    Helper.PlayPitched("Impacts/StabTiny", 0.8f, Main.rand.NextFloat(-0.3f, 0.3f), target.Center);

                    for (int j = 0; j < 2; j++)
                        Dust.NewDustPerfect(Projectile.Center + (direction * 15), ModContent.DustType<GraveBlood>(), direction.RotatedBy(Main.rand.NextFloat(-0.6f, 0.6f) + 3.14f) * Main.rand.NextFloat(0.5f, 5f));
                }

            }
        }

    }
    public class PhantomBuzzsaw : ModProjectile
    {
        public override string Texture => AssetDirectory.SteampunkItem + Name;

        public Projectile parent;

        private Player Player => Main.player[Projectile.owner];

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Buzzsaw");
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 700;
            Projectile.tileCollide = false;
            Main.projFrames[Projectile.type] = 2;
            Projectile.extraUpdates = 1;
            Projectile.hide = true;
        }

        private void findIfHit()
        {
            foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.immune[Projectile.owner] <= 0 && n.Hitbox.Intersects(Projectile.Hitbox)))
            {
                OnHitNPC(NPC, 0, 0, false);
            }
        }

        public override void AI()
        {
            if (parent is null)
            {
                //have to find the parent in mp note that Projectile arrays are NOT synced like NPC and Player arrays so we can't use the index in ai[0]
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == this.Projectile.owner && proj.type == ModContent.ProjectileType<BuzzsawProj2>())
                    {
                        parent = proj;
                        break;
                    }
                }
            }

            if (parent is null || !parent.active)
            {
                Projectile.active = false;
                return;
            }


            if (Main.myPlayer != Projectile.owner)
                findIfHit();

            Projectile.Center = parent.Center;
            Projectile.velocity = parent.velocity;


        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {

            if (Main.myPlayer == Projectile.owner)
            {
                Vector2 direction = target.Center - Projectile.Center;
                if (Helper.IsFleshy(target))
                {
                    int bloodID = ModContent.ProjectileType<BuzzsawBlood1>();
                    int spriteDirection = Math.Sign(direction.X);

                    Projectile proj = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, bloodID, 0, 0, Projectile.owner);
                    proj.spriteDirection = -spriteDirection;
                }

                Core.Systems.CameraSystem.Shake += 6;
            }


            target.immune[Projectile.owner] = 20;

            if (parent.ModProjectile is BuzzsawProj2 modProj)
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
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 9;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 90;
            Projectile.height = 90;
            Projectile.friendly = false;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 700;
            Projectile.rotation = Main.rand.NextFloat(0.78f);
            SetFrames();
        }

        public override void AI()
        {
            if (Projectile.ai[0]++ == 0)
                Projectile.position -= new Vector2(-Projectile.spriteDirection * 20, 28).RotatedBy(Projectile.rotation);
            Projectile.velocity = Vector2.Zero;
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame >= Main.projFrames[Projectile.type])
                    Projectile.active = false;
            }
        }
        protected virtual void SetFrames()
        {
            Main.projFrames[Projectile.type] = 3;
        }
    }
}