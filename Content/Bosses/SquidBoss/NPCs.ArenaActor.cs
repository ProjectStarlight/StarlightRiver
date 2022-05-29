using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.CustomHooks;
using StarlightRiver.Content.Items.Permafrost;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class ArenaActor : ModNPC
    {
        public NPC FakeBoss;

        private int WhitelistID => WallType<AuroraBrickWall>();

        public int WaterfallWidth = 0;
        ParticleSystem BubblesSystem = new ParticleSystem(AssetDirectory.SquidBoss + "Bubble", UpdateBubblesBody, 3);
        private Vector2 domeOffset = new Vector2(0, -886);

        private static VertexPositionColorTexture[] verticies;
        private static VertexBuffer buffer;
        private static Effect ApplyEffect;

        public float WaterLevelWorld { get => NPC.Center.Y + 35 * 16 - NPC.ai[0]; }

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("");

        public override bool? CanBeHitByItem(Player Player, Item Item) => false;

        public override bool? CanBeHitByProjectile(Projectile Projectile) => false;

        public override bool CheckActive() => false;

        public override void SetDefaults()
        {
            NPC.dontTakeDamage = true;
            NPC.dontCountMe = true;
            NPC.immortal = true;
            NPC.noGravity = true;
            NPC.lifeMax = 10;

            FakeBoss = new NPC();
            FakeBoss.SetDefaults(NPCType<SquidBoss>());
            FakeBoss.Center = StarlightWorld.SquidBossArena.Center() * 16 + new Vector2(0, -500);
            (FakeBoss.ModNPC as SquidBoss).QuickSetup();
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            database.Entries.Remove(bestiaryEntry);
        }

        public override void AI()
        {
            NPC.ai[1] += 0.04f; //used as timers for visuals
            NPC.ai[2] += 0.01f;

            if (!NPC.AnyNPCs(NPCType<SquidBoss>()))
            {
                (FakeBoss.ModNPC as SquidBoss).tentacles.ForEach(n => n.ai[1]++);
                (FakeBoss.ModNPC as SquidBoss).GlobalTimer++;
                (FakeBoss.ModNPC as SquidBoss).Opacity = 0f;
                (FakeBoss.ModNPC as SquidBoss).OpaqueJelly = true;
                (FakeBoss.ModNPC as SquidBoss).Animate(6, 0, 8);

                var followBox = new Rectangle((int)NPC.Center.X - 500, (int)NPC.Center.Y - 2000, 1000, 2500);

                if (followBox.Contains(Main.LocalPlayer.Center.ToPoint()))
                    FakeBoss.Center += ((Main.LocalPlayer.Center + new Vector2(0, -50)) - FakeBoss.Center) * 0.01f;

                if (WaterfallWidth > 0)
                    WaterfallWidth--;
            }

            if (WaterfallWidth > 0)
			{
                var heightDust = 2850 - (int)NPC.ai[0];

                Vector2 posDust = NPC.Center + new Vector2(-500 + Main.rand.NextFloat(-25, 25), -2280 + heightDust);
                Dust.NewDustPerfect(posDust, DustType<Dusts.Glow>(), -Vector2.UnitY.RotatedByRandom(1.2f) * Main.rand.NextFloat(5), 0, new Color(150, 200, 255));
                Dust.NewDustPerfect(posDust, DustType<Dusts.AuroraFast>(), -Vector2.UnitY.RotatedByRandom(1.2f) * Main.rand.NextFloat(15), 0, new Color(150, 200, 255));

                posDust = NPC.Center + new Vector2(500 + Main.rand.NextFloat(-25, 25), -2280 + heightDust);
                Dust.NewDustPerfect(posDust, DustType<Dusts.Glow>(), -Vector2.UnitY.RotatedByRandom(1.2f) * Main.rand.NextFloat(5), 0, new Color(150, 200, 255));
                Dust.NewDustPerfect(posDust, DustType<Dusts.AuroraFast>(), -Vector2.UnitY.RotatedByRandom(1.2f) * Main.rand.NextFloat(15), 0, new Color(150, 200, 255));
            }

            if (NPC.ai[0] < 150) 
                NPC.ai[0] = 150; //water clamping and return logic

            if (Main.LocalPlayer.controlQuickHeal)
                NPC.ai[0] += 4;

            if (!Main.npc.Any(n => n.active && n.ModNPC is SquidBoss) && NPC.ai[0] > 150) 
                NPC.ai[0]--;

            if (NPC.ai[1] > 6.28f) 
                NPC.ai[1] = 0;


            if (!Main.npc.Any(n => n.active && n.ModNPC is IcePlatform)) //spawn platforms if not present
            {
                SpawnPlatform(-640, 200);
                SpawnPlatform(640, 200);

                SpawnPlatform(-400, -70);
                SpawnPlatform(400, -70);

                SpawnPlatform(-150, -260);
                SpawnPlatform(150, -260);

                SpawnPlatform(-240, -150, true);
                SpawnPlatform(240, -150, true);

                SpawnPlatform(-460, 30, true);
                SpawnPlatform(460, 30, true);

                SpawnPlatform(-140, 300, true);
                SpawnPlatform(140, 300, true);

                SpawnPlatform(-340, 240, true);
                SpawnPlatform(340, 240, true);

                NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y - 2000, NPCType<GoldPlatform>());
            }

            Vector2 pos = NPC.Center + new Vector2(-832, 35 * 16) + new Vector2(0, -NPC.ai[0]);

            //Lighting
            if (!(StarlightWorld.cathedralOverlay is null) && StarlightWorld.cathedralOverlay.fade)
            {
                for (int k = 0; k < 45; k++)
                {
                    Vector2 target = pos + new Vector2(k / 45f * 3200, 0);

                    if (!WorldGen.InWorld((int)pos.X / 16, (int)pos.Y / 16)) return;

                    if (Framing.GetTileSafely(target).WallType == WhitelistID)
                    {
                        float sin = (float)Math.Sin(NPC.ai[1] + k);
                        float sin2 = (float)Math.Sin(NPC.ai[2] + k * 0.2f);
                        float cos = (float)Math.Cos(NPC.ai[2] + k);
                        Lighting.AddLight(target, new Vector3(10 * (1 + sin2), 14 * (1 + cos), 18) * (0.02f + sin * 0.003f));
                    }
                }

                for (int k = 0; k < 10; k++)
                {
                    var y = -200 + k * 60;

                    float opacity = 0.2f;

                    if ((NPC.Center.Y + y + 60) - pos.Y < 30)
                        opacity = Math.Clamp(1 - ((NPC.Center.Y + y + 60) - pos.Y) / 30f, 0.2f, 1);

                    Lighting.AddLight(NPC.Center + new Vector2(0, y), new Vector3(1, 1.2f, 1.5f) * 0.65f * opacity);
                    Lighting.AddLight(NPC.Center + new Vector2(-400, y), new Vector3(1, 1.2f, 1.5f) * 0.4f * opacity);
                    Lighting.AddLight(NPC.Center + new Vector2(400, y), new Vector3(1, 1.2f, 1.5f) * 0.4f * opacity);
                }
            }

            //Not Lighting
            for (int k = 0; k < 100; k++)
            {
                int x = (int)(NPC.Center.X / 16) - 50 + k;
                int y = (int)(NPC.Center.Y / 16) + 28;
                if (WorldGen.InWorld(x, y)) WorldGen.KillTile(x, y);
            }

            for (int k = 0; k < Main.maxPlayers; k++)
            {
                Player player = Main.player[k];

                if (player.active && player.Hitbox.Intersects(new Rectangle((int)pos.X, (int)pos.Y, 104 * 16, (int)NPC.ai[0])))
                    player.AddBuff(BuffType<Buffs.PrismaticDrown>(), 4, false);
            }

            for (int k = 0; k < Main.maxItems; k++)
            {
                Item Item = Main.item[k];

                if (Item is null || !Item.active)
                    continue;

                if (Item.Hitbox.Intersects(new Rectangle((int)pos.X, (int)pos.Y + 8, 200 * 16, (int)NPC.ai[0])) && Item.velocity.Y > -4) Item.velocity.Y -= 0.2f;

                if (Item.Hitbox.Intersects(new Rectangle((int)pos.X, (int)pos.Y - 8, 200 * 16, 16)))
                {
                    Item.position.Y = WaterLevelWorld - 16 + (float)Math.Sin((NPC.ai[1] + Item.position.X) % 6.28f) * 4;

                    if (Item.type == ItemType<SquidBossSpawn>() && NPC.ai[0] == 150 && !Main.npc.Any(n => n.active && n.ModNPC is SquidBoss)) //ready to spawn another squid              
                    {
                        NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y + 630, NPCType<SquidBoss>());
                        Item.active = false;
                        Item.TurnToAir();

                        for (int n = 0; n < 50; n++) Dust.NewDustPerfect(Item.Center, DustType<Dusts.Starlight>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20));
                    }
                }
            }
        }

        public void DrawWater(SpriteBatch spriteBatch)
        {
            if (Main.gameMenu) //bootleg solution to bootleg problems
                return;

            Texture2D tex = Request<Texture2D>(AssetDirectory.SquidBoss + "CathedralWater").Value;
            Vector2 pos = NPC.Center + new Vector2(-840, 30 * 16) + new Vector2(0, -tex.Height) - Main.screenPosition;
            var source = new Rectangle(0, tex.Height - (int)NPC.ai[0] + 5 * 16, tex.Width, (int)NPC.ai[0] - 5 * 16);

            Vector2 pos2 = NPC.Center + new Vector2(-840, 35 * 16) + new Vector2(0, -NPC.ai[0]) - Main.screenPosition;
            var source2 = new Rectangle(0, tex.Height - (int)NPC.ai[0] + 5 * 16, tex.Width, 2);

            LightingBufferRenderer.DrawWithLighting(pos + source.TopLeft(), tex, source, new Color(200, 230, 255) * 0.4f);
            LightingBufferRenderer.DrawWithLighting(pos2, tex, source2, Color.White * 0.8f);
        }

        private void DrawShine(Rectangle target)
        {
            if (Main.dedServ || !Helper.OnScreen(target)) return;

            //these should only initialize on the client!!!
            if (ApplyEffect is null) ApplyEffect = Main.dedServ ? null : ApplyEffect = Terraria.Graphics.Effects.Filters.Scene["WaterShine"].GetShader().Shader;
            if(buffer is null) buffer = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);
            if(verticies is null) verticies = new VertexPositionColorTexture[6];

            Matrix zoom = new Matrix
                (
                    Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                    0, Main.GameViewMatrix.Zoom.X, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                );

            ApplyEffect.Parameters["drawSize"].SetValue(target.Size());
            ApplyEffect.Parameters["colorSampleY"].SetValue(1 - (0.5f + DrawHelper.ConvertY(WaterLevelWorld - Main.screenPosition.Y) / 2f));
            ApplyEffect.Parameters["time"].SetValue((float)Main.timeForVisualEffects/ 75f);

            ApplyEffect.Parameters["draw"].SetValue(Request<Texture2D>(AssetDirectory.SquidBoss + "WaterOver").Value);
            ApplyEffect.Parameters["distort"].SetValue(Request<Texture2D>(AssetDirectory.SquidBoss + "WaterDistort").Value);
            ApplyEffect.Parameters["light"].SetValue(StarlightRiver.LightingBufferInstance.ScreenLightingTexture);
            ApplyEffect.Parameters["screenWidth"].SetValue(Main.screenWidth);
            ApplyEffect.Parameters["xOff"].SetValue(0.5f + DrawHelper.ConvertX(target.X) / 2f);
            ApplyEffect.Parameters["zoom"].SetValue(zoom);

            //var verticies = new VertexPositionColorTexture[6];
            //var buffer = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);

            verticies[0] = new VertexPositionColorTexture(new Vector3(DrawHelper.ConvertX(target.X), DrawHelper.ConvertY(target.Y), 0), Color.White, Vector2.Zero);
            verticies[1] = new VertexPositionColorTexture(new Vector3(DrawHelper.ConvertX(target.X + target.Width), DrawHelper.ConvertY(target.Y), 0), Color.White, Vector2.UnitX);
            verticies[2] = new VertexPositionColorTexture(new Vector3(DrawHelper.ConvertX(target.X), DrawHelper.ConvertY(target.Y + target.Height), 0), Color.White, Vector2.UnitY);

            verticies[3] = new VertexPositionColorTexture(new Vector3(DrawHelper.ConvertX(target.X + target.Width), DrawHelper.ConvertY(target.Y), 0), Color.White, Vector2.UnitX);
            verticies[4] = new VertexPositionColorTexture(new Vector3(DrawHelper.ConvertX(target.X + target.Width), DrawHelper.ConvertY(target.Y + target.Height), 0), Color.White, Vector2.One);
            verticies[5] = new VertexPositionColorTexture(new Vector3(DrawHelper.ConvertX(target.X), DrawHelper.ConvertY(target.Y + target.Height), 0), Color.White, Vector2.UnitY);

            buffer.SetData(verticies);

            Main.instance.GraphicsDevice.SetVertexBuffer(buffer);

            foreach (EffectPass pass in ApplyEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                Main.instance.GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, 2);
            }

            Main.instance.GraphicsDevice.SetVertexBuffer(null);
        }
      
        private static void UpdateBubblesBody(Particle particle)
        {
            particle.Timer--;

            particle.StoredPosition.Y += particle.Velocity.Y;
            particle.StoredPosition.X += (float)Math.Sin(StarlightWorld.rottime + particle.Velocity.X) * 0.45f;
            particle.Position = particle.StoredPosition - Main.screenPosition;
            particle.Alpha = particle.Timer < 70 ? particle.Timer / 70f : particle.Timer > 630 ? 1 - (particle.Timer - 630) / 70f : 1;
        }

        private void DrawWaterfalls(SpriteBatch spriteBatch)
		{
            if (WaterfallWidth <= 0)
                return;

            var width = WaterfallWidth + 2 * (float)Math.Sin(Main.GameUpdateCount * 0.1f);
            var height = 2850 - (int)NPC.ai[0];

            var tex = Request<Texture2D>("StarlightRiver/Assets/Bosses/SquidBoss/LaserGlow").Value;
            var tex2 = Request<Texture2D>("StarlightRiver/Assets/Bosses/SquidBoss/Laser").Value;

            spriteBatch.End();
            spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default);

            for (int k = 0; k < height; k += tex2.Height * 2)
            {
                var target = new Rectangle((int)(NPC.Center.X - Main.screenPosition.X) - (int)(width / 2) - 500, (int)(NPC.Center.Y - Main.screenPosition.Y) - 2300 + k, (int)width, tex2.Height * 2);
                var target2 = new Rectangle((int)(NPC.Center.X - Main.screenPosition.X) - (int)(width / 2) + 500, (int)(NPC.Center.Y - Main.screenPosition.Y) - 2300 + k, (int)width, tex2.Height * 2);
                var source = new Rectangle(0, (int)(-Main.GameUpdateCount * 1.5f) % tex2.Height, tex2.Width, tex2.Height);
                var color = new Color(200, 230, 255) * 0.2f;

                LightingBufferRenderer.DrawWithLighting(target, tex, source, color * 6);
                LightingBufferRenderer.DrawWithLighting(target2, tex, source, color * 6);

                LightingBufferRenderer.DrawWithLighting(target, tex2, source, color);
                LightingBufferRenderer.DrawWithLighting(target2, tex2, source, color);

                float sin = (float)Math.Sin(-NPC.ai[1] * 4 + k * 0.01f);
                float sin2 = (float)Math.Sin(-NPC.ai[2] * 4 + k * 0.002f);
                float cos = (float)Math.Cos(-NPC.ai[2] * 4 + k * 0.01f);
                Lighting.AddLight(target.Center() + Main.screenPosition, new Vector3(10 * (1 + sin2), 14 * (1 + cos), 18) * (0.02f + sin * 0.003f) * width / 50f);
                Lighting.AddLight(target2.Center() + Main.screenPosition, new Vector3(10 * (1 + sin2), 14 * (1 + cos), 18) * (0.02f + sin * 0.003f) * width / 50f);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
        }

        public void DrawBigWindow(SpriteBatch spriteBatch)
        {
            var drawCheck = new Rectangle(StarlightWorld.SquidBossArena.X * 16 - (int)Main.screenPosition.X, StarlightWorld.SquidBossArena.Y * 16 - (int)Main.screenPosition.Y, StarlightWorld.SquidBossArena.Width * 16, StarlightWorld.SquidBossArena.Height * 16);
            if (!Helper.OnScreen(drawCheck)) return;

            //parallax background
            Texture2D layer0 = Request<Texture2D>(AssetDirectory.SquidBoss + "Background0").Value;
            Texture2D layer1 = Request<Texture2D>(AssetDirectory.SquidBoss + "Background1").Value;
            Texture2D layer2 = Request<Texture2D>(AssetDirectory.SquidBoss + "Background2").Value;

            Vector2 pos = NPC.Center;
            Vector2 dpos = pos - Main.screenPosition;
            Rectangle target = new Rectangle((int)dpos.X - 630, (int)dpos.Y - 595 + 60, 1260, 1020);
            Color color = new Color(140, 150, 190);

            spriteBatch.Draw(layer0, target, GetSource(0.2f, layer0), color, 0, Vector2.Zero, 0, 0);
            spriteBatch.Draw(layer1, target, GetSource(0.15f, layer1), color, 0, Vector2.Zero, 0, 0);
            spriteBatch.Draw(layer2, target, GetSource(0.1f, layer2), color, 0, Vector2.Zero, 0, 0);

            target.Y -= 1100;
            target.X += 64;
            target.Width -= 128;

            spriteBatch.Draw(layer0, target, GetSource(0.2f, layer0), color, 0, Vector2.Zero, 0, 0);
            target.Y -= 100;
            spriteBatch.Draw(layer1, target, GetSource(0.15f, layer1), color, 0, Vector2.Zero, 0, 0);
            target.Y -= 240;
            spriteBatch.Draw(layer2, target, GetSource(0.1f, layer2), color, 0, Vector2.Zero, 0, 0);

            //Draw our fake boss           
            (FakeBoss.ModNPC as SquidBoss).tentacles.ForEach(n => (n.ModNPC as Tentacle).DrawUnderWater(spriteBatch, 0));
            (FakeBoss.ModNPC as SquidBoss).DrawUnderWater(spriteBatch, 0);

            BubblesSystem.DrawParticles(spriteBatch);

            if (Main.rand.Next(4) == 0)
                BubblesSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(6.28f), -Main.rand.NextFloat(0.6f, 1.2f)), 0, Main.rand.NextFloat(0.4f, 0.8f), Color.White * Main.rand.NextFloat(0.2f, 0.4f), 700, pos + new Vector2(Main.rand.Next(-600, 600), 500), new Rectangle(0, Main.rand.Next(3) * 16, 16, 16)));

            if (Main.rand.Next(4) == 0)
                BubblesSystem.AddParticle(new Particle(Vector2.Zero, new Vector2(Main.rand.NextFloat(6.28f), -Main.rand.NextFloat(0.6f, 1.2f)), 0, Main.rand.NextFloat(0.4f, 0.8f), Color.White * Main.rand.NextFloat(0.2f, 0.4f), 700, pos + new Vector2(Main.rand.Next(-600, 600), Main.rand.Next(-1200, -600)), new Rectangle(0, Main.rand.Next(3) * 16, 16, 16)));

            spriteBatch.End(); //we have to restart the SB here anyways, so lets use it to draw our BG with primitives

            Texture2D backdrop = Request<Texture2D>(AssetDirectory.SquidBoss + "Window").Value;         
            LightingBufferRenderer.DrawWithLighting(NPC.Center - backdrop.Size() / 2 + new Vector2(0, -886) - Main.screenPosition, backdrop);

            var shinePos = NPC.Center - backdrop.Size() / 2 + new Vector2(0, 1760 - NPC.ai[0]) - Main.screenPosition;
            DrawShine(new Rectangle((int)shinePos.X, (int)shinePos.Y, backdrop.Width, 240));

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            Texture2D dome = Request<Texture2D>(AssetDirectory.SquidBoss + "WindowDome").Value;
            spriteBatch.Draw(dome, NPC.Center - dome.Size() / 2 + domeOffset - Main.screenPosition, null, Color.White * 0.325f, 0, Vector2.Zero, 1, 0, 0);

            Texture2D glass = Request<Texture2D>(AssetDirectory.SquidBoss + "WindowIn").Value;
            Texture2D glass2 = Request<Texture2D>(AssetDirectory.SquidBoss + "WindowInGlow").Value;
            spriteBatch.Draw(glass, NPC.Center + new Vector2(0, -7 * 16 - 3) - Main.screenPosition, null, Color.White * 0.325f, 0, glass.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(glass2, NPC.Center + new Vector2(0, -7 * 16 - 3) - Main.screenPosition, null, Color.White * 0.2f, 0, glass.Size() / 2, 1, 0, 0);

            Texture2D ray = Request<Texture2D>(AssetDirectory.SquidBoss + "Godray").Value;

            for (int k = 0; k < 4; k++)
            {
                Color lightColor = new Color(120, 210, 255);
                spriteBatch.Draw(ray, NPC.Center + new Vector2(450, -250) - Main.screenPosition, null, lightColor * 0.5f, 0.9f + (float)Math.Sin((NPC.ai[2] + k) * 2) * 0.11f, Vector2.Zero, 1.5f, 0, 0);
                spriteBatch.Draw(ray, NPC.Center + new Vector2(-450, -250) - Main.screenPosition, null, lightColor * 0.5f, 0.45f + (float)Math.Sin((NPC.ai[2] + k) * 2) * 0.11f, Vector2.Zero, 1.5f, 0, 0);

                spriteBatch.Draw(ray, NPC.Center + new Vector2(0, -450) - Main.screenPosition, null, lightColor * 0.5f, 0.68f + (float)Math.Sin(NPC.ai[2] * 2 + k / 4f * 6.28f) * 0.13f, Vector2.Zero, 1.9f, 0, 0);
            }

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            drawReflections(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            DrawWaterfalls(spriteBatch);
        }

        /// <summary>
        /// small helper function to draw the reflections for the glass
        /// </summary>
        private void drawReflections(SpriteBatch spriteBatch)
        {
            Texture2D reflectionMap = Request<Texture2D>(AssetDirectory.SquidBoss + "WindowInMap").Value;
            Texture2D domeMap = Request<Texture2D>(AssetDirectory.SquidBoss + "WindowDomeMap").Value;
            var tintColor = Color.White;
            tintColor.A = (byte)(NPC.AnyNPCs(NPCType<SquidBoss>()) ? 25 : 100);

            ReflectionTarget.DrawReflection(spriteBatch, screenPos: NPC.Center - reflectionMap.Size() / 2 + new Vector2(0, -7 * 16 - 3) - Main.screenPosition, normalMap: reflectionMap, flatOffset: new Vector2(-0.0075f, 0.03f), offsetScale: 0.05f, tintColor: tintColor, restartSpriteBatch: false);
            ReflectionTarget.DrawReflection(spriteBatch, screenPos: NPC.Center - domeMap.Size() / 2 + domeOffset - Main.screenPosition, normalMap: domeMap, flatOffset: new Vector2(0f, 0.1f), offsetScale: 0.08f, tintColor: tintColor, restartSpriteBatch: false);
            ReflectionTarget.isDrawReflectablesThisFrame = true;
        }

        private void SpawnPlatform(int x, int y, bool small = false)
        {
            if (small) NPC.NewNPC(NPC.GetSource_FromThis(), (int)(NPC.Center.X + x), (int)(NPC.Center.Y + y), NPCType<IcePlatformSmall>());
            else NPC.NewNPC(NPC.GetSource_FromThis(), (int)(NPC.Center.X + x), (int)(NPC.Center.Y + y), NPCType<IcePlatform>());
        }

        private Rectangle GetSource(float offset, Texture2D tex)
        {
            int x = tex.Width / 2 - 640;
            int y = tex.Height / 2 - 595;
            Vector2 pos = new Vector2(x, y) - FindOffset(NPC.Center, offset);
            return new Rectangle((int)pos.X, (int)pos.Y, 1280, 1190);
        }

        private static Vector2 FindOffset(Vector2 basepos, float factor, bool noVertical = false)
        {
            Vector2 origin = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            float x = (origin.X - basepos.X) * factor;
            float y = (origin.Y - basepos.Y) * factor * 0.4f;
            return new Vector2(x, noVertical ? 0 : y);
        }

		public override void Unload()
		{
            BubblesSystem = null;
		}
	}
}
