using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.SpaceEvent
{
	class Thunderbuss : ModItem
	{
        public override string Texture => "StarlightRiver/Assets/Items/SpaceEvent/Thunderbuss";

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
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
		{
            List<NPC> targets = new List<NPC>();

            foreach(NPC npc in Main.npc.Where(n => n.active && !n.immortal && !n.dontTakeDamage && Helpers.Helper.CheckConicalCollision(player.Center, 500, (player.Center - Main.MouseWorld).ToRotation(), 1, n.Hitbox)))
			{
                targets.Add(npc);
			}

            if(targets.Count == 0) //whiff
			{
                Main.PlaySound(SoundID.DD2_BallistaTowerShot, player.Center);

                for (int k = 0; k < 20; k++)
                {
                    float dustRot = (player.Center - Main.MouseWorld).ToRotation() + 1.57f * 1.5f + Main.rand.NextFloat(-0.2f, 0.2f);
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
                mp.offset = Helpers.Helper.CompareAngle((player.Center - Main.MouseWorld).ToRotation(), (player.Center - targets[targetIndex].Center).ToRotation()) * -120;

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
            }

            return false;
		}
	}

    internal class ThunderbussShot : ModProjectile, IDrawAdditive
    {
        private Vector2 startPoint;
        public Vector2 endPoint;
        public Vector2 midPoint;

        private float dist1;
        private float dist2;

        public float offset = 0;
        public float holdRot = 0;

        Vector2 savedPos = Vector2.Zero;
        public NPC target;

        List<Vector2> nodes = new List<Vector2>();

        public override string Texture => AssetDirectory.Invisible;

        public override void SetDefaults()
        {
            projectile.width = 8;
            projectile.height = 8;
            projectile.friendly = true;
            projectile.ranged = true;
            projectile.penetrate = 3;
            projectile.timeLeft = 60;
            projectile.tileCollide = true;
            projectile.ignoreWater = false;
            projectile.magic = true;

            projectile.extraUpdates = 6;
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Electro Arrow");
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

        public override void AI()
        {
            if (projectile.extraUpdates != 0)
                projectile.rotation = projectile.velocity.ToRotation() - MathHelper.PiOver2;
            else
                projectile.Opacity = projectile.timeLeft > 8 ? 1 : projectile.timeLeft / 7f;

            if (projectile.timeLeft == 60)
            {
                Helpers.Helper.PlayPitched("Magic/LightningExplodeShallow", 0.4f, 0.5f, projectile.Center);

                savedPos = projectile.Center;
                startPoint = projectile.Center;       

                dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, endPoint - startPoint);
                dist2 = ApproximateSplineLength(30, midPoint, endPoint - startPoint, endPoint, endPoint - midPoint);
            }

            startPoint = Main.player[projectile.owner].Center + Vector2.UnitX.RotatedBy(holdRot) * 64;
            endPoint = target.Center;
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

                var target = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(nodes[k], prevPos) + 1, 20);
                var target2 = new Rectangle((int)(prevPos.X - Main.screenPosition.X), (int)(prevPos.Y - Main.screenPosition.Y), (int)Vector2.Distance(nodes[k], prevPos) + 1, 100);
                var origin = new Vector2(0, tex.Height / 2);
                var rot = (nodes[k] - prevPos).ToRotation();
                var color = new Color(200, 230, 255) * (projectile.extraUpdates == 0 ? projectile.timeLeft / 15f : 1);

                sb.Draw(tex, target, null, color, rot, origin, 0, 0);
                sb.Draw(tex, target2, null, color * 0.5f, rot, origin, 0, 0);

                if (Main.rand.Next(20) == 0)
                    Dust.NewDustPerfect(prevPos + new Vector2(0, 50), ModContent.DustType<Dusts.GlowLine>(), Vector2.Normalize(nodes[k] - prevPos) * Main.rand.NextFloat(-6, -4), 0, new Color(100, 150, 200), 0.8f);
            }
        }

        public override bool? CanHitNPC(NPC target) => target == this.target;

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            for (int k = 0; k < 20; k++)
            {
                float dustRot = Main.rand.NextFloat(6.28f);
                Dust.NewDustPerfect(target.Center + Vector2.One.RotatedBy(dustRot) * 24 + new Vector2(0, 32), ModContent.DustType<Dusts.GlowLine>(), Vector2.One.RotatedBy(dustRot) * Main.rand.NextFloat(1), 0, new Color(100, 200, 255), 0.5f);
            }

            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 8;
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
}
