using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Abilities.Faeflame
{
	public class Whip : Ability
    {
        public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
        public override float ActivationCostDefault => 0.15f;
        public override Color Color => new Color(255, 247, 126);

        public Trail trail;
        public Trail glowTrail;
        public Vector2[] trailPoints = new Vector2[100];
        public Effect effect;

        public Vector2 tipsPosition; //where the "tip" of the whip is in the world
        public bool attached; //if the whip is attached to anything
        public bool endRooted; //if the endpoint is "rooted" to a certain location and cant be moved

        private Vector2 startPoint; //visual spline fields
        public Vector2 midPoint;
        public float dist1;
        public float dist2;

        public float length;
        public float tipVelocity;

        public Vector2 extraVelocity;
        public float targetRot;

        public NPC attachedNPC; //if the whip is attached to an NPC, what is it attached to?

        public override void Reset()
        {

        }

        public override void OnActivate()
        {
            Player.mount.Dismount(Player);
            startPoint = Vector2.Zero;

            targetRot = (Main.MouseWorld - Player.Center).ToRotation();
            tipsPosition = Player.Center;
            tipVelocity = 4;

            for(int k = 0; k < 50; k++)
                Dust.NewDustPerfect(Player.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), DustType<Dusts.Glow>(), Vector2.Normalize(Main.MouseWorld - Player.Center).RotatedByRandom(1) * Main.rand.NextFloat(0, 9), 1, new Color(255, 190, 50), 0.5f);

            for (int k = 0; k < 20; k++)
            {
                float angle = Main.rand.NextFloat(-0.5f, 0.5f);
                var vel = Vector2.Normalize(Main.MouseWorld - Player.Center).RotatedBy(angle);
                Dust.NewDustPerfect(Player.Center + new Vector2(0, 60) + vel * 64, DustType<Dusts.GlowLine>(), vel * Main.rand.NextFloat(5, 15), 1, new Color(255, 190, 50), 1.0f);
            }

            Helpers.Helper.PlayPitched("Magic/HolyCastShort", 1, 1, Player.Center);
        }

        public override void UpdateActive()
        {
            bool control = StarlightRiver.Instance.AbilityKeys.Get<Whip>().Current;

            if (!control || Player.GetHandler().Stamina <= 0)
            {
                endRooted = false;
                attached = false;
                attachedNPC = null;

                Deactivate();

                extraVelocity = Main.MouseScreen;
                return;
            }

            Player.GetHandler().Stamina -= 0.0025f;

            if (!endRooted)
            {
                var dist = Vector2.Distance(Player.Center, tipsPosition);

                for (int k = 0; k < 4; k++)
                {
                    if (dist < 700)
                        tipsPosition += Vector2.UnitX.RotatedBy(targetRot) * tipVelocity;

                    if (Framing.GetTileSafely((int)tipsPosition.X / 16, (int)tipsPosition.Y / 16).BlockType == BlockType.Solid) //debug
                    {
                        endRooted = true;

                        for (int i = 0; i < 50; i++)
                            Dust.NewDustPerfect(tipsPosition + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), DustType<Dusts.Glow>(), Vector2.Normalize(Main.MouseWorld - Player.Center).RotatedByRandom(6.28f) * Main.rand.NextFloat(0, 4), 1, new Color(255, 190, 50), 0.3f);
                    }
                }

                if (tipVelocity < 16)
                    tipVelocity++;

                if (dist > 700)
                    Deactivate();

                length = dist - 80;
                if (length < 100)
                    length = 100;
            }
            else
            {
                if (attachedNPC != null && attachedNPC.active)
                    tipsPosition = attachedNPC.Center;

                Player.velocity -= extraVelocity;

                Player.velocity.Y -= 0.43f;

                Player.velocity += (Main.MouseWorld - tipsPosition) * -(0.05f - Helper.BezierEase(Player.velocity.Length() / 24f) * 0.025f);

                if (Player.velocity.Length() > 18)
                    Player.velocity = Vector2.Normalize(Player.velocity) * 17.99f;

                Player.velocity *= 0.92f;

                Vector2 pullPoint = tipsPosition + Vector2.Normalize(Player.Center - tipsPosition) * length;
                Player.velocity += (pullPoint - Player.Center) * 0.06f;
                extraVelocity = (pullPoint - Player.Center) * 0.05f;
            }

            for (int k = 0; k < 100; k++) //dust
            {
                var pos = PointOnSpline(k / 100f);

                if (k > 0 && Main.rand.Next(80) == 0)
                    Dust.NewDustPerfect(pos + new Vector2(0, 20), DustType<Dusts.GlowLineFast>(), Vector2.Normalize(pos - trailPoints[k - 1]).RotatedByRandom(0.1f) * Main.rand.NextFloat(6, 8), 1, new Color(255, Main.rand.Next(150, 255), 50), 0.4f);
            }
        }

		public override void DrawActiveEffects(SpriteBatch spriteBatch)
		{
            if (!Active || !CustomHooks.PlayerTarget.canUseTarget)
                return;

            if (trail is null)
                trail = new Trail(Main.graphics.GraphicsDevice, 100, new TriangularTip(4),  n => 20 + n * 0, n => new Color(255, 255, 150) * (endRooted ? Math.Min(n.X * 5f, 1) : (float)Math.Sin(n.X * 3.14f)));

            if (glowTrail is null)
                glowTrail = new Trail(Main.graphics.GraphicsDevice, 100, new TriangularTip(4), n => 46 + n * 0, n => new Color(255, 180, 100) * 0.35f * (endRooted ? Math.Min(n.X * 5f, 1) : (float)Math.Sin(n.X * 3.14f)));

            trail.Positions = trailPoints;
            glowTrail.Positions = trailPoints;

            for (int k = 0; k < 100; k++)
            {
                var pos = PointOnSpline(k / 100f);
                trailPoints[k] = pos;
            }

            if (effect is null)
                effect = Filters.Scene["WhipAbility"].GetShader().Shader;

            if (startPoint != Vector2.Zero)
            {

                Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
                Matrix view = Main.GameViewMatrix.ZoomMatrix;
                Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

                effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.05f);
                effect.Parameters["repeats"].SetValue(2f);
                effect.Parameters["transformMatrix"].SetValue(world * view * projection);
                effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail").Value);

                trail?.Render(effect);

                effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

                glowTrail?.Render(effect);
            }

            if (startPoint == Vector2.Zero)
            {               
                midPoint = Vector2.Lerp(Player.Center, tipsPosition, 0.5f);

                dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, tipsPosition - startPoint);
                dist2 = ApproximateSplineLength(30, midPoint, tipsPosition - startPoint, tipsPosition, tipsPosition - midPoint);
            }

            startPoint = Player.Center;
            midPoint += (Vector2.Lerp(Player.Center, tipsPosition, 0.5f) - midPoint) * 0.075f;

            Utils.DrawBorderString(spriteBatch, Player.velocity.Length() + " m/s", Player.Center + Vector2.UnitY * -40 - Main.screenPosition, Color.White);
		}

        private Vector2 PointOnSpline(float progress) //someone force me to generalize this stuff later lol
        {
            float factor = dist1 / (dist1 + dist2);

            if (progress < factor)
                return Vector2.Hermite(startPoint, midPoint - startPoint, midPoint, tipsPosition - startPoint, progress * (1 / factor));
            if (progress >= factor)
                return Vector2.Hermite(midPoint, tipsPosition - startPoint, tipsPosition, tipsPosition - midPoint, (progress - factor) * (1 / (1 - factor)));

            return Vector2.Zero;
        }

        private float ApproximateSplineLength(int steps, Vector2 start, Vector2 startTan, Vector2 end, Vector2 endTan)
        {
            float total = 0;
            Vector2 prevPoint = start;

            for (int k = 0; k < steps; k++)
            {
                Vector2 testPoint = Vector2.Hermite(start, startTan, end, endTan, k / (float)steps);
                total += Vector2.Distance(prevPoint, testPoint);

                prevPoint = testPoint;
            }

            return total;
        }

        public override void OnExit()
        {

        }

        public override bool HotKeyMatch(TriggersSet triggers, AbilityHotkeys abilityKeys)
        {
            return abilityKeys.Get<Whip>().Current;
        }
    }
}