using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.SpaceEvent
{
	class Thunderbuss : ModItem
	{
        public Projectile ball;

        public override string Texture => "StarlightRiver/Assets/Items/SpaceEvent/Thunderbuss";

        public override bool AltFunctionUse(Player player) => !Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ModContent.ProjectileType<ThunderbussBall>());

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
            item.damage = 32;
            item.useTime = 30;
            item.useAnimation = 30;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.magic = true;
            item.mana = 30;
            item.shoot = ModContent.ProjectileType<ThunderbussShot>();
            item.shootSpeed = 10;
            item.value = Item.sellPrice(0, 1, 0, 0);
            item.rare = ItemRarityID.Orange;
        }

		public override Vector2? HoldoutOffset()
		{
			return new Vector2(-10, 0);
		}

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                item.useTime = 60;
                item.useAnimation = 60;
            }
            else
            {
                item.useTime = 30;
                item.useAnimation = 30;         
            }
            return true;
        }

        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            List<NPC> targets = FindTargets(player);

            if (player.altFunctionUse != 2 && targets.Count == 0) //whiff
                mult = 0;
        }


        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
            float aim = (player.Center - Main.MouseWorld).ToRotation();

            if (ball != null && (!ball.active || ball.type != ModContent.ProjectileType<ThunderbussBall>()))
                ball = null;

            if(player.altFunctionUse == 2)
			{
                int i = Projectile.NewProjectile(player.Center - new Vector2(48, 0).RotatedBy(aim), new Vector2(speedX, speedY) * 0.4f, ModContent.ProjectileType<ThunderbussBall>(), damage, 0, player.whoAmI);
                ball = Main.projectile[i];

                Helper.PlayPitched("Magic/LightningExplodeShallow", 0.5f, -0.2f, player.Center);

                return false;
			}

            if(ball != null && player == Main.LocalPlayer && Vector2.Distance(ball.Center, Main.MouseWorld) < 128)
			{
                int i = Projectile.NewProjectile(player.Center, Vector2.Zero, type, damage, knockBack, player.whoAmI);

                var mp = Main.projectile[i].modProjectile as ThunderbussShot;

                mp.holdRot = (Main.MouseWorld - player.Center).ToRotation();
                mp.projTarget = ball;
                mp.power = 30;

                ball.ai[1] = 1;

                return false;
			}

            List<NPC> targets = FindTargets(player);

            if(targets.Count == 0) //whiff
			{
                Main.PlaySound(SoundID.DD2_BallistaTowerShot, player.Center);

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
                int i = Projectile.NewProjectile(player.Center, Vector2.Zero, type, damage, knockBack, player.whoAmI);

                var mp = Main.projectile[i].modProjectile as ThunderbussShot;
                mp.offset = Helpers.Helper.CompareAngle(aim, (player.Center - targets[targetIndex].Center).ToRotation()) * -120;

                if(targets.Count == 1)
				{
                    if (k == 1) mp.offset += 50f;
                    if (k == 2) mp.offset -= 50f;
                }

                if (targets.Count == 2)
                {
                    if (k == 2) mp.offset += 50f;
                }

                mp.offset *= Vector2.Distance(targets[targetIndex].Center, player.Center) / 500f;

                mp.holdRot = (Main.MouseWorld - player.Center).ToRotation();
                mp.target = targets[targetIndex];
                mp.power = 20;
            }

            return false;
		}

        private List<NPC> FindTargets(Player player)
        {
            List<NPC> targets = new List<NPC>();
            float aim = (player.Center - Main.MouseWorld).ToRotation();

            foreach (NPC npc in Main.npc.Where(n => n.active &&
             !n.dontTakeDamage &&
             !n.townNPC &&
             Helper.CheckConicalCollision(player.Center, 500, aim, 1, n.Hitbox) &&
             Utils.PlotLine((n.Center / 16).ToPoint16(), (player.Center / 16).ToPoint16(), (x, y) => Framing.GetTileSafely(x, y).collisionType != 1)))
            {
                targets.Add(npc);
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

        public float offset = 0;
        public float holdRot = 0;

        public NPC target;

        private List<Vector2> cache;
        private Trail trail;

        private bool manuallyFoundTarget = false;

        private float dist1;
        private float dist2;

        Vector2 savedPos = Vector2.Zero;
        List<Vector2> nodes = new List<Vector2>();

        public override string Texture => AssetDirectory.Invisible;

        public override bool? CanHitNPC(NPC target) => target == this.target;

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.timeLeft = 60;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.magic = true;
            projectile.penetrate = 2;
            projectile.extraUpdates = 6;

            projectile.usesIDStaticNPCImmunity = true;
            projectile.idStaticNPCHitCooldown = 30;
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

        private NPC FindTarget()
		{
            List<NPC> targets = new List<NPC>();

            foreach (NPC npc in Main.npc.Where(n => n.active &&
             !n.dontTakeDamage &&
             !n.townNPC &&
             Vector2.Distance(projectile.Center, n.Center) < 500 &&
             Utils.PlotLine((n.Center / 16).ToPoint16(), (projectile.Center / 16).ToPoint16(), (x, y) => Framing.GetTileSafely(x, y).collisionType != 1)))
            {
                targets.Add(npc);
            }

            if (targets.Count == 0)
                return null;

            manuallyFoundTarget = true;
            return targets[Main.rand.Next(targets.Count)];
        }

        public override void AI()
        {
            ManageCaches();
            ManageTrails();

            if (target is null)
                target = FindTarget();

            if (target is null)
                projectile.active = false;

            if (projectile.extraUpdates != 0)
                projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
            else
                projectile.Opacity = projectile.timeLeft > 8 ? 1 : projectile.timeLeft / 7f;

            if (projectile.timeLeft == 60)
            {
                Helper.PlayPitched("Magic/LightningExplodeShallow", 0.2f * (power / 20f), 0.5f, projectile.Center);

                savedPos = projectile.Center;
                startPoint = projectile.Center;       

                dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
                dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
            }

            if (!manuallyFoundTarget)
            {
                if (projOwner is null)
                    startPoint = Main.player[projectile.owner].Center + Vector2.UnitX.RotatedBy(holdRot) * 48;
                else
                    startPoint = projOwner.Center;
            }

            if (projTarget is null)
                endPoint = target.Center;
            else
                endPoint = projTarget.Center;

            midPoint = Vector2.Lerp(startPoint, endPoint, 0.5f) + Vector2.Normalize(endPoint - startPoint).RotatedBy(1.57f) * (offset + (float)Math.Sin(Main.GameUpdateCount * 0.2f) * 10);

            projectile.Center = endPoint;

            if (Main.GameUpdateCount % 1 == 0) //rebuild electricity nodes
            {
                nodes.Clear();

                var point1 = startPoint;
                var point2 = projectile.Center;
                int nodeCount = (int)Vector2.Distance(point1, point2) / 30;

                for (int k = 1; k < nodeCount; k++)
                {
                    nodes.Add(PointOnSpline( k / (float)nodeCount) +
                        (k == nodes.Count - 1 ? Vector2.Zero : Vector2.Normalize(point1 - point2).RotatedBy(1.58f) * (Main.rand.NextFloat(2) - 1) * 30 / 3));
                }

                nodes.Add(point2);
            }

            if (projectile.timeLeft == 1)
                PreKill(projectile.timeLeft);
        }

        public void DrawAdditive(SpriteBatch sb)
        {
            var point1 = startPoint;
            var point2 = projectile.Center;

            if (point1 == Vector2.Zero || point2 == Vector2.Zero)
                return;

            var tex = ModContent.GetTexture("StarlightRiver/Assets/GlowTrail");

            for (int k = 1; k < nodes.Count; k++)
            {
                Vector2 prevPos = k == 1 ? point1 : nodes[k - 1];

                var target = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(nodes[k], prevPos) + 1, power);
                var origin = new Vector2(0, tex.Height / 2);
                var rot = (nodes[k] - prevPos).ToRotation();
                var color = new Color(200, 230, 255) * (projectile.extraUpdates == 0 ? projectile.timeLeft / 15f : 1);

                sb.Draw(tex, target, null, color, rot, origin, 0, 0);

                if (Main.rand.Next(20) == 0)
                    Dust.NewDustPerfect(prevPos + new Vector2(0, 30), ModContent.DustType<Dusts.GlowLine>(), Vector2.Normalize(nodes[k] - prevPos) * Main.rand.NextFloat(-3, -2), 0, new Color(100, 150, 200) * (power / 30f), 0.5f);
            }
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();

                for (int i = 0; i < 50; i++)
                {
                    cache.Add(projectile.Center);
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

                return new Color(160, 220, 255) * 0.05f * (projectile.extraUpdates == 0 ? projectile.timeLeft / 15f : 1);
            });

            trail.Positions = cache.ToArray();
            trail.NextPosition = projectile.Center;
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
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/GlowTrail"));

            trail?.Render(effect);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            for (int k = 0; k < 20; k++)
            {
                float dustRot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(target.Center + Vector2.One.RotatedBy(dustRot) * 24 + new Vector2(0, 32), ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(1), 0, new Color(100, 200, 255), 0.5f);
            }

            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += power / 4;
            projectile.damage = 0;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (projectile.extraUpdates == 0)
                return true;

            projectile.velocity *= 0;
            projectile.friendly = false;
            projectile.timeLeft = 15;
            projectile.extraUpdates = 0;

            return false;
        }

        public override bool PreKill(int timeLeft)
        {
            if (projectile.extraUpdates == 0)
                return true;

            projectile.velocity *= 0;
            projectile.friendly = false;
            projectile.timeLeft = 15;
            projectile.extraUpdates = 0;

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

        public ref float Stacks => ref projectile.ai[0];
        public ref float ShouldFire => ref projectile.ai[1];

		public override bool? CanHitNPC(NPC target)
		{
            if (projectile.timeLeft > 30 && Helper.CheckCircularCollision(projectile.Center, 64, target.Hitbox))
            {
                projectile.tileCollide = false;
                projectile.timeLeft = 30;
                projectile.velocity *= 0;

                return true;
            }

            return null;
		}

		public override void SetDefaults()
		{
            projectile.width = 1;
            projectile.height = 1;
            projectile.timeLeft = 600;
            projectile.magic = true;
            projectile.penetrate = -1;
            projectile.damage = 0;
		}

		public override void AI()
		{
            projectile.velocity.Y += 0.015f;

            if(Stacks < 1.5f)
                Stacks += 0.04f;

            ManageCaches();
            ManageTrails();

			if (projectile.timeLeft == 29)
			{
				for (int k = 0; k < 50; k++)
				{
					Dust.NewDustPerfect(projectile.Center + new Vector2(0, 100), ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 6), 0, new Color(100, 200, 255), 1.3f);
                }

                for(int k = 0; k < 20; k++)
				{
                    Dust.NewDustPerfect(projectile.Center + new Vector2(0, 50), ModContent.DustType<Dusts.LightningBolt>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(3, 6), 0, new Color(100, 200, 255), 0.8f);
                }

				Helper.PlayPitched("Magic/LightningCast", 0.5f, 0.9f, projectile.Center);
                Helper.PlayPitched("Magic/LightningExplode", 0.5f, 0.9f, projectile.Center);
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 40;
            }

            if (projectile.timeLeft == 20)
            {
                projectile.damage *= 2;
                projectile.width = 300;
                projectile.height = 300;

                projectile.position -= Vector2.One * 150;
                projectile.friendly = true;
            }

            if (projectile.timeLeft <= 30)
			{

                return;
			}

            Dust.NewDustPerfect(projectile.Center + new Vector2(0, 16), ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(4), 0, new Color(100, 200, 255), 0.3f);

            if (ShouldFire > 0)
			{
                for(int k = 0; k < Main.maxNPCs; k++)
				{
                    var npc = Main.npc[k];
                    if(npc.active && npc.CanBeChasedBy(this) && Helpers.Helper.CheckCircularCollision(projectile.Center, (int)(150 * Stacks), npc.Hitbox))
					{
                        int i = Projectile.NewProjectile(projectile.Center, Vector2.Zero, ModContent.ProjectileType<ThunderbussShot>(), projectile.damage, 0, projectile.owner);
                        var proj = Main.projectile[i].modProjectile as ThunderbussShot;

                        proj.target = npc;
                        proj.projOwner = projectile;
                        proj.power = 15;
					}
				}

                ShouldFire = 0;
			}
		}

		public override bool OnTileCollide(Vector2 oldVelocity)
		{
            if (projectile.timeLeft > 30)
            {
                projectile.tileCollide = false;
                projectile.timeLeft = 30;
                projectile.velocity *= 0;
            }
            return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
            float scale = 0;
            float opacity = 1;

            var tex = ModContent.GetTexture("StarlightRiver/Assets/Keys/GlowSoft");
            var texRing = ModContent.GetTexture("StarlightRiver/Assets/Bosses/VitricBoss/BombTell");

			if (projectile.timeLeft <= 30)
			{
				scale = Helper.SwoopEase(1 - projectile.timeLeft / 30f);
				opacity = Helper.SwoopEase(projectile.timeLeft / 30f);

				spriteBatch.Draw(texRing, projectile.Center - Main.screenPosition, null, new Color(160, 230, 255) * 0.8f * (projectile.timeLeft / 30f), 0, texRing.Size() / 2, (1 - projectile.timeLeft / 30f) * 1.4f, 0, 0);
			}

            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(160, 230, 255) * opacity, 0, tex.Size() / 2, (1.5f + scale * 3) * (Stacks / 1.5f), 0, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, new Color(200, 230, 255) * opacity, 0, tex.Size() / 2, (1f + scale * 2) * (Stacks / 1.5f), 0, 0);

            if(projectile.timeLeft > 30)
			    spriteBatch.Draw(texRing, projectile.Center - Main.screenPosition, null, new Color(120, 200, 255) * 0.4f * opacity, 0, texRing.Size() / 2, 0.75f * Stacks, 0, 0);
        }

        private void ManageCaches()
        {
            if (cache == null)
            {
                cache = new List<Vector2>();
                cache2 = new List<Vector2>();

                for (int i = 0; i < 10; i++)
                {
                    cache.Add(projectile.Center);
                    cache2.Add(projectile.Center);
                }
            }

            for (int i = 0; i < 10; i++)
            {
                float rad = 35 * (Stacks / 1.5f);

                if (projectile.timeLeft <= 30)
                    rad += Helper.SwoopEase((30 - projectile.timeLeft) / 30f) * 80;

                var baseOffset = Vector2.UnitX.RotatedBy(Main.GameUpdateCount * 0.15f + (i / 10f) * 5) * rad;
				cache.Add(projectile.Center + new Vector2(baseOffset.X, baseOffset.Y * 0.4f));

                var baseOffset2 = Vector2.UnitX.RotatedBy(Main.GameUpdateCount * 0.15f + 3.14f + (i / 10f) * 5) * rad;
                cache2.Add(projectile.Center + new Vector2(baseOffset2.X * 0.4f, baseOffset2.Y));
            }

            while (cache.Count > 10)
            {
                cache.RemoveAt(0);
                cache2.RemoveAt(0);
            }
        }

		private void ManageTrails()
		{
			trail = trail ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => 10 + factor * 4 + (projectile.timeLeft <= 30 ? Helper.SwoopEase(1 - projectile.timeLeft / 30f) * 30 : 0), factor =>
			{
				if (factor.X > 0.95f)
					return Color.Transparent;

                float mul = 1;
                if (projectile.timeLeft < 30)
                    mul = Helper.SwoopEase(projectile.timeLeft / 30f);


                return new Color(100, 220, 255) * factor.X * (0.5f + (float)Math.Sin(Main.GameUpdateCount * 0.15f) * 0.25f) * mul;
			});

			trail.Positions = cache.ToArray();
			trail.NextPosition = projectile.Center + Vector2.UnitX.RotatedBy(Main.GameUpdateCount * 0.1f + (11 / 10f) * 3) * 60;

            trail2 = trail2 ?? new Trail(Main.instance.GraphicsDevice, 10, new TriangularTip(40 * 4), factor => 10 + factor * 4 + (projectile.timeLeft <= 30 ? Helper.SwoopEase(1 - projectile.timeLeft / 30f) * 30 : 0), factor =>
            {
                if (factor.X > 0.95f)
                    return Color.Transparent;

                float mul = 1;
                if (projectile.timeLeft < 30)
                    mul = Helper.SwoopEase(projectile.timeLeft / 30f);

                return new Color(100, 220, 255) * factor.X * (0.5f + (float)Math.Cos(Main.GameUpdateCount * 0.15f + 3.14f) * 0.25f) * mul;
            });

            trail2.Positions = cache2.ToArray();
            trail2.NextPosition = projectile.Center + Vector2.UnitY.RotatedBy(Main.GameUpdateCount * 0.1f + (11 / 10f) * 3) * 60;
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
            effect.Parameters["sampleTexture"].SetValue(ModContent.GetTexture("StarlightRiver/Assets/LightningTrail"));

			trail?.Render(effect);
            trail2?.Render(effect);
        }
    }
}
