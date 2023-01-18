//TODO:
//Negative attack
//Magnet display above head
//Make it a 1 in 50 chance

using StarlightRiver.Content.Items.Dungeon;
using StarlightRiver.Content.Items.Magnet;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Graphics.Effects;
using System;
using StarlightRiver.Content.Dusts;
using static Humanizer.In;

namespace StarlightRiver.Content.NPCs.Misc
{
    public class MagnetizedEnemies : GlobalNPC
    {
        public const int SEGMENTS = 20;

        public int charge = 0;

        public int attackCounter = 0;

        public int attackCycleLength = 400;

        public Vector2 lightningDirection = Vector2.Zero;

        public Vector2 endPoint = Vector2.Zero;

        public Player chargedPlayer = default;

        public Color baseColor = new(200, 230, 255);
        public Color endColor = Color.Purple;

        public List<Vector2> cache;
        public List<Vector2> cache2;

        public Trail trail;
        public Trail trail2;

        public float fade = 1f;


        public override bool InstancePerEntity => true;

        public override void OnSpawn(NPC npc, IEntitySource source)
        {
            if (npc.boss || npc.immortal || npc.dontTakeDamage || npc.friendly || npc.townNPC)
                return;

            if (source is EntitySource_SpawnNPC spawnSource)
            {
                if (Main.npc.Any(n => n.active && n.GetGlobalNPC<MagnetizedEnemies>().charge != 0))
                    return;

                Player player = Main.player.Where(n => n.active && !n.dead).OrderBy(n => n.DistanceSQ(npc.Center)).FirstOrDefault();
                if (player != default && player.HasItem(ModContent.ItemType<Items.Magnet.UnchargedMagnet>()))
                {
                    chargedPlayer = player;
                    charge = Main.rand.NextBool() ? 1 : -1;
                    npc.lifeMax *= 4;
                    npc.life = npc.lifeMax;
                }
            }
        }

        public override void AI(NPC npc)
        {
            if (charge != 0 && Main.rand.NextBool(6))
            {
                Vector2 dir = Main.rand.NextFloat(6.28f).ToRotationVector2();
                Vector2 offset = Main.rand.NextBool(4) ? dir * Main.rand.NextFloat(30) : new Vector2(Main.rand.Next(-35, 35), npc.height / 2);

                float smalLCharge = 0.5f;

                var proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center + offset, dir.RotatedBy(Main.rand.NextFloat(-1, 1)) * 5, ModContent.ProjectileType<CloudstrikeShot>(), 0, 0, chargedPlayer.whoAmI, smalLCharge, 2);
                var mp = proj.ModProjectile as CloudstrikeShot;
                mp.velocityMult = Main.rand.Next(1, 4);

                mp.thickness = 0.45f;
                mp.host = npc;

                mp.baseColor = (charge == -1) ? Color.OrangeRed : Color.Cyan;
            }

            if (charge == 1)
            {
                if (attackCounter++ % attackCycleLength < 200)
                    lightningDirection = npc.DirectionTo(chargedPlayer.Center);

                Vector2 offset = Vector2.Zero;

                for (int k = 0; k < 50; k++)
                {
                    offset = lightningDirection * k * 16;

                    int i = (int)((npc.Center.X + offset.X) / 16);
                    int j = (int)((npc.Center.Y + offset.Y) / 16);
                    Tile testTile = Main.tile[i, j];

                    if (testTile.HasTile && Main.tileSolid[testTile.TileType] && !TileID.Sets.Platforms[testTile.TileType])
                    {
                        break;
                    }
                }

                if (attackCounter % attackCycleLength == 300)
                {
                    Helper.PlayPitched("Magic/LightningExplode", 0.4f, 0f, npc.Center);
                    if (cache != null)
                    {
                        for (int i = 2; i < cache.Count - 2; i++)
                        {
                            Vector2 vel = cache[i].DirectionTo(npc.Center).RotatedByRandom(0.4f) * Main.rand.NextFloat(2, 4);
                            Dust.NewDustPerfect(cache[i] + Main.rand.NextVector2Circular(16, 16), ModContent.DustType<GlowLine>(), vel, 0, Color.Cyan, 0.8f);
                        }
                    }
                    Projectile.NewProjectile(npc.GetSource_FromAI(), npc.Center, Vector2.Zero, ModContent.ProjectileType<EnemyMagnetShot>(), (int)(npc.damage * (Main.expertMode || Main.masterMode ? 0.5f : 1)), 0, default, endPoint.X, endPoint.Y);
                    fade = 4.5f;
                }

                endPoint = npc.Center + offset;

                if (!Main.dedServ)
                {
                    ManageCache(npc);
                    ManageTrails(npc);
                }

                if (fade > 1f)
                {
                    fade -= 0.1f;
                    cache?.ForEach(n => Lighting.AddLight(n, Color.Cyan.ToVector3() * ((fade - 1) * 0.3f)));
                }
            }
        }

