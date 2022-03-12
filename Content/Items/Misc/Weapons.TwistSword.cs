using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
    class TwistSword : ModItem
    {
        public int charge = 0;

        int timer = 0;

        bool noItemLastFrame = false;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override bool CloneNewInstances => true;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Twisted Greatsword");
            Tooltip.SetDefault("Hold to unleash a whirling slash\nHold jump while slashing to accelerate upward");
        }

        public override bool Autoload(ref string name)
        {
            On.Terraria.Main.DrawPlayer += DrawChargeBar;
            return true;
        }

        public override void SetDefaults()
        {
            item.damage = 28;
            item.crit = 5;
            item.melee = true;
            item.width = 40;
            item.height = 20;
            item.useTime = 10;
            item.useAnimation = 10;
            item.useStyle = ItemUseStyleID.HoldingUp;
            item.noMelee = true;
            item.knockBack = 8;
            item.rare = ItemRarityID.Orange;
            item.channel = true;
            item.noUseGraphic = true;
        }

        public override ModItem Clone(Item item)
        {
            var clone = base.Clone(item);

            if (Main.mouseItem.type == ItemType<TwistSword>())
                item.modItem.HoldItem(Main.player[Main.myPlayer]);

            (clone as TwistSword).charge = (item.modItem as TwistSword).charge;
            (clone as TwistSword).timer = (item.modItem as TwistSword).timer;

            return clone;
        }

        public override bool CanUseItem(Player player) => charge > 40;

        public override bool UseItem(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(player.Center, Vector2.Zero, ProjectileType<TwistSwordProjectile>(), item.damage, item.knockBack, player.whoAmI);
                return true;
            }
            return false;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(charge);
            writer.Write(timer);
        }

        public override void NetRecieve(BinaryReader reader)
        {
            charge = reader.ReadInt32();
            timer = reader.ReadInt32();
        }

        public override void HoldItem(Player player)
        {
            if (noItemLastFrame && player.whoAmI == Main.myPlayer && !player.noItems && player.channel && CanUseItem(player))
            {
                //if the player gets hit by a noItem effect like cursed or forbidden winds dash, the twist sword projectile will die, but if they continue to hold left click through it we want to resummon the twist sword at the end
                //alteratively we could change the logic to have noitem set player.channel to false so they have to manually reclick once the effect ends but I think this feels more polished
                bool doesntOwnTwistSwordProj = true;
                for(int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if(proj.active && proj.owner == player.whoAmI && proj.type == ModContent.ProjectileType<TwistSwordProjectile>())
                    {
                        doesntOwnTwistSwordProj = false;
                        break;
                    }
                }
                if (doesntOwnTwistSwordProj)
                    UseItem(player);
            }
                

            if (player.channel && !player.noItems)
            {
                timer++;

                if (player.controlJump && timer % 2 == 1)
                {
                    charge--;
                    timer++;
                }

                player.fallStart = (int)player.position.Y / 16;

                if (player.velocity.Y > 2)
                    player.velocity.Y = 2;

                if (player.velocity.X < 5 && player.controlRight)
                    player.velocity.X += 0.2f;

                if (player.velocity.X > -5 && player.controlLeft)
                    player.velocity.X -= 0.2f;

                charge--;
            }
            else
            {
                timer = 0;
            }

            if (timer % 20 == 0 && timer > 0)
                Helper.PlayPitched("Magic/WaterWoosh", 0.3f, Main.rand.NextFloat(0.2f, 0.4f), player.Center);

            if (timer % 20 == 10 && timer > 0)
                Helper.PlayPitched("Magic/WaterWoosh", 0.3f, -0.4f, player.Center);

            if (charge <= 0)
                player.channel = false;

            if (charge < 600 && (!player.channel || player.noItems))
            {
                if (player.velocity.Y == 0)
                    charge += 10;
                else
                    charge += 2;
            }

            if (charge > 600)
                charge = 600;

            noItemLastFrame = player.noItems;
        }

        public override void UpdateInventory(Player player)
        {
            if (player.HeldItem != item)
            {
                if (charge < 600)
                {
                    if (player.velocity.Y == 0)
                        charge += 10;
                    else
                        charge += 2;
                }
            }
        }

        private void DrawChargeBar(On.Terraria.Main.orig_DrawPlayer orig, Main self, Player drawPlayer, Vector2 Position, float rotation, Vector2 rotationOrigin, float shadow)
        {
            orig(self, drawPlayer, Position, rotation, rotationOrigin, shadow);



            if (drawPlayer != null && !drawPlayer.HeldItem.IsAir && drawPlayer.HeldItem.type == ItemType<TwistSword>() && PlayerTarget.canUseTarget)
            {
                int charge = (drawPlayer.HeldItem.modItem as TwistSword).charge;
                var tex = GetTexture(AssetDirectory.GUI + "SmallBar1");
                var tex2 = GetTexture(AssetDirectory.GUI + "SmallBar0");
                Point pos = (drawPlayer.Center + new Vector2(-tex.Width / 2, -40) + Vector2.UnitY * drawPlayer.gfxOffY - Main.screenPosition).ToPoint();
                Rectangle target = new Rectangle(pos.X, pos.Y, (int)(charge / 600f * tex.Width), tex.Height);
                Rectangle source = new Rectangle(0, 0, (int)(charge / 600f * tex.Width), tex.Height);
                Rectangle target2 = new Rectangle(pos.X, pos.Y + 2, tex2.Width, tex2.Height);
                Vector3 color = Vector3.Lerp(Color.Red.ToVector3(), Color.Aqua.ToVector3(), charge / 800f);

                Main.spriteBatch.Draw(tex2, target2, new Color(40, 40, 40));
                Main.spriteBatch.Draw(tex, target, source, new Color(color.X, color.Y, color.Z));
            }
        }
    }

    class TwistSwordProjectile : ModProjectile, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        public override string Texture => AssetDirectory.MiscItem + Name;

        public override void SetDefaults()
        {
            projectile.width = 250;
            projectile.height = 50;
            projectile.friendly = true;
            projectile.tileCollide = false;
            projectile.penetrate = -1;
            projectile.timeLeft = 2;
            projectile.extraUpdates = 3;
            projectile.melee = true;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float rot = projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 160;
            float y = (float)Math.Sin(-rot) * 70;
            Vector2 off = new Vector2(x, y);

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), projectile.Center, projectile.Center + off);
        }

        private void findIfHit()
        {
            foreach (NPC npc in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.immune[projectile.owner] <= 0 && Colliding(projectile.Hitbox, n.Hitbox) == true))
            {
                OnHitNPC(npc, 0, 0, false);
            }
        }

        public override void AI()
        {
            Player player = Main.player[projectile.owner];

            if (projectile.ai[1] < 400)
            projectile.ai[1]++;

            if (!player.controlJump && projectile.ai[1] > 200)
                projectile.ai[1] = 200;

            projectile.ai[0] += 0.4f + projectile.ai[1] * 0.0025f;

            projectile.Center = player.Center + new Vector2(0, player.gfxOffY);

            if (player.channel && player.HeldItem.type == ItemType<TwistSword>() && !player.noItems)
                projectile.timeLeft = 2;

            if (projectile.ai[1] > 200 && player.velocity.Y > -4)
                player.velocity.Y -= 0.0004f * projectile.ai[1];

            //visuals
            float rot = projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 120;
            float y = (float)Math.Sin(-rot) * 40;
            Vector2 off = new Vector2(x, y);

            if (rot > 3.14f)
                player.heldProj = projectile.whoAmI;

            if (Main.rand.Next(3) == 0)
                Dust.NewDustPerfect(player.Center + off, DustType<Content.Dusts.Glow>(), off * Main.rand.NextFloat(0.01f), 0, new Color(10, 30, 255), Main.rand.NextFloat(0.2f, 0.4f));

            if (Main.rand.Next(25) == 0)
                Dust.NewDustPerfect(player.Center + off, DustType<Content.Dusts.WaterBubble>(), off * Main.rand.NextFloat(0.01f), 0, new Color(160, 180, 255), Main.rand.NextFloat(0.2f, 0.4f));

            if (player.channel && player.HeldItem.type == ItemType<TwistSword>() && !player.noItems)
                player.UpdateRotation(rot);
            else
                player.UpdateRotation(0);

            Lighting.AddLight(projectile.Center + off, new Vector3(0.1f, 0.25f, 0.6f));

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }

            if (Main.myPlayer != projectile.owner)
                findIfHit();
        }


        public override void Kill(int timeLeft)
        {
            //have to reset rotation in multiplayer when proj is gone
            Player player = Main.player[projectile.owner];
            player.UpdateRotation(0);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            float rot = projectile.ai[0] % 80 / 80f * 6.28f;
            var away = Vector2.UnitX.RotatedBy(rot);

            target.immune[projectile.owner] = 10; //same as regular pierce projectile but explicit for multiplayer compatibility

            target.velocity += away * 8 * target.knockBackResist;

            if (Main.netMode != NetmodeID.Server)
                onHitEffect(target);
        }

        public void onHitEffect(NPC target)
        {
            Helper.PlayPitched("Magic/WaterSlash", 0.4f, 0.2f, projectile.Center);
            Helper.PlayPitched("Magic/WaterWoosh", 0.3f, 0.6f, projectile.Center);

            float rot = projectile.ai[0] % 80 / 80f * 6.28f;
            var away = Vector2.UnitX.RotatedBy(rot);
            for (int k = 0; k < 20; k++)
                Dust.NewDustPerfect(target.Center, DustType<Dusts.Glow>(), away.RotatedByRandom(0.2f) * Main.rand.NextFloat(4), 0, new Color(50, 110, 255), 0.4f);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            float rot = projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 120;

            Texture2D tex = GetTexture(Texture);
            var owner = Main.player[projectile.owner];

            Rectangle target = new Rectangle((int)(owner.Center.X - Main.screenPosition.X), (int)(owner.Center.Y - Main.screenPosition.Y), (int)Math.Abs(x / 120f * tex.Size().Length()), 40);

            spriteBatch.Draw(tex, target, null, lightColor, -rot, new Vector2(0, tex.Height), SpriteEffects.None, default);

            return false;
        }

        private void ManageCaches()
        {
            float rot = projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 120;
            float y = (float)Math.Sin(-rot) * 40;
            Vector2 off = new Vector2(x, y);

            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cache.Add(projectile.Center + off);
                }
            }

            cache.Add(projectile.Center + off);

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            float rot = projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 120;
            float y = (float)Math.Sin(-rot) * 40;
            Vector2 off = new Vector2(x, y);

            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => factor * 25, factor =>
            {
                if (factor.X >= 0.98f)
                    return Color.White * 0;

                return new Color(50, 30 + (int)(100 * factor.X), 255) * factor.X;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center + projectile.velocity + off;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["CeirosRing"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(GetTexture("StarlightRiver/Assets/ShadowTrail"));

            trail?.Render(effect);
        }
    }
}
