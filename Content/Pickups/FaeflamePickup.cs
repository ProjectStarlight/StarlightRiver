using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Core;
using StarlightRiver.Content.GUI;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
    internal class FaeflamePickup : AbilityPickup
    {
        public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
        public override Color GlowColor => new Color(255, 255, 130);

        public override bool CanPickup(Player Player)
        {
            return !Player.GetHandler().Unlocked<Whip>();
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Faeflame");
        }

        public override void Visuals()
        {
            Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime), (float)Math.Sin(StarlightWorld.rottime)) * (float)Math.Sin(StarlightWorld.rottime * 2 + 1) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);
            Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime + 2) / 2, (float)Math.Sin(StarlightWorld.rottime + 2)) * (float)Math.Sin(StarlightWorld.rottime * 2 + 4) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);
            Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.rottime + 4), (float)Math.Sin(StarlightWorld.rottime + 4) / 2) * (float)Math.Sin(StarlightWorld.rottime * 2 + 2) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);

            Dust.NewDustPerfect(NPC.Center + Vector2.One.RotateRandom(Math.PI) * (float)Math.Sin(StarlightWorld.rottime * 2 + 2) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.UnitY * -2, 0, default, 0.25f);
        }

        public override void PickupVisuals(int timer)
        {
            if (timer == 1)
            {
                SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Pickups/get")); //start the SFX
            }

            if (Main.rand.NextBool(3) && timer > 40 && timer < 500)
            {
                Vector2 vel = Main.rand.NextVector2Circular(1,1);
                Dust.NewDustPerfect(Main.LocalPlayer.Center - vel * 90, DustType<Dusts.GlowLine>(), vel * Main.rand.NextFloat(4, 5), 1, new Color(255, 190, 50), 0.75f);
            }

            if (timer < 300 && timer > 50 && (Main.rand.NextBool(70) || timer % 75 == 0))
                SummonTendril(timer);

            if (timer == 569) //popup
            {
                string message = StarlightRiver.Instance.AbilityKeys.Get<Whip>().GetAssignedKeys().Count > 0 ?
                    "Press W/A/S/D + " + StarlightRiver.Instance.AbilityKeys.Get<Whip>().GetAssignedKeys()[0] + " to hook to enemies and tiles." :
                    "Press W/A/S/D + [Please Bind a Key] to hook to enemies and tiles.";

                Main.LocalPlayer.GetHandler().GetAbility<Whip>(out var whip);
                Core.Loaders.UILoader.GetUIState<TextCard>().Display("Faewhip", message, whip);

                Filters.Scene.Activate("Shockwave", Main.LocalPlayer.Center).GetShader().UseProgress(0f).UseIntensity(0);
                Filters.Scene.Deactivate("Shockwave");
            }
        }

        public override void PickupEffects(Player player)
        {
            AbilityHandler mp = player.GetHandler();
            mp.Unlock<Whip>();

            player.GetModPlayer<StarlightPlayer>().MaxPickupTimer = 570;
            player.AddBuff(BuffID.Featherfall, 580);
        }

        private void SummonTendril(int timer)
        {
            float x = 0;
            float y = 0;
            bool success = false;
            for (int tries = 0; tries < 99; tries++)
            {
                x = NPC.Center.X + Main.rand.Next(-200, 200);
                int ySign = Main.rand.NextBool() ? 1 : -1;

                for (float yOffset = 0; yOffset < 20; yOffset++)
                {
                    y = NPC.Center.Y + ((yOffset * 16) * ySign);

                    Tile tile = Main.tile[(int)x / 16, (int)y / 16];
                    if (tile.HasTile && Main.tileSolid[tile.TileType])
                    {
                        success = true;
                        break;
                    }
                }
                if (success)
                    break;
            }

            if (!success)
                return;

            Vector2 pos = new Vector2(x, y);

            Player player = Main.LocalPlayer;
            Vector2 posTo = player.Center;
            Projectile proj = Projectile.NewProjectileDirect(NPC.GetSource_Misc("Faeflame"), pos, pos.DirectionTo(NPC.Center) * 0.1f, ModContent.ProjectileType<FaeflameSpawnTendril>(), 0, 0);
            proj.ai[0] = posTo.X;
            proj.ai[1] = posTo.Y;
            proj.timeLeft = 570 - timer;

            player.velocity = pos.DirectionTo(NPC.Center) * -6;

            Helper.PlayPitched("Magic/HolyCastShort", 0.3f, 1, player.Center);
        }
    }

    public class FaeflamePickupTile : AbilityPickupTile
    {
        public override int PickupType => NPCType<FaeflamePickup>();
    }

    public class FaeflameTileItem : QuickTileItem
    {
        public FaeflameTileItem() : base("Faeflame", "Debug placer for ability pickup", "FaeflamePickupTile", -1) { }

        public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
    }

    internal class FaeflameSpawnTendril : ModProjectile, IDrawPrimitive, IDrawAdditive
    {
        public override string Texture => AssetDirectory.Assets + "Invisible";

        public Trail trail;
        public Trail glowTrail;
        public List<Vector2> cache;
        public float endScale = 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Tendril");
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = false;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 100;
        }

        public override void AI()
        {
            Vector2 endPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            if (Projectile.timeLeft < 100)
                Projectile.velocity *= 1.1f;
            if ((endPos - Projectile.Center).Length() < 20)
                Projectile.active = false;
            if (endScale < 1.4f)
                endScale += 0.2f;

            ManageCache();

            if (!Main.dedServ)
            {
                ManageTrail();
            }

            //SpawnDust();
        }

        public override void Kill(int timeLeft)
        {
            foreach (Vector2 point in cache)
            {
                 Dust.NewDustPerfect(point + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), DustType<Dusts.Glow>(), Main.rand.NextVector2Circular(1.5f,1.5f), 1, new Color(255, 190, 50), Main.rand.NextFloat(0.1f,0.5f));
            }
        }

        private void ManageCache()
        {
            cache = new List<Vector2>();
            for (int i = 0; i < 30; i++)
            {
                float lerper = 1 - (i / 30f);
                Vector2 endPos = new Vector2(Projectile.ai[0], Projectile.ai[1]);

                cache.Add(Vector2.Lerp(Projectile.Center, endPos, lerper));
            }
        }

        private void ManageTrail()
        {
            if (trail is null)
                trail = new Trail(Main.graphics.GraphicsDevice, 30, new TriangularTip(4), n => 10 + n * 0, n => new Color(255, 255, 150) * Math.Min(n.X * 5f, 1));

            if (glowTrail is null)
                glowTrail = new Trail(Main.graphics.GraphicsDevice, 30, new TriangularTip(4), n => 18 + n * 0, n => new Color(255, 150, 50) * 0.1f * Math.Min(n.X * 5f, 1));

            trail.Positions = cache.ToArray();
            glowTrail.Positions = cache.ToArray();
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End();
            var tex0 = ModContent.Request<Texture2D>("StarlightRiver/Assets/EnergyTrail", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            var tex1 = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
            Effect effect = Filters.Scene["WhipAbility"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * -0.025f);
            effect.Parameters["repeats"].SetValue(2f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(tex0);

            trail?.Render(effect);

            effect.Parameters["sampleTexture"].SetValue(tex1);

            glowTrail?.Render(effect);
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            var endTex = Request<Texture2D>("StarlightRiver/Assets/Abilities/WhipEndRoot").Value;
            var endGlow = Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;

            spriteBatch.Draw(endTex, Projectile.Center - Main.screenPosition, null, new Color(255, 190, 100), Main.GameUpdateCount * 0.1f, endTex.Size() / 2, endScale * 0.45f, 0, 0);
            spriteBatch.Draw(endGlow, Projectile.Center - Main.screenPosition, null, new Color(255, 190, 100), 0, endGlow.Size() / 2, endScale * 0.75f, 0, 0);
        }

        private void SpawnDust()
        {
            foreach (Vector2 point in cache)
            {
                if (Main.rand.NextBool(120))
                {
                    Vector2 vel = Vector2.Normalize(Projectile.velocity).RotatedByRandom(0.1f);
                    Dust.NewDustPerfect(Projectile.Center + vel * 64, DustType<Dusts.GlowLine>(), vel * Main.rand.NextFloat(2, 5), 1, new Color(255, 190, 50), 1.0f);
                }
            }
        }
    }
}