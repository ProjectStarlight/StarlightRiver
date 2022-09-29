using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.SpaceEvent
{
    class Thunderbuss : ModItem
    {
        public Projectile ball;

        public override string Texture => "StarlightRiver/Assets/Items/SpaceEvent/Thunderbuss";

        public override bool AltFunctionUse(Player Player) => !Main.projectile.Any(n => n.active && n.owner == Player.whoAmI && n.type == ModContent.ProjectileType<ThunderbussBall>());

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Thunderbuss");

            Tooltip.SetDefault("Fires powerful lightning at enemies in a cone\n" +
                "Right click to fire a lightning orb\n" +
                "Shooting at the orb zaps all enemies near it\n" +
                "The orb explodes on impact, and only one may be active at once\n" +
                "'Crush the path of most resistance'");
        }

        public override void SetDefaults()
        {
            Item.damage = 32;
            Item.useTime = 30;
            Item.useAnimation = 30;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 60;
            Item.shoot = ModContent.ProjectileType<ThunderbussShot>();
            Item.shootSpeed = 10;
            Item.value = Item.sellPrice(0, 1, 0, 0);
            Item.rare = ItemRarityID.Orange;
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-10, 0);
        }

        public override bool CanUseItem(Player Player)
        {
            if (Player.altFunctionUse == 2)
            {
                Item.useTime = 60;
                Item.useAnimation = 60;
            }
            else
            {
                Item.useTime = 30;
                Item.useAnimation = 30;
            }
            return true;
        }

        public override void ModifyManaCost(Player Player, ref float reduce, ref float mult)
        {
            List<NPC> targets = FindTargets(Player);

            if (Player.altFunctionUse != 2) //whiff
            {
                if (targets.Count == 0)
                    mult = 0;
                else
                    mult = 0.25f;
            }
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float aim = (player.Center - Main.MouseWorld).ToRotation();

            if (ball != null && (!ball.active || ball.type != ModContent.ProjectileType<ThunderbussBall>()))
                ball = null;

            if (player.altFunctionUse == 2)
            {
                int i = Projectile.NewProjectile(source, player.Center - new Vector2(48, 0).RotatedBy(aim), velocity * 0.4f, ModContent.ProjectileType<ThunderbussBall>(), (int)(damage * 1.25), 0, player.whoAmI);
                ball = Main.projectile[i];

                Helper.PlayPitched("Magic/LightningExplodeShallow", 0.5f, -0.2f, player.Center);

                return false;
            }

            if (ball != null && player == Main.LocalPlayer && Vector2.Distance(ball.Center, Main.MouseWorld) < 128)
            {
                int i = Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, -1);

                var mp = Main.projectile[i].ModProjectile as ThunderbussShot;

                mp.projTarget = ball;
                mp.power = 30;

                mp.Projectile.netUpdate = true;

                (ball.ModProjectile as ThunderbussBall).ShouldFire = true;

                return false;
            }

            List<NPC> targets = FindTargets(player);

            if (targets.Count == 0) //whiff
            {
                Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_BallistaTowerShot, player.Center);

                for (int k = 0; k < 20; k++)
                {
                    float dustRot = aim + 1.57f * 1.5f + Main.rand.NextFloat(-0.2f, 0.2f);
                    Dust.NewDustPerfect(player.Center + Vector2.One.RotatedBy(dustRot) * 80 + new Vector2(0, 32), ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(5), 0, new Color(100, 200, 255), 0.6f);
                }

                return false;
            }

            targets.Sort((a, b) => (int)(Vector2.Distance(a.Center, player.Center) - Vector2.Distance(b.Center, player.Center)));

            for (int k = 0; k < 3; k++)
            {
                int targetIndex = k % targets.Count;

                float offset = Helpers.Helper.CompareAngle(aim, (player.Center - targets[targetIndex].Center).ToRotation()) * -120;

                if (targets.Count == 1)
                {
                    if (k == 1) offset += 50f;
                    if (k == 2) offset -= 50f;
                }

                if (targets.Count == 2)
                {
                    if (k == 2) offset += 50f;
                }

                offset *= Vector2.Distance(targets[targetIndex].Center, player.Center) / 500f;

                int targetId = targets[targetIndex].whoAmI;

                int i = Projectile.NewProjectile(source, player.Center, Vector2.Zero, type, damage, knockback, player.whoAmI, targetId, offset);

                var mp = Main.projectile[i].ModProjectile as ThunderbussShot;

                mp.target = targets[targetIndex];
                mp.power = 20;
            }

            return false;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddIngredient(ModContent.ItemType<Astroscrap>(), 12);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

        private List<NPC> FindTargets(Player Player)
        {
            List<NPC> targets = new List<NPC>();
            float aim = (Player.Center - Main.MouseWorld).ToRotation();

            foreach (NPC NPC in Main.npc.Where(n => n.active &&
             !n.dontTakeDamage &&
             !n.townNPC &&
             Helper.CheckConicalCollision(Player.Center, 500, aim, 1, n.Hitbox) &&
             Utils.PlotLine((n.Center / 16).ToPoint16(), (Player.Center / 16).ToPoint16(), (x, y) => (!Framing.GetTileSafely(x, y).HasTile || !Main.tileSolid[Framing.GetTileSafely(x, y).TileType]) )))
            {
                targets.Add(NPC);
            }
            return targets;
        }

    }

    internal class ThunderbussShot : ModProjectile, IDrawAdditive, IDrawPrimitive
    {
        public Vector2 startPoint;
        public Vector2 endPoint;
        public Vector2 midPoint;

        public int power = 20;
        public Projectile projOwner;
        public Projectile projTarget;

        public bool sentProjOwner = false;

        public ref float targetId => ref Projectile.ai[0];
        public ref float offset => ref Projectile.ai[1];

        public NPC target;

        private List<Vector2> cache;
        private Trail trail;

        private float dist1;
        private float dist2;

        Vector2 savedPos = Vector2.Zero;
        List<Vector2> nodes = new List<Vector2>();

        public override string Texture => AssetDirectory.Invisible;

        public override bool? CanHitNPC(NPC target) => target == this.target;

        public override void SetDefaults()
        {
            Projectile.width = 8;
            Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 60;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = false;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 2;
            Projectile.extraUpdates = 6;

            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 30;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electro Shock");
        }

        private Vector2 PointOnSpline(float progress) //I should really move this spline stuff somewhere central eventually. heh.
        {
            float factor = dist1 / (dist1 + dist2);

            if (progress < factor)
                return Vector2.Hermite(startPoint, midPoint - startPoint, midPoint, endPoint - startPoint, progress * (1 / factor));
            if (progress >= factor)
                return Vector2.Hermite(midPoint, endPoint - startPoint, endPoint, endPoint - midPoint, (progress - factor) * (1 / (1 - factor)));

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

        private void FindTarget()
        {

            if (targetId < 0) //no NPC target which means we are targetting the ball instead, and need to find it
            {
                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == this.Projectile.owner && proj.type == ModContent.ProjectileType<ThunderbussBall>())
                    {
                        power = 30;
                        projTarget = proj;
                        return;
                    }
                }
            }
            else if ((int)targetId == 0) //targetting with ai[0] of zero which 99% of the time means this was fired from a poltergeist minion, so we check if poltergeist minion, then NPC manually
            {
                List<NPC> targets = new List<NPC>();

                for (int i = 0; i < Main.maxProjectiles; i++)
                {
                    Projectile proj = Main.projectile[i];
                    if (proj.active && proj.owner == this.Projectile.owner && proj.type == ModContent.ProjectileType<PoltergeistMinion>() && ((PoltergeistMinion)proj.ModProjectile).Item.type == ModContent.ItemType<Thunderbuss>())
                    {
                        projOwner = proj;
                    }
                }

                if (projOwner is null)
                {
                    //wasn't actually a poltergeist minion so we can just set directly
                    target = Main.npc[(int)targetId];
                    return;
                }


                foreach (NPC NPC in Main.npc.Where(n => n.active &&
                 !n.dontTakeDamage &&
                 !n.townNPC &&
                 n.CanBeChasedBy(projOwner) &&
                 Vector2.Distance(Projectile.Center, n.Center) < 500 &&
                 Utils.PlotLine((n.Center / 16).ToPoint16(), (Projectile.Center / 16).ToPoint16(), (x, y) => Framing.GetTileSafely(x, y).BlockType != BlockType.Solid)))
                {
                    targets.Add(NPC);
                }

                if (targets.Count == 0)
                    return;

                target = targets[Main.rand.Next(targets.Count)];
                targetId = target.whoAmI;
            } else
                target = Main.npc[(int)targetId];
        }

        private void findProjOwner()
        {
            for (int i = 0; i < Main.maxProjectiles; i++)
            {
                Projectile proj = Main.projectile[i];
                if (proj.active && proj.owner == this.Projectile.owner && proj.type == ModContent.ProjectileType<ThunderbussBall>())
                {
                    power = 15;
                    projOwner = proj;
                    return;
                }
            }
        }

        public override void AI()
        {
            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrails();
            }

            if (target is null && projTarget is null)
                FindTarget();

            if (target is null && projTarget is null)
            {
                //failed to find anything so we just skip for this client
                Projectile.active = false;
                return;
            }

            if (offset == 1000000)
            {
                //extremely dirty hack where we abuse the offset field to determine if this has the start anchored to the thunder ball
                if (projOwner is null)
                    findProjOwner();
            }

            if (!sentProjOwner && projOwner != null && Main.myPlayer == Projectile.owner)
            {
                sentProjOwner = true;
                Projectile.netUpdate = true;
            }

            if (Projectile.extraUpdates != 0)
                Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;
            else
                Projectile.Opacity = Projectile.timeLeft > 8 ? 1 : Projectile.timeLeft / 7f;

            if (Projectile.timeLeft == 60)
            {
                Helper.PlayPitched("Magic/LightningExplodeShallow", 0.2f * (power / 20f), 0.5f, Projectile.Center);

                savedPos = Projectile.Center;
                startPoint = Projectile.Center;

                dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
                dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
            }

            float effectiveOffset = offset;

            if (projOwner is null)
            {
                Player player = Main.player[Projectile.owner];
                float armRot = player.itemRotation + (player.direction == -1 ? 3.14f : 0);
                startPoint = player.Center + Vector2.UnitX.RotatedBy(armRot) * 48;
            }
            else
            {
                startPoint = projOwner.Center;
                effectiveOffset = 0;
            }

            if (projTarget is null)
                endPoint = target.Center;
            else
                endPoint = projTarget.Center;

            midPoint = Vector2.Lerp(startPoint, endPoint, 0.5f) + Vector2.Normalize(endPoint - startPoint).RotatedBy(1.57f) * (effectiveOffset + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 10);

            Projectile.Center = endPoint;

            if (Main.GameUpdateCount % 1 == 0) //rebuild electricity nodes
            {
                nodes.Clear();

                var point1 = startPoint;
                var point2 = Projectile.Center;
                int nodeCount = (int)Vector2.Distance(point1, point2) / 30;

                for (int k = 1; k < nodeCount; k++)
                {
                    nodes.Add(PointOnSpline(k / (float)nodeCount) +
                        (k == nodes.Count - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * (Main.rand.NextFloat(2) - 1) * 30 / 3));
                }

                nodes.Add(point2);
            }

            for (int n = 1; n < nodes.Count - 1; n++)
            {
                Vector2 prevPos = n == 1 ? startPoint : nodes[n - 1];
                Vector2 dustVel = Vector2.Normalize(nodes[n] - prevPos) * Main.rand.NextFloat(-3, -2);
                if (Main.rand.Next(20) == 0)
                    Dust.NewDustPerfect(prevPos + new Vector2(0, 30), ModContent.DustType<Dusts.GlowLine>(), dustVel, 0, new Color(100, 150, 200) * (power / 30f), 0.5f);
            }

            if (Projectile.timeLeft == 1)
                PreKill(Projectile.timeLeft);
        }

        public void DrawAdditive(SpriteBatch sb)
        {
            var point1 = startPoint;
            var point2 = Projectile.Center;

            if (point1 == Vector2.Zero || point2 == Vector2.Zero)
                return;

            var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value;

            for (int k = 1; k < nodes.Count; k++)
            {
                Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];

                var target = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(nodes[k], prevPos) + 1, power);
                var origin = new Vector2(0, tex.Height / 2);
                var rot = (nodes[k] - prevPos).ToRotation();
                var color = new Color(200, 230, 255) * (Projectile.extraUpdates == 0 ? Projectile.timeLeft / 15f : 1);

                sb.Draw(tex, target, null, color, rot, origin, 0, 0);
            }
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cache.Add(Projectile.Center);
                }
            }

            for (int i = 0; i < 50; i++)
            {
                cache.Add(PointOnSpline(i / 50f));
            }

            while (cache.Count > 50)
            {
                cache.RemoveAt(0);
            }
        }

        private void ManageTrails()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 50, new TriangularTip(40 * 4), factor => 40 + power, factor =>
            {
                if (factor.X > 0.99f)
                    return Color.Transparent;

                return new Color(160, 220, 255) * 0.05f * (Projectile.extraUpdates == 0 ? Projectile.timeLeft / 15f : 1);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(1f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/GlowTrail").Value);

            trail?.Render(effect);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            for (int k = 0; k < 20; k++)
            {
                float dustRot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(target.Center + Vector2.One.RotatedBy(dustRot) * 24 + new Vector2(0, 32), ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(1), 0, new Color(100, 200, 255), 0.5f);
            }

            Core.Systems.CameraSystem.Shake += power / 4;
            Projectile.damage = 0;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.extraUpdates == 0)
                return true;

            Projectile.velocity *= 0;
            Projectile.friendly = false;
            Projectile.timeLeft = 15;
            Projectile.extraUpdates = 0;

            return false;
        }

        public override bool PreKill(int timeLeft)
        {
            if (Projectile.extraUpdates == 0)
                return true;

            Projectile.velocity *= 0;
            Projectile.friendly = false;
            Projectile.timeLeft = 15;
            Projectile.extraUpdates = 0;

            return false;
        }
    }

    internal class ThunderbussBall : ModProjectile, IDrawAdditive, IDrawPrimitive
    {
        private List<Vector2> cache;
        private Trail trail;

        private List<Vector2> cache2;
        private Trail trail2;

        public override string Texture => AssetDirectory.Invisible;

        public ref float Stacks => ref Projectile.ai[0];


        public bool ShouldFire = false;

        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.timeLeft > 30 && Helper.CheckCircularCollision(Projectile.Center, 64, target.Hitbox))
            {
                Projectile.tileCollide = false;
                Projectile.timeLeft = 30;
                Projectile.velocity *= 0;
                Projectile.netUpdate = true;

                return true;
            }

            return null;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.timeLeft);
            writer.Write(Projectile.tileCollide);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.timeLeft = reader.ReadInt32();
            Projectile.tileCollide = reader.ReadBoolean();
        }

        public override void SetDefaults()
        {
            Projectile.width = 1;
            Projectile.height = 1;
            Projectile.timeLeft = 600;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.damage = 0;
        }

        public override void AI()
        {
            Projectile.velocity.Y += 0.015f;

            if (Stacks < 1.5f)
                Stacks += 0.04f;

            if (Main.netMode != NetmodeID.Server)
            {
                ManageCaches();
                ManageTrails();
            }


            if (Projectile.timeLeft == 29)
            {
                for (int k = 0; k < 50; k++)
                {
                    Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 100), ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 6), 0, new Color(100, 200, 255), 1.3f);
                }

                for (int k = 0; k < 20; k++)
                {
                    Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 50), ModContent.DustType<Dusts.LightningBolt>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 6), 0, new Color(100, 200, 255), 0.8f);
                }

                Helper.PlayPitched("Magic/LightningCast", 0.5f, 0.9f, Projectile.Center);
                Helper.PlayPitched("Magic/LightningExplode", 0.5f, 0.9f, Projectile.Center);
                if (Projectile.owner == Main.myPlayer)
                    Core.Systems.CameraSystem.Shake += 40;
            }

            if (Projectile.timeLeft == 20)
            {
                Projectile.damage *= 2;
                Projectile.width = 300;
                Projectile.height = 300;

                Projectile.position -= Vector2.One * 150;
                Projectile.friendly = true;
            }

            if (Projectile.timeLeft <= 30)
            {
                return;
            }

            Dust.NewDustPerfect(Projectile.Center + new Vector2(0, 16), ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, new Color(100, 200, 255), 0.3f);

            if (ShouldFire)
            {
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    var NPC = Main.npc[k];
                    if (NPC.active && NPC.CanBeChasedBy(this) && Helpers.Helper.CheckCircularCollision(Projectile.Center, (int)(150 * Stacks), NPC.Hitbox))
                    {
                        int i = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<ThunderbussShot>(), Projectile.damage, 0, Projectile.owner, k, 1000000);
                        var proj = Main.projectile[i].ModProjectile as ThunderbussShot;

                        proj.target = NPC;
                        proj.projOwner = Projectile;
                        proj.power = 15;
                    }
                }

                ShouldFire = false;
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.tileCollide = false;
            Projectile.velocity *= 0;
            if (Projectile.timeLeft > 30)
            {
                Projectile.timeLeft = 30;
            }
            return false;
        }

        public void DrawAdditive(SpriteBatch spriteBatch)
        {
            float scale = 0;
            float opacity = 1;

            var tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/GlowSoft").Value;
            var texRing = ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/BombTell").Value;

            if (Projectile.timeLeft <= 30)
            {
                scale = Helper.SwoopEase(1 - Projectile.timeLeft / 30f);
                opacity = Helper.SwoopEase(Projectile.timeLeft / 30f);

                spriteBatch.Draw(texRing, Projectile.Center - Main.screenPosition, null, new Color(160, 230, 255) * 0.8f * (Projectile.timeLeft / 30f), 0, texRing.Size() / 2, (1 - Projectile.timeLeft / 30f) * 1.4f, 0, 0);
            }

            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(160, 230, 255) * opacity, 0, tex.Size() / 2, (1.5f + scale * 3) * (Stacks / 1.5f), 0, 0);
            spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, new Color(200, 230, 255) * opacity, 0, tex.Size() / 2, (1f + scale * 2) * (Stacks / 1.5f), 0, 0);

            if (Projectile.timeLeft > 30)
                spriteBatch.Draw(texRing, Projectile.Center - Main.screenPosition, null, new Color(120, 200, 255) * 0.4f * opacity, 0, texRing.Size() / 2, 0.75f * Stacks, 0, 0);
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                cache2 = new List<Vector2>();

                for (int i = 0; i < 10; i++)
                {
                    cache.Add(Projectile.Center);
                    cache2.Add(Projectile.Center);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                float rad = 35 * (Stacks / 1.5f);

                if (Projectile.timeLeft <= 30)
                    rad += Helper.SwoopEase((30 - Projectile.timeLeft) / 30f) * 80;

                var baseOffset = Vector2.UnitX.RotatedBy(Main.GameUpdateCount * 0.15f + (i / 10f) * 5) * rad;
                cache.Add(Projectile.Center + new Vector2(baseOffset.X, baseOffset.Y * 0.4f));

                var baseOffset2 = Vector2.UnitX.RotatedBy(Main.GameUpdateCount * 0.15f + 3.14f + (i / 10f) * 5) * rad;
                cache2.Add(Projectile.Center + new Vector2(baseOffset2.X * 0.4f, baseOffset2.Y));
            }

            while (cache.Count > 10)
            {
                cache.RemoveAt(0);
                cache2.RemoveAt(0);
            }
        }

        private void ManageTrails()
        {
            trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => 10 + factor * 4 + (Projectile.timeLeft <= 30 ? Helper.SwoopEase(1 - Projectile.timeLeft / 30f) * 30 : 0), factor =>
            {
                if (factor.X > 0.95f)
                    return Color.Transparent;

                float mul = 1;
                if (Projectile.timeLeft < 30)
                    mul = Helper.SwoopEase(Projectile.timeLeft / 30f);


                return new Color(100, 220, 255) * factor.X * (0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.25f) * mul;
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = Projectile.Center + Vector2.UnitX.RotatedBy(Main.GameUpdateCount * 0.1f + (11 / 10f) * 3) * 60;

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => 10 + factor * 4 + (Projectile.timeLeft <= 30 ? Helper.SwoopEase(1 - Projectile.timeLeft / 30f) * 30 : 0), factor =>
            {
                if (factor.X > 0.95f)
                    return Color.Transparent;

                float mul = 1;
                if (Projectile.timeLeft < 30)
                    mul = Helper.SwoopEase(Projectile.timeLeft / 30f);

                return new Color(100, 220, 255) * factor.X * (0.5f + (float)Math.Cos(Main.GameUpdateCount * 0.15f + 3.14f) * 0.25f) * mul;
            });

            trail2.Positions = cache2.ToArray();
            trail2.NextPosition = Projectile.Center + Vector2.UnitY.RotatedBy(Main.GameUpdateCount * 0.1f + (11 / 10f) * 3) * 60;
        }

        public void DrawPrimitives()
        {
            Effect effect = Filters.Scene["LightningTrail"].GetShader().Shader;

            Matrix world = Matrix.CreateTranslation(-Main.screenPosition.Vec3());
            Matrix view = Main.GameViewMatrix.ZoomMatrix;
            Matrix projection = Matrix.CreateOrthographicOffCenter(0, Main.screenWidth, Main.screenHeight, 0, -1, 1);

            effect.Parameters["time"].SetValue(Main.GameUpdateCount * 0.05f);
            effect.Parameters["repeats"].SetValue(1f);
            effect.Parameters["transformMatrix"].SetValue(world * view * projection);
            effect.Parameters["sampleTexture"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/LightningTrail").Value);

            trail?.Render(effect);
            trail2?.Render(effect);
        }
    }
}
