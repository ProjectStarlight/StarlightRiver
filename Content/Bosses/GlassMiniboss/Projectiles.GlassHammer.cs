using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using ReLogic.Content;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassHammer : ModProjectile
    {
        Vector2 origin;

        public NPC Parent => Main.npc[(int)Projectile.ai[0]];

        public override string Texture => AssetDirectory.GlassMiniboss + "GlassHammer";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Hammer");

        public override void SetDefaults()
        {
            Projectile.width = 36;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.timeLeft = 150;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public override void AI()
        {
            if (Projectile.timeLeft == 120)
            {
                //TODO: figure out the shockwave
                //Projectile.NewProjectile(Projectile.InheritSource(Projectile), Parent.Center, Vector2.UnitX * 9, ProjectileType<Shockwave>(), 22, 0, Main.myPlayer, Projectile.ai[0]); //Shockwave spawners
                //Projectile.NewProjectile(Projectile.InheritSource(Projectile), Parent.Center, Vector2.UnitX * -9, ProjectileType<Shockwave>(), 22, 0, Main.myPlayer, Projectile.ai[0]);
            }

            float swingAccel = Utils.GetLerpValue(92f, 80f, Projectile.timeLeft, true);

            Vector2 handleOffset;
            int handFrame = (int)((swingAccel) * 3f);
            switch (handFrame)
            {
                default:
                    handleOffset = new Vector2(-10, 0);
                    break;
                case 1:
                    handleOffset = new Vector2(25, -12);
                    break;
                case 2:
                    handleOffset = new Vector2(30, 0);
                    break;
                case 3:
                    handleOffset = new Vector2(40, 5);
                    break;
            }
            handleOffset.X *= Parent.direction;

            float chargeRot = MathHelper.Lerp(MathHelper.ToRadians(170), MathHelper.ToRadians(30), swingAccel)
                + MathHelper.SmoothStep(0, MathHelper.ToRadians(20), Utils.GetLerpValue(140, 115, Projectile.timeLeft, true))
                - MathHelper.Lerp(0, MathHelper.ToRadians(70), Helpers.Helper.BezierEase(Utils.GetLerpValue(115, 70, Projectile.timeLeft, true)));

            Projectile.rotation = (chargeRot * -Parent.direction) + (Parent.direction < 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4) + Parent.rotation;
            origin = Parent.Center + handleOffset;
            Projectile.Center = origin + new Vector2(83, -90).RotatedBy(Projectile.rotation - (Parent.direction < 0 ? MathHelper.PiOver2 : 0));

            if (Projectile.timeLeft == 80)
            {
                Helpers.Helper.PlayPitched("VitricBoss/CeirosPillarImpact", 0.7f, 1.33f, Projectile.Center);
                Core.Systems.CameraSystem.Shake += 15;
                for (int i = 0; i < 30; i++)
                    Dust.NewDust(Projectile.Center - new Vector2(4, -4), 8, 4, DustType<Dusts.GlassGravity>(), 0, -4);
            }

            if (Projectile.timeLeft > 120)
            {
                Vector2 alongHammer = Vector2.Lerp(origin, Projectile.Center, Main.rand.NextFloat());
                Dust.NewDustPerfect(alongHammer, DustType<Dusts.GlowFastDecelerate>(), newColor: Color.DarkOrange, Scale: 0.3f); 
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Projectile.timeLeft <= 90 && Projectile.timeLeft >= 75)
                target.AddBuff(BuffType<Buffs.Squash>(), 180);
        }


        public override void Kill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);

            for (int k = 0; k < 10; k++)
                Dust.NewDustPerfect(Vector2.Lerp(origin, Projectile.Center, Main.rand.NextFloat()), DustType<Dusts.GlassGravity>());
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCs.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> hammer = Request<Texture2D>(Texture);
            Rectangle frame = hammer.Frame(1, 2, 0, 0);
            Rectangle hotFrame = hammer.Frame(1, 2, 0, 1);

            SpriteEffects effects = Parent.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            Vector2 handle = frame.Size() * new Vector2(Parent.direction < 0 ? 1f : 0f, 1f);

            Color fadeIn = lightColor * Utils.GetLerpValue(140, 134, Projectile.timeLeft, true);
            Main.EntitySpriteDraw(hammer.Value, origin - Main.screenPosition, frame, fadeIn, Projectile.rotation, handle, 1, effects, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(150, 140, Projectile.timeLeft, true) * Utils.GetLerpValue(90, 120, Projectile.timeLeft, true);
            Main.EntitySpriteDraw(hammer.Value, origin - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, handle, 1, effects, 0);

            return false;
        }
    }

    class Shockwave : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.timeLeft = 150;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
        }

        public NPC Parent => Main.npc[(int)Projectile.ai[0]];

        public override bool OnTileCollide(Vector2 oldVelocity) => false;

        public override void AI()
        {
            //Tile tile = Framing.GetTileSafely((int)Projectile.Center.X / 16 + (Projectile.velocity.X > 0 ? 1 : -1), (int)Projectile.Center.Y / 16);
            //Main.NewText(tile.type);//debug

            Projectile.direction = Projectile.velocity.X < 0 ? -1 : 1;

            if (Projectile.timeLeft < 150 && Projectile.velocity.Y == 0 && Projectile.timeLeft % 15 == 0)
                Projectile.NewProjectile(Projectile.InheritSource(Projectile), Projectile.Center + Vector2.UnitY * 9f, Vector2.Zero, ProjectileType<ShockwaveSpike>(), Projectile.damage, 0, Projectile.owner, ai1: Projectile.direction);

            if (Projectile.velocity.X == 0)
                Projectile.Kill();

            Projectile.velocity.Y = 5;
        }
    }

    class ShockwaveSpike : ModProjectile
    {
        public override string Texture => AssetDirectory.GlassMiniboss + "GlassSpike";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Glass Formation");

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 2;
            Projectile.hostile = true;
            Projectile.hide = true;
            Projectile.timeLeft = 180;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
        }

        private float offGround;

        public override void AI()
        {
            Projectile.ai[0]++; //ticks up the timer

            if (Projectile.ai[0] == 70)
            {
                Projectile.hostile = true; //when this Projectile goes off
                Core.Systems.CameraSystem.Shake += 5;
                for (int k = 0; k < 15; k++)
                {
                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(12, 3), DustType<Dusts.Stone>(), Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-4, -2));
                    Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 3), DustType<Dusts.Glow>(), Vector2.UnitY.RotatedByRandom(1) * Main.rand.NextFloat(-2, 0), 0, new Color(255, 160, 80), 0.4f);
                }

                SoundEngine.PlaySound(SoundID.Item70, Projectile.Center);
            }

            offGround = 1f - (Helpers.Helper.BezierEase(Utils.GetLerpValue(60, 70, Projectile.ai[0], true)) * Helpers.Helper.BezierEase(Utils.GetLerpValue(200, 110, Projectile.ai[0], true)));
        }

        public override void DrawBehind(int index, List<int> behindNPCsAndTiles, List<int> behindNPCs, List<int> behindProjectiles, List<int> overPlayers, List<int> overWiresUI) => behindNPCsAndTiles.Add(index);

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> spike = Request<Texture2D>(Texture);
            Rectangle frame = spike.Frame(2, 1);

            Rectangle groundFrame = new Rectangle(0, 0, frame.Width, (int)(frame.Height * (1 - offGround)));
            Rectangle hotGroundFrame = new Rectangle(frame.Width, 0, frame.Width, (int)(frame.Height * (1 - offGround)));
            Vector2 off = new Vector2(0, frame.Height * offGround);

            Main.spriteBatch.Draw(spike.Value, Projectile.Center + off - Main.screenPosition, groundFrame, lightColor, 0, frame.Size() * new Vector2(0.5f, 1f), 1f, 0, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(120, 90, Projectile.ai[0], true);
            Main.spriteBatch.Draw(spike.Value, Projectile.Center + off - Main.screenPosition, hotGroundFrame, hotFade, 0, frame.Size() * new Vector2(0.5f, 1f), 1f, 0, 0);

            Asset<Texture2D> tell = TextureAssets.Extra[98];
            Rectangle tellFrame = tell.Frame(1, 2);

            Color tellFade = Color.OrangeRed * Utils.GetLerpValue(0, 10, Projectile.ai[0], true) * Utils.GetLerpValue(40, 10, Projectile.ai[0], true);
            tellFade.A = 0;
            Vector2 tellScale = new Vector2(Utils.GetLerpValue(40, 0, Projectile.ai[0], true) * 12f, 2f + (Utils.GetLerpValue(0, 40, Projectile.ai[0], true) * 10f));
            Main.spriteBatch.Draw(tell.Value, Projectile.Center - Main.screenPosition, tellFrame, tellFade, 0, tellFrame.Size() * new Vector2(0.5f, 1f), tellScale, 0, 0);

            return false;
        }
    }
}