        public override void OnKill(NPC npc)
        {
            if (chargedPlayer == default)
                return;
            for (int index = 0; index < 54; ++index)
            {
                if (chargedPlayer.inventory[index].type == ModContent.ItemType<UnchargedMagnet>() && chargedPlayer.inventory[index].stack > 0)
                {
                    chargedPlayer.inventory[index].stack--;
                    if (chargedPlayer.inventory[index].stack <= 0)
                        chargedPlayer.inventory[index].TurnToAir();

                    Item.NewItem(npc.GetSource_Death(), chargedPlayer.Center, ModContent.ItemType<ChargedMagnet>());

                    break;
                }
            }
        }

        public override bool PreDraw(NPC npc, SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            if (charge == 1)
                DrawPrimitives();
            return base.PreDraw(npc, spriteBatch, screenPos, drawColor);
        }

        private void ManageCache(NPC npc)
        {
            cache = new List<Vector2>();

            for (int j = 0; j < SEGMENTS + 1; j++)
            {
                float lerp = j / (float)SEGMENTS;
                cache.Add(Vector2.Lerp(npc.Center, endPoint, lerp));
            }

            cache2 = new List<Vector2>();
            for (int i = 0; i < cache.Count; i++)
            {
                Vector2 point = cache[i];
                Vector2 nextPoint = i == cache.Count - 1 ? endPoint : cache[i + 1];
                Vector2 dir = Vector2.Normalize(nextPoint - point).RotatedBy(Main.rand.NextBool() ? -1.57f : 1.57f);

                if (i > cache.Count - 3 || dir == Vector2.Zero)
                    cache2.Add(point);
                else
                    cache2.Add(point + dir * Main.rand.NextFloat(8) * (fade - 1));
            }
        }

        private void ManageTrails(NPC npc)
        {
            Vector2 endPoint = cache[SEGMENTS];
            trail ??= new Trail(Main.instance.GraphicsDevice, SEGMENTS + 1, new TriangularTip(4), factor => 16 * Math.Max(fade, 1), factor =>
            {
                if (factor.X > 0.99f)
                    return Color.Transparent;

                return new Color(160, 220, 255) * ((fade - 0.5f) * 0.3f) * 0.1f * EaseFunction.EaseCubicOut.Ease(1 - factor.X);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = endPoint;

            trail2 ??= new Trail(Main.instance.GraphicsDevice, SEGMENTS + 1, new TriangularTip(4), factor => 3 * (fade > 1 ? Main.rand.NextFloat(0.55f, 1.45f) : 1) * Math.Max(fade, 1), factor =>
            {
                float progress = EaseFunction.EaseCubicOut.Ease(1 - factor.X);
                return Color.Lerp(baseColor, endColor, EaseFunction.EaseCubicIn.Ease(1 - progress)) * ((fade - 0.5f) * 0.3f) * progress;
            });

            trail2.Positions = cache2.ToArray();
            trail2.NextPosition = endPoint;
        }

        public void DrawPrimitives()
        {
            Main.spriteBatch.End();
            Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

            var world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            var projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(1f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

            trail?.Render(effect);
            trail2?.Render(effect);

            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
        }
    }

    internal class EnemyMagnetShot : ModProjectile
    {
        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.timeLeft = 10;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
        }

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electro Shock");
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 endPoint = new Vector2(Projectile.ai[0], Projectile.ai[1]);
            float collisionPoint = 0;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, endPoint, 20, ref collisionPoint);
        }

    }
}
