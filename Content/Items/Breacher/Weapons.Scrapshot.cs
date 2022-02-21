using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using System.Collections.Generic;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Bosses.VitricBoss;
using Terraria.Graphics.Effects;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.IO;

namespace StarlightRiver.Content.Items.Breacher
{
    public class Scrapshot : ModItem
    {
        public ScrapshotHook hook;

        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override bool AltFunctionUse(Player player) => true;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Scrapshot");
            Tooltip.SetDefault("Right click to hook your enemies and pull closer\nFire while hooked to reduce spread and go flying");
        }

        public override void SetDefaults()
        {
            item.width = 24;
            item.height = 28;
            item.damage = 6;
            item.useAnimation = 30;
            item.useTime = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.knockBack = 2f;
            item.rare = ItemRarityID.Orange;
            item.value = Item.sellPrice(0, 10, 0, 0);
            item.noMelee = true;
            item.useAmmo = AmmoID.Bullet;
            item.ranged = true;
            item.shoot = 0;
            item.shootSpeed = 17;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-15, 0);
        }

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                item.useTime = 14;
                item.useAnimation = 14;
                item.noUseGraphic = true;

                return !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<ScrapshotHook>());
            }
            else
            {
                item.useTime = 30;
                item.useAnimation = 30;
                item.noUseGraphic = false;

                if (Main.netMode == NetmodeID.MultiplayerClient && Main.myPlayer != player.whoAmI && (hook == null || !hook.projectile.active || hook.projectile.type != ModContent.ProjectileType<ScrapshotHook>() || hook.projectile.owner != player.whoAmI))
                    findHook(player);

                return (hook is null || (hook != null && (!hook.projectile.active || hook.projectile.type != ModContent.ProjectileType<ScrapshotHook>() || (hook.isHooked && !hook.struck))));
            }
        }

        public override void UseStyle(Player player)
        {
            //only know rotation for self
            if (player.whoAmI == Main.myPlayer)
            {
                if (player.altFunctionUse != 2)
                {
                    player.direction = (player.Center - Main.MouseWorld).ToRotation().ToRotationVector2().X < 0 ? 1 : -1;
                    player.itemRotation = (player.Center - Main.MouseWorld).ToRotation() + (player.direction == 1 ? 3.14f : 0);
                }
            }
        }

        public override bool UseItem(Player player)
        {
            //even though this is a "gun" we are using useitem so that it runs on all clients. need to deconstruct the useammo and damage modifiers ourselves here

            int damage = 0;
            float speed = item.shootSpeed;
            float speedX = 0;
            float speedY = 0;
            float knockback = 0;

            if (Main.myPlayer == player.whoAmI)
            {
                damage = (int)(item.damage * player.rangedDamage);
                float rotation = (player.Center - Main.MouseWorld).ToRotation() - 1.57f;
                speedX = speed * (float)Math.Sin(rotation);
                speedY = speed * -(float)Math.Cos(rotation);
                knockback = item.knockBack;
            }

            if (player.altFunctionUse == 2)
            {
                if (Main.myPlayer == player.whoAmI)
                {
                    int i = Projectile.NewProjectile(player.Center, new Vector2(speedX, speedY), ModContent.ProjectileType<ScrapshotHook>(), item.damage, item.knockBack, player.whoAmI);
                    hook = Main.projectile[i].modProjectile as ScrapshotHook;
                }

                Helper.PlayPitched("Guns/ChainShoot", 0.5f, 0, player.Center);
            }
            else
            {
                float spread = 0.5f;

                int type = ProjectileID.Bullet;
                Item sample = new Item();
                sample.SetDefaults(type);
                sample.useAmmo = AmmoID.Bullet;

                bool shoot = true;

                player.PickAmmo(sample, ref type, ref speed, ref shoot, ref damage, ref knockback, !ConsumeAmmo(player));
                shoot = player.HasAmmo(sample, shoot);

                if (!shoot)
                    return false;

                if (type == ProjectileID.Bullet)
                    type = ModContent.ProjectileType<ScrapshotShrapnel>();

                if (Main.myPlayer == player.whoAmI)
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 8;
                

                if (hook != null && hook.projectile.type == ModContent.ProjectileType<ScrapshotHook>() && hook.projectile.active && hook.isHooked)
                {
                    hook.struck = true;

                    NPC hooked = Main.npc[hook.hookedNpcIndex];
                    hook.projectile.timeLeft = 20;
                    player.velocity = Vector2.Normalize(hook.startPos - hooked.Center) * 12;

                    Helper.PlayPitched("ChainHit", 0.5f, 0, player.Center);

                    for (int k = 0; k < 20; k++)
                    {
                        var direction = Vector2.One.RotatedByRandom(6.28f);
                        Dust.NewDustPerfect(player.Center + direction * 10, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(2, 4), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
                    }

                    if (Main.myPlayer == player.whoAmI)
                    {
                        spread = 0.05f;
                        damage += 4;

                        player.GetModPlayer<StarlightPlayer>().Shake += 12;
                    }
                }

                float rot = new Vector2(speedX, speedY).ToRotation();

                if (Main.myPlayer != player.whoAmI)
                    hook = null;

                for (int k = 0; k < 6; k++)
                {
                    Vector2 offset = Vector2.UnitX.RotatedBy(rot);
                    var direction = offset.RotatedByRandom(spread);

                    if (Main.myPlayer == player.whoAmI)
                    {
                        int i = Projectile.NewProjectile(player.Center + (offset * 25), direction * item.shootSpeed, type, damage, item.knockBack, player.whoAmI);

                        if (type != ModContent.ProjectileType<ScrapshotShrapnel>())
                            Main.projectile[i].timeLeft = 30;

                        //don't know direction for other players so we only add these for self.
                        Dust.NewDustPerfect(player.Center + direction * 60, ModContent.DustType<Dusts.Glow>(), direction * Main.rand.NextFloat(20), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.2f, 0.5f));
                        Dust.NewDustPerfect(player.Center + direction * 60, ModContent.DustType<Dusts.Smoke>(), Vector2.UnitY * -2 + direction * 5, 0, new Color(60, 55, 50) * 0.5f, Main.rand.NextFloat(0.5f, 1));
                    }
                }

                Helper.PlayPitched("Guns/Scrapshot", 0.4f, 0, player.Center);
            }

            return true;
        }

        public override bool ConsumeAmmo(Player player)
        {
            return player.altFunctionUse != 2;
        }

        /// <summary>
        /// we are using the precondition that only 1 scrapshot hook can exist for a player in order to find and assign the hook in multiplayer
        /// </summary>
        /// <returns></returns>
        private void findHook(Player player)
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];

                if (proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<ScrapshotHook>())
                {
                    hook = (proj.modProjectile as ScrapshotHook);
                    return;
                }
            }
        }
    }

    public class ScrapshotHook : ModProjectile
    {
        public bool isHooked = false;
        public Vector2 startPos;
        public bool struck;

        public int timer;

        ref float Progress => ref projectile.ai[0];
        ref float Distance => ref projectile.ai[1];
        bool Retracting => projectile.timeLeft < 30;

        Player player => Main.player[projectile.owner];

        public byte hookedNpcIndex = 0;

        public override string Texture => AssetDirectory.BreacherItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.friendly = true;
            projectile.timeLeft = 60;
            projectile.aiStyle = -1;
            projectile.penetrate = 2;
        }

        private void findIfHit()
        {
            foreach (NPC npc in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.Hitbox.Intersects(projectile.Hitbox)))
            {
                if (player.HeldItem.modItem is Scrapshot)
                {
                    player.itemAnimation = 1;
                    player.itemTime = 1;
                }

                isHooked = true;
                projectile.velocity *= 0;
                startPos = player.Center;
                Distance = Vector2.Distance(startPos, npc.Center);
                hookedNpcIndex = (byte)npc.whoAmI;
            }
        }

        public override void AI()
        {

            projectile.rotation = projectile.velocity.ToRotation();

            if (projectile.timeLeft < 40)//slows down the projectile by 8%, for about 10 ticks before it retracts
                projectile.velocity *= 0.92f;

            if (projectile.timeLeft == 30)
            {
                startPos = projectile.Center;
                projectile.velocity *= 0;
            }

            if (Retracting)
                projectile.Center = Vector2.Lerp(player.Center, startPos, projectile.timeLeft / 30f);

            if (!isHooked && !Retracting && Main.myPlayer != projectile.owner)
            {
                projectile.friendly = true; //otherwise it will stop just short of actually intersecting the hitbox
                findIfHit(); //since onhit hooks are client side only, all other clients will manually check for collisions
            }

            if (isHooked && !struck)
            {
                timer++;
                NPC hooked = Main.npc[hookedNpcIndex];
                player.direction = startPos.X > hooked.Center.X ? -1 : 1;

                if (timer == 1)
                    Helper.PlayPitched("Guns/ChainPull", 1, 0, player.Center);

                if (timer < 10)
                {
                    player.velocity *= 0.96f;
                    return;
                }

                if (timer >= 10)
                    startPos = player.Center;

                projectile.timeLeft = 52;

                if (Vector2.Distance(projectile.Center, hooked.Center) > 128 || player.dead) //break the hook if the enemy is too fast or teleports, or if the player is dead
                {
                    hooked = null;
                    projectile.timeLeft = 30;
                    return;
                }

                projectile.Center = hooked.Center;

                Progress += (10f / Distance) * (0.8f + Progress * 1.5f);

                if (player.velocity.Y == 0 && hooked.Center.Y > player.Center.Y)
                    player.Center = new Vector2(Vector2.Lerp(startPos, hooked.Center, Progress).X, player.Center.Y);
                else
                    player.Center = Vector2.Lerp(startPos, hooked.Center, Progress);

                player.velocity *= 0;

                if (player.Hitbox.Intersects(hooked.Hitbox) || Progress > 1)
                {
                    struck = true;
                    projectile.timeLeft = 20;

                    player.immune = true;
                    player.immuneTime = 20;
                    player.velocity = Vector2.Normalize(startPos - hooked.Center) * 15;
                    player.GetModPlayer<StarlightPlayer>().Shake += 15;

                    hooked.StrikeNPC(projectile.damage, projectile.knockBack, player.Center.X < hooked.Center.X ? -1 : 1);
                    Helper.PlayPitched("Guns/ChainPull", 0.001f, 0, player.Center);
                }
            }

            if (struck)
            {
                player.fullRotation += (projectile.timeLeft / 20f) * 3.14f * player.direction;
                player.fullRotationOrigin = player.Size / 2;
                player.velocity *= 0.95f;
            }
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            if (target.life <= 0 || Retracting)
                return;

            if (player.HeldItem.modItem is Scrapshot)
            {
                player.itemAnimation = 1;
                player.itemTime = 1;
            }

            hookedNpcIndex = (byte)target.whoAmI;
            isHooked = true;
            projectile.velocity *= 0;
            startPos = player.Center;
            Distance = Vector2.Distance(startPos, target.Center);
            projectile.friendly = false;
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            damage /= 4;
            knockback /= 4f;
            crit = false;
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            if (struck)
                return false;

            Texture2D chainTex1 = ModContent.GetTexture(AssetDirectory.BreacherItem + "ScrapshotHookChain1");
            Texture2D chainTex2 = ModContent.GetTexture(AssetDirectory.BreacherItem + "ScrapshotHookChain2");
            Player player = Main.player[projectile.owner];

            float dist = Vector2.Distance(player.Center, projectile.Center);
            float rot = (player.Center - projectile.Center).ToRotation() + (float)Math.PI / 2f;

            float length = 1f / dist * chainTex1.Height;

            for (int k = 0; k * length < 1; k++)
            {
                var pos = Vector2.Lerp(projectile.Center, player.Center, k * length);

                if (k % 2 == 0)
                    spriteBatch.Draw(chainTex1, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
                else
                    spriteBatch.Draw(chainTex2, pos - Main.screenPosition, null, lightColor, rot, chainTex1.Size() / 2, 1, 0, 0);
            }

            Texture2D hook = Main.projectileTexture[projectile.type];

            spriteBatch.Draw(hook, projectile.Center - Main.screenPosition, null, lightColor, rot + ((float)Math.PI * 0.75f), hook.Size() / 2, 1, 0, 0);

            return false;
        }
    }

    public class ScrapshotShrapnel : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public override string Texture => AssetDirectory.BreacherItem + "ExplosiveFlare";

        public override void SetDefaults()
        {
            projectile.width = 16;
            projectile.height = 16;
            projectile.ranged = true;
            projectile.friendly = true;
            projectile.penetrate = 1;
            projectile.timeLeft = 100;
            projectile.extraUpdates = 4;
            projectile.alpha = 255;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Explosive Shrapnel");
            Main.projFrames[projectile.type] = 2;
        }

        public override void AI()
        {
            if (projectile.timeLeft == 100)
                projectile.velocity *= Main.rand.NextFloat(1.5f, 2);

            projectile.velocity *= 0.95f;

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                for (int i = 0; i < 10; i++)
                {
                    cache.Add(projectile.Center);
                }
            }

            cache.Add(projectile.Center);

            while (cache.Count > 10)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => factor * 5, factor =>
            {
                return new Color(255, 170, 80) * factor.X * (projectile.timeLeft / 100f);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));

            trail?.Render(effect);
        }
    }
}