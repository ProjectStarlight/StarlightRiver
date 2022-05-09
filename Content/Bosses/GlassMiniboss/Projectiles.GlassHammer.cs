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
using Terraria.DataStructures;

namespace StarlightRiver.Content.Bosses.GlassMiniboss
{
	class GlassHammer : ModProjectile
    {
        Vector2 origin;

        public override string Texture => AssetDirectory.GlassMiniboss + "GlassHammer";

        public override void SetStaticDefaults() => DisplayName.SetDefault("Woven Hammer");

        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
            Projectile.hide = true;
        }

        public NPC Parent => Main.npc[(int)Projectile.ai[0]];

        public ref float TotalTime => ref Projectile.ai[1];

        //yay this exists
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.localAI[0] = (int)TotalTime;
            Projectile.timeLeft = (int)TotalTime + 50;
            Helpers.Helper.PlayPitched("GlassMiniboss/WeavingLong", 1f, 0f, Projectile.Center);
        }

        public override void AI()
        {
            Projectile.localAI[0]--;

            float dropLerp = Utils.GetLerpValue(TotalTime * 0.93f, TotalTime * 0.5f, Projectile.localAI[0], true);
            float liftLerp = Helpers.Helper.SwoopEase(1f - Utils.GetLerpValue(TotalTime * 0.6f, TotalTime * 0.15f, Projectile.localAI[0], true));
            float swingAccel = Utils.GetLerpValue(TotalTime * 0.16f, 0, Projectile.localAI[0], true) * Utils.GetLerpValue(TotalTime * 0.16f, TotalTime * 0.02f, Projectile.localAI[0], true);
            //10 degree drop
            //30 degree lift
            //160 degree swing to floor

            float chargeRot = MathHelper.Lerp(-MathHelper.ToRadians(10), 0, dropLerp)
                - MathHelper.Lerp(MathHelper.ToRadians(30), 0, liftLerp)
                + MathHelper.Lerp(MathHelper.ToRadians(180), MathHelper.ToRadians(20), swingAccel);

            Vector2 handleOffset;
            int handleFrame = (int)(Utils.GetLerpValue(TotalTime * 0.2f, TotalTime * 0.01f, Projectile.localAI[0], true) * 3f);
            switch (handleFrame)
            {
                default:
                    handleOffset = new Vector2(-28, 2);
                    break;
                case 1:
                    handleOffset = new Vector2(40, 5);
                    break;
                case 2:
                    handleOffset = new Vector2(42, 7);
                    break;
                case 3:
                    handleOffset = new Vector2(48, 9);
                    break;
            }
            handleOffset.X *= Parent.direction;

            Projectile.rotation = (chargeRot * -Parent.direction) + (Parent.direction < 0 ? -MathHelper.PiOver4 : MathHelper.PiOver4) + Parent.rotation;
            origin = Parent.Center + handleOffset;
            Projectile.Center = origin + new Vector2(70, -80).RotatedBy(Projectile.rotation - (Parent.direction < 0 ? MathHelper.PiOver2 : 0));

            if (Projectile.localAI[0] == 30)
            {
                Projectile.NewProjectile(Entity.InheritSource(Projectile), Parent.Center, Vector2.UnitX * 18f, ProjectileType<HammerShockwave>(), 0, 0f, Main.myPlayer, Projectile.ai[0]);
                Projectile.NewProjectile(Entity.InheritSource(Projectile), Parent.Center, -Vector2.UnitX * 18f, ProjectileType<HammerShockwave>(), 0, 0f, Main.myPlayer, Projectile.ai[0]);
            }
            
            if (Projectile.localAI[0] == 0)
            {
                Helpers.Helper.PlayPitched("GlassMiniboss/GlassSmash", 1f, 0f, Projectile.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 15;

                for (int i = 0; i < 30; i++)
                {
                    Dust.NewDust(Projectile.Center - new Vector2(4, -4), 8, 4, DustType<Dusts.GlassGravity>(), Main.rand.Next(-1, 1), -4);
                    if (Main.rand.NextBool())
                    {
                        Dust glow = Dust.NewDustDirect(Projectile.Center - new Vector2(4, -4), 8, 4, DustType<Dusts.Glow>(), Main.rand.Next(-1, 1), -4, newColor: Color.DarkOrange);
                        glow.noGravity = false;
                    }
                }
            }

            if (Projectile.localAI[0] > TotalTime * 0.55f)
            {
                Vector2 alongHammer = Vector2.Lerp(origin, Projectile.Center + Main.rand.NextVector2Circular(30, 50).RotatedBy(Projectile.rotation), Main.rand.NextFloat());
                Dust glow = Dust.NewDustPerfect(alongHammer, DustType<Dusts.Glow>(), -Vector2.UnitY.RotatedByRandom(0.4f), newColor: Color.DarkOrange, Scale: 0.3f);
                glow.noGravity = false;
            }
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            if (Projectile.localAI[0] < TotalTime * 0.12f)
                target.AddBuff(BuffType<Buffs.Squash>(), 180);
        }

        public override void Kill(int timeLeft)
        {
            Helpers.Helper.PlayPitched("GlassMiniboss/GlassShatter", 1f, Main.rand.NextFloat(0.1f), Projectile.Center);

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

            Vector2 handle = frame.Size() * new Vector2(Parent.direction < 0 ? 0.9f : 0.1f, 0.9f);

            Color fadeIn = lightColor * Utils.GetLerpValue(TotalTime * 0.93f, TotalTime * 0.89f, Projectile.localAI[0], true);
            Main.EntitySpriteDraw(hammer.Value, origin - Main.screenPosition, frame, fadeIn, Projectile.rotation, handle, 1, effects, 0);

            Color hotFade = new Color(255, 255, 255, 128) * Utils.GetLerpValue(TotalTime, TotalTime * 0.87f, Projectile.localAI[0], true) * Utils.GetLerpValue(TotalTime * 0.55f, TotalTime * 0.83f, Projectile.localAI[0], true);
            Main.EntitySpriteDraw(hammer.Value, origin - Main.screenPosition, hotFrame, hotFade, Projectile.rotation, handle, 1, effects, 0);

            return false;
        }
    }

    class HammerShockwave : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("Shockwave");

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 32;
            Projectile.aiStyle = -1;
            Projectile.penetrate = -1;
        }

        public NPC Parent => Main.npc[(int)Projectile.ai[0]];

        public override bool OnTileCollide(Vector2 oldVelocity) => Math.Abs(Projectile.velocity.X) < 0.1f;

        public override void AI()
        {
            Projectile.localAI[0]++;

            Projectile.velocity.X *= 0.988f;
            //Projectile.velocity.Y = 9.8f;

            if (Projectile.localAI[0] % 15 == 0 && Projectile.localAI[0] > 5)
            {
                Vector2 spawnPos = Helpers.Helper.QuickBestPosition(Projectile.Center, Parent.GetTargetData().Center);
                Dust.NewDustPerfect(spawnPos, DustID.AncientLight, -Vector2.UnitY * 1f);
            }
        }
    }
}
