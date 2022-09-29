﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Graphics;
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

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Twisted Greatsword");
            Tooltip.SetDefault("Hold to unleash a whirling slash\nHold jump while slashing to accelerate upward");
        }

        public override void Load()
        {
            On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayer += DrawChargeBar;
        }

		public override void SetDefaults()
        {
            Item.damage = 28;
            Item.crit = 5;
            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 20;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.noMelee = true;
            Item.knockBack = 8;
            Item.rare = ItemRarityID.Orange;
            Item.channel = true;
            Item.noUseGraphic = true;
        }

        public override ModItem Clone(Item Item)
        {
            var clone = base.Clone(Item);

            if (!(Item.ModItem is TwistSword))
                return clone;

            if (Main.mouseItem.type == ItemType<TwistSword>())
                Item.ModItem.HoldItem(Main.player[Main.myPlayer]);

            (clone as TwistSword).charge = (Item.ModItem as TwistSword).charge;
            (clone as TwistSword).timer = (Item.ModItem as TwistSword).timer;

            return clone;
        }

        public override bool CanUseItem(Player Player) => charge > 40;

        public override bool? UseItem(Player Player)
        {
            if (Player.whoAmI == Main.myPlayer)
            {
                Projectile.NewProjectile(Player.GetSource_ItemUse(Item), Player.Center, Vector2.Zero, ProjectileType<TwistSwordProjectile>(), Item.damage, Item.knockBack, Player.whoAmI);
                return true;
            }
            return false;
        }

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(charge);
            writer.Write(timer);
        }

		public override void NetReceive(BinaryReader reader)
		{
            charge = reader.ReadInt32();
            timer = reader.ReadInt32();
        }

        public override void HoldItem(Player Player)
        {
            if (noItemLastFrame && Player.whoAmI == Main.myPlayer && !Player.noItems && Player.channel && CanUseItem(Player))
            {
                //if the Player gets hit by a noItem effect like cursed or forbidden winds dash, the twist sword Projectile will die, but if they continue to hold left click through it we want to resummon the twist sword at the end
                //alteratively we could change the logic to have noItem set Player.channel to false so they have to manually reclick once the effect ends but I think this feels more polished
                bool doesntOwnTwistSwordProj = true;
                for(int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if(proj.active && proj.owner == Player.whoAmI && proj.type == ModContent.ProjectileType<TwistSwordProjectile>())
                    {
                        doesntOwnTwistSwordProj = false;
                        break;
                    }
                }
                if (doesntOwnTwistSwordProj)
                    UseItem(Player);
            }
                

            if (Player.channel && !Player.noItems)
            {
                timer++;

                if (Player.controlJump && timer % 2 == 1)
                {
                    charge--;
                    timer++;
                }

                Player.fallStart = (int)Player.position.Y / 16;

                if (Player.velocity.Y > 2)
                    Player.velocity.Y = 2;

                if (Player.velocity.X < 5 && Player.controlRight)
                    Player.velocity.X += 0.2f;

                if (Player.velocity.X > -5 && Player.controlLeft)
                    Player.velocity.X -= 0.2f;

                charge--;
            }
            else
            {
                timer = 0;
            }

            if (timer % 20 == 0 && timer > 0)
                Helper.PlayPitched("Magic/WaterWoosh", 0.3f, Main.rand.NextFloat(0.2f, 0.4f), Player.Center);

            if (timer % 20 == 10 && timer > 0)
                Helper.PlayPitched("Magic/WaterWoosh", 0.3f, -0.4f, Player.Center);

            if (charge <= 0)
                Player.channel = false;

            if (charge < 600 && (!Player.channel || Player.noItems))
            {
                if (Player.velocity.Y == 0)
                    charge += 10;
                else
                    charge += 2;
            }

            if (charge > 600)
                charge = 600;

            noItemLastFrame = Player.noItems;
        }

        public override void UpdateInventory(Player Player)
        {
            if (Player.HeldItem != Item)
            {
                if (charge < 600)
                {
                    if (Player.velocity.Y == 0)
                        charge += 10;
                    else
                        charge += 2;
                }
            }
        }

        private void DrawChargeBar(On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.orig_DrawPlayer orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, Player drawPlayer, Vector2 position, float rotation, Vector2 rotationOrigin, float shadow, float scale)
        {
            orig(self, camera, drawPlayer, position, rotation, rotationOrigin, shadow, scale);

            if (drawPlayer != null && !drawPlayer.HeldItem.IsAir && drawPlayer.HeldItem.type == ItemType<TwistSword>() && PlayerTarget.canUseTarget)
            {
                int charge = (drawPlayer.HeldItem.ModItem as TwistSword).charge;
                var tex = Request<Texture2D>(AssetDirectory.GUI + "SmallBar1").Value;
                var tex2 = Request<Texture2D>(AssetDirectory.GUI + "SmallBar0").Value;
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
            Projectile.width = 250;
            Projectile.height = 50;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 2;
            Projectile.extraUpdates = 3;
            Projectile.DamageType = DamageClass.Melee;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float rot = Projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 160;
            float y = (float)Math.Sin(-rot) * 70;
            Vector2 off = new Vector2(x, y);

            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + off);
        }

        private void findIfHit()
        {
            foreach (NPC NPC in Main.npc.Where(n => n.active && !n.dontTakeDamage && !n.townNPC && n.life > 0 && n.immune[Projectile.owner] <= 0 && Colliding(Projectile.Hitbox, n.Hitbox) == true))
            {
                OnHitNPC(NPC, 0, 0, false);
            }
        }

        public override void AI()
        {
            Player Player = Main.player[Projectile.owner];

            if (Projectile.ai[1] < 400)
            Projectile.ai[1]++;

            if (!Player.controlJump && Projectile.ai[1] > 200)
                Projectile.ai[1] = 200;

            Projectile.ai[0] += 0.4f + Projectile.ai[1] * 0.0025f;

            Projectile.Center = Player.Center + new Vector2(0, Player.gfxOffY);

            if (Player.channel && Player.HeldItem.type == ItemType<TwistSword>() && !Player.noItems)
                Projectile.timeLeft = 2;

            if (Projectile.ai[1] > 200 && Player.velocity.Y > -4)
                Player.velocity.Y -= 0.0004f * Projectile.ai[1];

            //visuals
            float rot = Projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 120;
            float y = (float)Math.Sin(-rot) * 40;
            Vector2 off = new Vector2(x, y);

            if (rot > 3.14f)
                Player.heldProj = Projectile.whoAmI;

            if (Main.rand.Next(3) == 0)
                Dust.NewDustPerfect(Player.Center + off, DustType<Content.Dusts.Glow>(), off * Main.rand.NextFloat(0.01f), 0, new Color(10, 30, 255), Main.rand.NextFloat(0.2f, 0.4f));

            if (Main.rand.Next(25) == 0)
                Dust.NewDustPerfect(Player.Center + off, DustType<Content.Dusts.WaterBubble>(), off * Main.rand.NextFloat(0.01f), 0, new Color(160, 180, 255), Main.rand.NextFloat(0.2f, 0.4f));

            if (Player.channel && Player.HeldItem.type == ItemType<TwistSword>() && !Player.noItems)
                Player.UpdateRotation(rot);
            else
                Player.UpdateRotation(0);

            Lighting.AddLight(Projectile.Center + off, new Vector3(0.1f, 0.25f, 0.6f));

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrail();
            }

            if (Main.myPlayer != Projectile.owner)
                findIfHit();
        }


        public override void Kill(int timeLeft)
        {
            //have to reset rotation in multiPlayer when proj is gone
            Player Player = Main.player[Projectile.owner];
            Player.UpdateRotation(0);
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            float rot = Projectile.ai[0] % 80 / 80f * 6.28f;
            var away = Vector2.UnitX.RotatedBy(rot);

            target.immune[Projectile.owner] = 10; //same as regular pierce Projectile but explicit for multiPlayer compatibility

            target.velocity += away * 8 * target.knockBackResist;

            if (Main.netMode != NetmodeID.Server)
                onHitEffect(target);
        }

        public void onHitEffect(NPC target)
        {
            Helper.PlayPitched("Magic/WaterSlash", 0.4f, 0.2f, Projectile.Center);
            Helper.PlayPitched("Magic/WaterWoosh", 0.3f, 0.6f, Projectile.Center);

            float rot = Projectile.ai[0] % 80 / 80f * 6.28f;
            var away = Vector2.UnitX.RotatedBy(rot);
            for (int k = 0; k < 20; k++)
                Dust.NewDustPerfect(target.Center, DustType<Dusts.Glow>(), away.RotatedByRandom(0.2f) * Main.rand.NextFloat(4), 0, new Color(50, 110, 255), 0.4f);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float rot = Projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 120;

            Texture2D tex = Request<Texture2D>(Texture).Value;
            var owner = Main.player[Projectile.owner];

            Rectangle target = new Rectangle((int)(owner.Center.X - Main.screenPosition.X), (int)(owner.Center.Y - Main.screenPosition.Y), (int)Math.Abs(x / 120f * tex.Size().Length()), 40);

            Main.spriteBatch.Draw(tex, target, null, lightColor, -rot, new Vector2(0, tex.Height), SpriteEffects.None, default);

            return false;
        }

        private void ManageCaches()
        {
            float rot = Projectile.ai[0] % 80 / 80f * 6.28f;
            float x = (float)Math.Cos(-rot) * 120;
            float y = (float)Math.Sin(-rot) * 40;
            Vector2 off = new Vector2(x, y);

            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cache.Add(Projectile.Center + off);
                }
            }

            cache.Add(Projectile.Center + off);

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrail()
        {
            float rot = Projectile.ai[0] % 80 / 80f * 6.28f;
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
            trail.NextPosition = Projectile.Center + Projectile.velocity + off;
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
            effect.Parameters["sampleTexture"].SetValue(Request<Texture2D>("StarlightRiver/Assets/ShadowTrail").Value);

            trail?.Render(effect);
        }
    }
}
