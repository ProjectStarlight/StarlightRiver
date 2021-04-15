using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using StarlightRiver.Content.Tiles.Permafrost;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Content.Items.Permafrost.Tools;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Items.Permafrost;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
    class ArenaActor : ModNPC
    {
        readonly int whitelistID = WallType<AuroraBrickWall>();
        public float WaterLevel { get => npc.Center.Y + 35 * 16 - npc.ai[0]; }

        public override string Texture => AssetDirectory.Invisible;

        public override void SetStaticDefaults() => DisplayName.SetDefault("");

        public override bool? CanBeHitByItem(Player player, Item item) => false;

        public override bool? CanBeHitByProjectile(Projectile projectile) => false;

        public override bool CheckActive() => false;

        public override void SetDefaults()
        {
            npc.dontTakeDamage = true;
            npc.dontCountMe = true;
            npc.immortal = true;
            npc.noGravity = true;
            npc.lifeMax = 2;
        }

        public override void AI()
        {
            npc.ai[1] += 0.04f; //used as timers for visuals
            npc.ai[2] += 0.01f;

            if (npc.ai[0] < 150) npc.ai[0] = 150; //water clamping and return logic

            if (!Main.npc.Any(n => n.active && n.modNPC is SquidBoss) && npc.ai[0] > 150) npc.ai[0]--;

            if (npc.ai[1] > 6.28f) npc.ai[1] = 0;


            if (!Main.npc.Any(n => n.active && n.modNPC is IcePlatform)) //spawn platforms if not present
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

                NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y - 2000, NPCType<GoldPlatform>());
            }

            Vector2 pos = npc.Center + new Vector2(-832, 35 * 16) + new Vector2(0, -npc.ai[0]);

            //Lighting
            if (StarlightWorld.cathedralOverlay.fade)
            {
                for (int k = 0; k < 45; k++)
                {
                    Vector2 target = pos + new Vector2(k / 45f * 3200, 0);

                    if (!WorldGen.InWorld((int)pos.X / 16, (int)pos.Y / 16)) return;

                    if (Framing.GetTileSafely(target).wall == whitelistID)
                    {
                        float sin = (float)Math.Sin(npc.ai[1] + k);
                        float sin2 = (float)Math.Sin(npc.ai[2] + k * 0.2f);
                        float cos = (float)Math.Cos(npc.ai[2] + k);
                        Lighting.AddLight(target, new Vector3(10 * (1 + sin2), 14 * (1 + cos), 18) * (0.02f + sin * 0.003f));
                    }
                }

                for (int k = 0; k < 10; k++)
                {
                    Lighting.AddLight(npc.Center + new Vector2(0, -200 + k * 60), new Vector3(1, 1.2f, 1.5f) * 0.4f);
                    Lighting.AddLight(npc.Center + new Vector2(-400, -200 + k * 60), new Vector3(1, 1.2f, 1.5f) * 0.2f);
                    Lighting.AddLight(npc.Center + new Vector2(400, -200 + k * 60), new Vector3(1, 1.2f, 1.5f) * 0.2f);
                }
            }

            //Not Lighting
            for (int k = 0; k < 100; k++)
            {
                int x = (int)(npc.Center.X / 16) - 50 + k;
                int y = (int)(npc.Center.Y / 16) + 29;
                if (WorldGen.InWorld(x, y)) WorldGen.KillTile(x, y);
            }

            for (int k = 0; k < Main.ActivePlayersCount; k++)
            {
                Player player = Main.player[k];

                if (player.active && player.Hitbox.Intersects(new Rectangle((int)pos.X, (int)pos.Y, 104 * 16, (int)npc.ai[0])))
                    player.AddBuff(BuffType<Buffs.PrismaticDrown>(), 4, false);
            }

            for (int k = 0; k < Main.maxItems; k++)
            {
                Item item = Main.item[k];

                if (item.Hitbox.Intersects(new Rectangle((int)pos.X, (int)pos.Y + 8, 200 * 16, (int)npc.ai[0])) && item.velocity.Y > -4) item.velocity.Y -= 0.2f;

                if (item.Hitbox.Intersects(new Rectangle((int)pos.X, (int)pos.Y - 8, 200 * 16, 16)))
                {
                    item.position.Y = WaterLevel - 16 + (float)Math.Sin((npc.ai[1] + item.position.X) % 6.28f) * 4;

                    if (item.type == ItemType<SquidBossSpawn>() && npc.ai[0] == 150 && !Main.npc.Any(n => n.active && n.modNPC is SquidBoss)) //ready to spawn another squid              
                    {
                        NPC.NewNPC((int)npc.Center.X, (int)npc.Center.Y + 630, NPCType<SquidBoss>());
                        item.TurnToAir();

                        for (int n = 0; n < 50; n++) Dust.NewDustPerfect(item.Center, DustType<Dusts.Starlight>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(20));
                    }
                }
            }
        }

        public void DrawWater(SpriteBatch spriteBatch)
        {
            if (Main.gameMenu) //bootleg solution to bootleg problems
                return;

            Texture2D tex = GetTexture(AssetDirectory.SquidBoss + "CathedralWater");
            Vector2 pos = npc.Center + new Vector2(-840, 30 * 16) + new Vector2(0, -tex.Height) - Main.screenPosition;
            var source = new Rectangle(0, tex.Height - (int)npc.ai[0] + 5 * 16, tex.Width, (int)npc.ai[0] - 5 * 16);

            Vector2 pos2 = npc.Center + new Vector2(-840, 35 * 16) + new Vector2(0, -npc.ai[0]) - Main.screenPosition;
            var source2 = new Rectangle(0, tex.Height - (int)npc.ai[0] + 5 * 16, tex.Width, 2);

            LightingBufferRenderer.DrawWithLighting(pos, tex, source, new Color(200, 230, 255) * 0.4f);
            spriteBatch.Draw(tex, pos2, source2, Color.White * 0.6f, 0, Vector2.Zero, 1, 0, 0);
        }

        private static readonly VertexPositionColorTexture[] verticies = new VertexPositionColorTexture[6];

        private static readonly VertexBuffer buffer = new VertexBuffer(Main.instance.GraphicsDevice, typeof(VertexPositionColorTexture), 6, BufferUsage.WriteOnly);

        private static Effect ApplyEffect;

        private void DrawShine(Rectangle target)
        {
            if (Main.dedServ || !Helper.OnScreen(target)) return;

            if (ApplyEffect is null) ApplyEffect = Main.dedServ ? null : ApplyEffect = Filters.Scene["WaterShine"].GetShader().Shader;

            Matrix zoom = new Matrix
                (
                    Main.GameViewMatrix.Zoom.X, 0, 0, 0,
                    0, Main.GameViewMatrix.Zoom.X, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1
                );

            ApplyEffect.Parameters["drawSize"].SetValue(target.Size());
            ApplyEffect.Parameters["colorSampleY"].SetValue(1 - (0.5f + DrawHelper.ConvertY(WaterLevel - Main.screenPosition.Y) / 2f));
            ApplyEffect.Parameters["time"].SetValue(Main.GameUpdateCount / 75f);

            ApplyEffect.Parameters["draw"].SetValue(GetTexture(AssetDirectory.SquidBoss + "WaterOver"));
            ApplyEffect.Parameters["distort"].SetValue(GetTexture(AssetDirectory.SquidBoss + "WaterDistort"));
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

        public static ParticleSystem.Update updateBubbles => UpdateBubblesBody;
        ParticleSystem system = new ParticleSystem(AssetDirectory.SquidBoss + "Bubble", updateBubbles, 3);

        private static void UpdateBubblesBody(Particle particle)
        {
            particle.Timer--;

            particle.StoredPosition.Y += particle.Velocity.Y;
            particle.StoredPosition.X += (float)Math.Sin(StarlightWorld.rottime + particle.GetHashCode()) * 0.45f;
            particle.Position = particle.StoredPosition - Main.screenPosition;
        }

        public void DrawBigWindow(SpriteBatch spriteBatch)
        {
            var drawCheck = new Rectangle(StarlightWorld.SquidBossArena.X * 16 - (int)Main.screenPosition.X, StarlightWorld.SquidBossArena.Y * 16 - (int)Main.screenPosition.Y, StarlightWorld.SquidBossArena.Width * 16, StarlightWorld.SquidBossArena.Height * 16);
            if (!Helper.OnScreen(drawCheck)) return;

            //parallax background
            Texture2D layer0 = GetTexture(AssetDirectory.SquidBoss + "Background0");
            Texture2D layer1 = GetTexture(AssetDirectory.SquidBoss + "Background1");
            Texture2D layer2 = GetTexture(AssetDirectory.SquidBoss + "Background2");

            Vector2 pos = npc.Center;
            Vector2 dpos = pos - Main.screenPosition;
            Rectangle target = new Rectangle((int)dpos.X - 630, (int)dpos.Y - 595 + 60, 1260, 1020);
            Color color = new Color(140, 150, 190);

            spriteBatch.Draw(layer0, target, GetSource(0.2f, layer0), color, 0, Vector2.Zero, 0, 0);
            spriteBatch.Draw(layer1, target, GetSource(0.15f, layer1), color, 0, Vector2.Zero, 0, 0);

            system.DrawParticles(spriteBatch);

            if (Main.rand.Next(4) == 0)
                system.AddParticle(new Particle(Vector2.Zero, Vector2.UnitY * -Main.rand.NextFloat(0.6f, 1.2f), 0, Main.rand.NextFloat(0.4f, 0.8f), Color.White * Main.rand.NextFloat(0.2f, 0.4f), 700, pos + new Vector2(Main.rand.Next(-600, 600), 500)));

            spriteBatch.Draw(layer2, target, GetSource(0.1f, layer2), color, 0, Vector2.Zero, 0, 0);

            spriteBatch.End(); //we have to restart the SB here anyways, so lets use it to draw our BG with primitives

            Texture2D backdrop = GetTexture(AssetDirectory.SquidBoss + "Window");
            LightingBufferRenderer.DrawWithLighting(npc.Center - backdrop.Size() / 2 + new Vector2(0, -114) - Main.screenPosition, backdrop);

            var shinePos = npc.Center - backdrop.Size() / 2 + new Vector2(0, 920 - npc.ai[0]) - Main.screenPosition;
            DrawShine(new Rectangle((int)shinePos.X, (int)shinePos.Y, backdrop.Width, 240));

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            Texture2D glass = GetTexture(AssetDirectory.SquidBoss + "WindowIn");
            Texture2D glass2 = GetTexture(AssetDirectory.SquidBoss + "WindowInGlow");
            spriteBatch.Draw(glass, npc.Center + new Vector2(0, -7 * 16 - 3) - Main.screenPosition, null, Color.White * 0.425f, 0, glass.Size() / 2, 1, 0, 0);
            spriteBatch.Draw(glass2, npc.Center + new Vector2(0, -7 * 16 - 3) - Main.screenPosition, null, Color.White * 0.2f, 0, glass.Size() / 2, 1, 0, 0);

            Texture2D ray = GetTexture(AssetDirectory.SquidBoss + "Godray");

            for (int k = 0; k < 4; k++)
            {
                Color lightColor = new Color(120, 210, 255);
                spriteBatch.Draw(ray, npc.Center + new Vector2(450, -250) - Main.screenPosition, null, lightColor * 0.5f, 0.9f + (float)Math.Sin((npc.ai[2] + k) * 2) * 0.11f, Vector2.Zero, 1.5f, 0, 0);
                spriteBatch.Draw(ray, npc.Center + new Vector2(-450, -250) - Main.screenPosition, null, lightColor * 0.5f, 0.45f + (float)Math.Sin((npc.ai[2] + k) * 2) * 0.11f, Vector2.Zero, 1.5f, 0, 0);

                spriteBatch.Draw(ray, npc.Center + new Vector2(0, -450) - Main.screenPosition, null, lightColor * 0.5f, 0.68f + (float)Math.Sin(npc.ai[2] * 2 + k / 4f * 6.28f) * 0.13f, Vector2.Zero, 1.9f, 0, 0);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            DrawWindow(spriteBatch, new Vector2(0, -70), new Color(200, 255, 255));

            DrawWindow(spriteBatch, new Vector2(-20, -65), new Color(200, 255, 255));
            DrawWindow(spriteBatch, new Vector2(-11, -105), new Color(200, 255, 255));

            DrawWindow(spriteBatch, new Vector2(20, -65), new Color(200, 255, 255));
            DrawWindow(spriteBatch, new Vector2(11, -105), new Color(200, 255, 255));

            spriteBatch.End();

            DrawWindowLit(new Vector2(0, -70));

            DrawWindowLit(new Vector2(-20, -65));
            DrawWindowLit(new Vector2(-11, -105));

            DrawWindowLit(new Vector2(20, -65));
            DrawWindowLit(new Vector2(11, -105));

            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            Texture2D texIn = GetTexture(AssetDirectory.SquidBoss + "SmallWindowIn");

            DrawWindowGlass(spriteBatch, texIn, new Vector2(0, -70));

            DrawWindowGlass(spriteBatch, texIn, new Vector2(-20, -65));
            DrawWindowGlass(spriteBatch, texIn, new Vector2(-11, -105));

            DrawWindowGlass(spriteBatch, texIn, new Vector2(20, -65));
            DrawWindowGlass(spriteBatch, texIn, new Vector2(11, -105));
        }

        private void DrawWindowLit(Vector2 off)
        {
            Texture2D background1 = GetTexture(AssetDirectory.SquidBoss + "Background1");
            Texture2D background2 = GetTexture(AssetDirectory.SquidBoss + "Background2");

            var position = npc.Center + new Vector2(off.X * 16, off.Y * 16) - Main.screenPosition;
            position -= new Vector2(70, 220);
            var target = new Rectangle((int)position.X, (int)position.Y + 105, 140, 428);

            var sourcePos2 = new Vector2(250, 1500) + off * 16 + FindOffset(npc.Center, -0.18f);
            var source2 = new Rectangle((int)sourcePos2.X, (int)sourcePos2.Y, 160, 668);

            var sourcePos0 = new Vector2(500, 1500) + off * 16 + FindOffset(npc.Center, -0.15f);
            var source0 = new Rectangle((int)sourcePos0.X, (int)sourcePos0.Y, 160, 668);

            var sourcePos1 = new Vector2(500, 1800) + off * 16 + FindOffset(npc.Center, -0.1f);
            var source1 = new Rectangle((int)sourcePos1.X, (int)sourcePos1.Y, 160, 668);

            DrawHelper.DrawTriangle(background1, new Vector2[]
            {
                target.TopLeft() + new Vector2(target.Width / 2, -200),
                target.TopRight(),
                target.TopLeft(),
            }, new Vector2[]
            {
                source2.TopLeft() + new Vector2(source2.Width / 2, -200),
                source2.TopRight(),
                source2.TopLeft(),
            }
            );

            DrawHelper.DrawTriangle(background1, new Vector2[]
            {
                target.TopLeft() + new Vector2(target.Width / 2, -200),
                target.TopRight(),
                target.TopLeft(),
            }, new Vector2[]
            {
                source0.TopLeft() + new Vector2(source0.Width / 2, -200),
                source0.TopRight(),
                source0.TopLeft(),
            }
            );

            DrawHelper.DrawTriangle(background2, new Vector2[]
            {
                target.TopLeft() + new Vector2(target.Width / 2, -200),
                target.TopRight(),
                target.TopLeft(),

            }, new Vector2[]
            {
               source1.TopLeft() + new Vector2(source1.Width / 2, -200),
               source1.TopRight(),
               source1.TopLeft(),
            }
            );

            Texture2D texOver = GetTexture(AssetDirectory.SquidBoss + "SmallWindow");

            Vector2 pos = npc.Center - texOver.Size() / 2 + off * 16;
            LightingBufferRenderer.DrawWithLighting(pos - Main.screenPosition, texOver);
        }

        private void DrawWindowGlass(SpriteBatch spriteBatch, Texture2D tex, Vector2 off)
        {
            Vector2 pos = npc.Center - tex.Size() / 2 + off * 16;
            spriteBatch.Draw(tex, pos - Main.screenPosition, Color.White * 0.25f);
        }

        private void DrawWindow(SpriteBatch spriteBatch, Vector2 off, Color color)
        {
            Texture2D background1 = GetTexture(AssetDirectory.SquidBoss + "Background1");
            Texture2D background2 = GetTexture(AssetDirectory.SquidBoss + "Background2");
            Texture2D texUnder = GetTexture(AssetDirectory.SquidBoss + "SmallWindowUnder");

            spriteBatch.Draw(texUnder, npc.Center + new Vector2(off.X * 16, off.Y * 16) - Main.screenPosition, null, new Color(0, 5, 15), 0, texUnder.Size() / 2, 1, 0, 0);

            var position = npc.Center + new Vector2(off.X * 16, off.Y * 16) - Main.screenPosition;
            position -= new Vector2(70, 220);
            var target = new Rectangle((int)position.X, (int)position.Y + 105, 140, 428);

            var sourcePos2 = new Vector2(250, 1500) + off * 16 + FindOffset(npc.Center, -0.18f);
            var source2 = new Rectangle((int)sourcePos2.X, (int)sourcePos2.Y, 160, 668);

            spriteBatch.Draw(background1, target, source2, Color.White);

            var sourcePos0 = new Vector2(500, 1500) + off * 16 + FindOffset(npc.Center, -0.12f);
            var source0 = new Rectangle((int)sourcePos0.X, (int)sourcePos0.Y, 160, 668);

            spriteBatch.Draw(background1, target, source0, Color.White);

            var sourcePos1 = new Vector2(500, 1800) + off * 16 + FindOffset(npc.Center, -0.08f);
            var source1 = new Rectangle((int)sourcePos1.X, (int)sourcePos1.Y, 160, 668);

            spriteBatch.Draw(background2, target, source1, Color.White);

            if (!StarlightWorld.HasFlag(WorldFlags.SquidBossDowned) && !NPC.AnyNPCs(NPCType<SquidBoss>()))
            {
                Texture2D tentacleGlow = GetTexture(AssetDirectory.SquidBoss + "TentacleGlow");
                Texture2D tentacleTop = GetTexture(AssetDirectory.SquidBoss + "TentacleTop");
                Texture2D tentacleMid = GetTexture(AssetDirectory.SquidBoss + "TentacleBody");
                Texture2D squidBody = GetTexture(AssetDirectory.SquidBoss + "BodyPreview");

                float sin = 1 + (float)Math.Sin(StarlightWorld.rottime);
                float cos = 1 + (float)Math.Cos(StarlightWorld.rottime);
                var glowColor = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f);
                var squidColor = new Color(150, 200, 255);

                if (off.X == 0)
                {
                    Vector2 tentaclePos = new Vector2(off.X * 16 + (float)Math.Cos(npc.ai[1] + off.X * 0.2f) * 5, off.Y * 16 + (float)Math.Sin(npc.ai[1] + off.X * 0.2f) * 2);
                    spriteBatch.Draw(squidBody, npc.Center + tentaclePos + new Vector2(-4, 214) - Main.screenPosition, new Rectangle(22, 0, 136, 150), squidColor, 0, new Vector2(64, 50), 1, 0, 0);
                }
                else
                {
                    Vector2 tentaclePos = new Vector2(off.X * 16 + (float)Math.Cos(npc.ai[1] + off.X * 0.2f) * 20, off.Y * 16 + (float)Math.Sin(npc.ai[1] + off.X * 0.2f) * 5);
                    float tentacleRot = (npc.Center + tentaclePos - (npc.Center + off * 16 + new Vector2(0, 200))).ToRotation() + 1.57f;

                    for (int k = 0; k < 32; k++)
                    {
                        var pos = npc.Center + Vector2.Lerp(tentaclePos, off * 16 + new Vector2((float)Math.Sin(npc.ai[1] + k * 0.5f) * 5, 200), k / 20f) - Main.screenPosition;
                        spriteBatch.Draw(tentacleMid, pos, null, squidColor, 0, tentacleMid.Size() / 2, 1, 0, 0);
                    }

                    spriteBatch.Draw(tentacleGlow, npc.Center + tentaclePos - Main.screenPosition, null, glowColor, tentacleRot, tentacleGlow.Size() / 2, 1, 0, 0);
                    spriteBatch.Draw(tentacleTop, npc.Center + tentaclePos - Main.screenPosition, null, squidColor, tentacleRot, tentacleGlow.Size() / 2, 1, 0, 0);
                }
            }

            if (StarlightWorld.cathedralOverlay.fade)
                for (int k = 0; k < 7; k++)
                    Lighting.AddLight(npc.Center + new Vector2(off.X * 16, off.Y * 16) + new Vector2(0, -80 + k * 50), color.ToVector3() * 0.5f);

            Dust d = Dust.NewDustPerfect(npc.Center + off * 16 + new Vector2(Main.rand.Next(-60, 60), 300 - Main.rand.Next(400)), 257, new Vector2(0, -2), 200, color * 1.8f, 1);
            d.noGravity = true;
        }

        private void SpawnPlatform(int x, int y, bool small = false)
        {
            if (small) NPC.NewNPC((int)(npc.Center.X + x), (int)(npc.Center.Y + y), NPCType<IcePlatformSmall>());
            else NPC.NewNPC((int)(npc.Center.X + x), (int)(npc.Center.Y + y), NPCType<IcePlatform>());
        }

        private Rectangle GetSource(float offset, Texture2D tex)
        {
            int x = tex.Width / 2 - 640;
            int y = tex.Height / 2 - 595;
            Vector2 pos = new Vector2(x, y) - FindOffset(npc.Center, offset);
            return new Rectangle((int)pos.X, (int)pos.Y, 1280, 1190);
        }

        private static Vector2 FindOffset(Vector2 basepos, float factor, bool noVertical = false)
        {
            Vector2 origin = Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            float x = (origin.X - basepos.X) * factor;
            float y = (origin.Y - basepos.Y) * factor * 0.4f;
            return new Vector2(x, noVertical ? 0 : y);
        }
    }
}
