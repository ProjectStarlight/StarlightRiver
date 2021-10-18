using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Vitric
{
	class VitricBossBow : ModItem
    {
        public int manaCharge = 0;

        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Coalescence");
            Tooltip.SetDefault("Charge for a volley of brilliant magic\nFully charged shots leech mana when they collide");
        }

        public override void SetDefaults()
        {
            item.damage = 44;
            item.magic = true;
            item.width = 16;
            item.height = 64;
            item.useTime = 6;
            item.useAnimation = 6;
            item.useStyle = ItemUseStyleID.HoldingOut;
            item.noMelee = true;
            item.noUseGraphic = true;
            item.knockBack = 1;
            item.rare = ItemRarityID.Orange;
            item.channel = true;
            item.shoot = ProjectileType<VitricBowProjectile>();
            item.shootSpeed = 0f;
            item.autoReuse = true;
            item.mana = 40;

            item.useTurn = true;
        }

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
            tooltips.Find(n => n.Name == "Speed" && n.mod == "Terraria").text = "Slow charge";
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (!Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<VitricBowProjectile>()))
            {
                Projectile.NewProjectile(position, new Vector2(speedX, speedY) / 4f, item.shoot, damage, knockBack, player.whoAmI);
                manaCharge = 0;
            }

            return false;
        }

		public override void HoldItem(Player player)
		{
			if(manaCharge >= 5)
			{
                manaCharge = 0;
                player.statMana += item.mana;
                CombatText.NewText(player.Hitbox, CombatText.HealMana, item.mana);
			}
		}

		public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
		{
            if (Main.projectile.Any(n => n.active && n.owner == player.whoAmI && n.type == ProjectileType<VitricBowProjectile>()))
                mult = 0;
        }
	}

	internal class VitricBowProjectile : ModProjectile, IDrawAdditive
	{
        private int charge = 0;

        public float chargePercent => charge / 90f;
        Player owner => Main.player[projectile.owner];

        public ref float State => ref projectile.ai[0];
        public ref float Angle => ref projectile.ai[1];

        public override string Texture => AssetDirectory.VitricItem + "VitricBossBow";

        public override void SetDefaults()
		{
            projectile.width = 32;
            projectile.height = 32;
            projectile.tileCollide = false;
		}

		public override void AI()
		{
            if(owner == Main.LocalPlayer)
                Angle = (owner.Center - Main.MouseWorld).ToRotation() + MathHelper.Pi;

            projectile.netUpdate = true;

            projectile.rotation = Angle; 
            projectile.Center = owner.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * 24;
            owner.heldProj = projectile.whoAmI;

            if (owner.channel && State == 0)
            {
                float damageMult = 0.25f + chargePercent * 0.75f;

                if (charge < 75)
                    charge ++;

                if(charge == 1)
                    Projectile.NewProjectile(projectile.Center, Vector2.UnitX, ProjectileType<VitricBowShard>(), (int)(projectile.damage * damageMult), 1, projectile.owner, 0, 1);

                for (int k = 2; k < 4; k++)
				{
                    if (charge == 19 * k + 1)
                    {
                        Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy((k - 1) * 0.3f), ProjectileType<VitricBowShard>(), (int)(projectile.damage * damageMult), 1, projectile.owner, 0, k);
                        Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy((k - 1) * -0.3f), ProjectileType<VitricBowShard>(), (int)(projectile.damage * damageMult), 1, projectile.owner, 0, k);
                    }
				}
            }

            else if (charge > 0)
            {
                State = 1;
                projectile.timeLeft = charge;
                charge -= 4;
            }

            Lighting.AddLight(owner.Center, new Vector3(0.3f + 0.3f * chargePercent, 0.6f + 0.2f * chargePercent, 1) * chargePercent);
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
		{
            var tex = GetTexture(Texture);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, tex.Size() / 2, 1, 0, 0);

            return false;
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
            var texStar = GetTexture(AssetDirectory.Dust + "Aurora");
            var texGlow = GetTexture("StarlightRiver/Assets/Keys/GlowSoft");

            var color1 = new Color(80, 240, 255);
            var color2 = new Color(90, 200, 255);
            var color3 = new Color(180, 220, 255);

            var offset = Vector2.UnitX.RotatedBy(projectile.rotation);

            var prog1 = RangeLerp(chargePercent, 0, 0.3f) + (float)Math.Sin(Main.GameUpdateCount / 20f) * 0.1f;
            var prog2 = RangeLerp(chargePercent, 0.3f, 0.6f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 1) * 0.1f;
            var prog3 = RangeLerp(chargePercent, 0.6f, 0.9f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 2) * 0.1f;

            DrawRing(spriteBatch, projectile.Center + offset * (-30 + prog1 * 40), 1, 1, Main.GameUpdateCount / 40f, prog1, color3);
            DrawRing(spriteBatch, projectile.Center + offset * (-30 + prog2 * 80), 1.5f, 1.5f, -Main.GameUpdateCount / 30f, prog2, color2);
            DrawRing(spriteBatch, projectile.Center + offset * (-30 + prog3 * 120), 2, 2, Main.GameUpdateCount / 20f, prog3, color1);

            var prog4 = RangeLerp(chargePercent, 0.2f, 0.5f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 3) * 0.2f;
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 15f), null, color3 * prog4, Main.GameUpdateCount / 10f, texStar.Size() / 2, prog1 * 0.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, Main.GameUpdateCount / 10f), null, color1 * prog4, Main.GameUpdateCount / 15f, texStar.Size() / 2, prog2 * 0.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, Main.GameUpdateCount / 25f), null, color3 * prog4, Main.GameUpdateCount / 8f, texStar.Size() / 2, prog1 * 0.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 35f), null, color2 * prog4, Main.GameUpdateCount / 6f, texStar.Size() / 2, prog2 * 0.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 20f), null, color1 * prog4, Main.GameUpdateCount / 12f, texStar.Size() / 2, prog3 * 0.2f, 0, 0);

            var prog5 = RangeLerp(chargePercent, 0.5f, 0.8f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 3) * 0.2f;
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 12f), null, color2 * prog5, Main.GameUpdateCount / 18f, texStar.Size() / 2, prog3 * 0.2f * 1.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, -Main.GameUpdateCount / 18f), null, color3 * prog5, Main.GameUpdateCount / 12f, texStar.Size() / 2, prog3 * 0.2f * 1.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, -Main.GameUpdateCount / 42f), null, color1 * prog5, Main.GameUpdateCount / 10f, texStar.Size() / 2, prog2 * 0.2f * 1.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 32f), null, color3 * prog5, Main.GameUpdateCount / 6f, texStar.Size() / 2, prog2 * 0.2f * 1.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 25f), null, color3 * prog5, Main.GameUpdateCount / 19f, texStar.Size() / 2, prog3 * 0.2f * 1.2f, 0, 0);

            spriteBatch.Draw(texGlow, projectile.Center + offset * (-40 + prog2 * 90) - Main.screenPosition, null, color3 * (chargePercent * 0.5f), 0, texGlow.Size() / 2, 3.5f, 0, 0);
        }

        private void DrawRing(SpriteBatch sb, Vector2 pos, float w, float h, float rotation, float prog, Color color) //optimization nightmare. Figure out smth later
		{
            var texRing = GetTexture(AssetDirectory.VitricItem + "BossBowRing");
            var effect = Filters.Scene["BowRing"].GetShader().Shader;

            effect.Parameters["uProgress"].SetValue(rotation);
            effect.Parameters["uColor"].SetValue(color.ToVector3());
            effect.Parameters["uImageSize1"].SetValue(new Vector2(Main.screenWidth, Main.screenHeight));
            effect.Parameters["uOpacity"].SetValue(prog);

            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, effect, Main.GameViewMatrix.ZoomMatrix);

            var target = toRect(pos, (int)(16 * (w + prog)), (int)(60 * (h + prog)));
            sb.Draw(texRing, target, null, color * prog, projectile.rotation, texRing.Size() / 2, 0, 0);

            sb.End();
            sb.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);
        }

        private Rectangle toRect(Vector2 pos, int w, int h)
		{
            return new Rectangle((int)(pos.X - Main.screenPosition.X), (int)(pos.Y - Main.screenPosition.Y), w, h);
		}

        private float RangeLerp(float input, float start, float end)
		{
            if (input < start) 
                return 0;

            return MathHelper.Clamp( Helper.BezierEase((input - start) / (end - start)), 0, 1);
		}

        private Vector2 PosRing(Vector2 center, float w, float h, float rot)
		{
            return center + new Vector2((float)Math.Cos(rot) * h, (float)Math.Sin(rot) * w).RotatedBy(projectile.rotation + MathHelper.PiOver2) - Main.screenPosition;
		}
	}

    internal class VitricBowShard : ModProjectile, IDrawAdditive
	{
        public override string Texture => AssetDirectory.Invisible;

        public Player owner => Main.player[projectile.owner];

        public ref float Timer => ref projectile.ai[0];

        public int fadeIn = 15;

        private Vector2 startPoint;
        private Vector2 startCenter;
        private Vector2 targetPoint;
        private float storedRotation;
        private float targetRotation => (targetPoint - startCenter).ToRotation();
        private float targetDist => Vector2.Distance(targetPoint, startPoint);
        float dist1;
        float dist2;

        Vector2 midPoint => startPoint + Vector2.UnitX.RotatedBy(storedRotation - Helpers.Helper.CompareAngle(storedRotation, targetRotation) * 0.5f) * targetDist / 2f;

		public override void SetDefaults()
		{
            projectile.width = 16;
            projectile.height = 16;
            projectile.tileCollide = false;
            projectile.timeLeft = 122;
            projectile.friendly = true;
            projectile.hostile = false;
            projectile.penetrate = 20;
            projectile.magic = true;

            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanHitNPC(NPC target)
		{
            if (projectile.timeLeft < 120)
                return base.CanHitNPC(target);
            return false;
		}

        private Vector2 PointOnSpline(float progress)
        {
            float factor = dist1 / (dist1 + dist2);

            if (progress < factor)
                return Vector2.Hermite(startPoint, midPoint - startPoint, midPoint, targetPoint - startPoint, progress * (1 / factor));
            if (progress >= factor)
                return Vector2.Hermite(midPoint, targetPoint - startPoint, targetPoint, targetPoint - midPoint, (progress - factor) * (1 / (1 - factor)));

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
            if (owner.channel && projectile.timeLeft >= 120)
            {
                if (Timer < fadeIn)
                    Timer++;

                projectile.rotation = projectile.velocity.ToRotation() + (owner.Center - Main.MouseWorld).ToRotation() + 3.14f;

                projectile.timeLeft = 121;
                projectile.Center = owner.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * (80 + (float)Math.Sin(Main.GameUpdateCount / 10f + projectile.velocity.X * 6) * 10);
            }

            else if (Timer >= fadeIn)
            {
                Timer++;

                if(Timer == 16)
				{
					projectile.rotation = projectile.velocity.ToRotation() + (owner.Center - Main.MouseWorld).ToRotation() + 3.14f;
                    projectile.Center = owner.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * (80 + (float)Math.Sin(Main.GameUpdateCount / 10f + projectile.velocity.X * 6) * 10);
                }

                if (startPoint == Vector2.Zero)
                {
                    if (owner == Main.LocalPlayer)
                        targetPoint = Main.MouseWorld;

                    startPoint = projectile.Center;
                    startCenter = owner.Center;
                    storedRotation = projectile.rotation;
                    dist1 = ApproximateSplineLength(30, startPoint, midPoint - startPoint, midPoint, targetPoint - startPoint);
                    dist2 = ApproximateSplineLength(30, midPoint, targetPoint - startPoint, targetPoint, targetPoint - midPoint);
                }

                int lifeTime = 122 - projectile.timeLeft;
                int timeToMerge = (int)(Math.Min(0.4f, targetDist / 1200f) * 90);

                if (lifeTime < timeToMerge)
                {
                    float progress = lifeTime / (float)timeToMerge;
                    projectile.Center = PointOnSpline(progress);
                    projectile.rotation = (projectile.Center - PointOnSpline(progress + 0.05f)).ToRotation();

                    projectile.velocity = Vector2.Zero;

                    if (Main.rand.Next(4) == 0)
                    {
                        var color = new Color(20 + (int)(projectile.ai[1] / 4f * 100), 150, 255);
                        var d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.8f, 1.4f));
                        d.customData = Main.rand.NextFloat(0.4f, 1.5f);
                        d.fadeIn = 30;
                    }

                    if (Main.rand.Next(2) == 0)
                    {
                        var color = new Color(20 + (int)(projectile.ai[1] / 4f * 100), 150, 255);
                        var d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.4f, 0.6f));
                        d.customData = Main.rand.NextFloat(0.4f, 0.8f);
                        d.fadeIn = 30;
                    }
                }
                else
                {
                    projectile.velocity = Vector2.UnitX.RotatedBy(targetRotation) * 15 * (1 - (lifeTime - timeToMerge) / (122f - timeToMerge));
                    projectile.rotation = targetRotation;

                    var color = new Color(20 + (int)(projectile.ai[1] / 4f * 100), 150, 255);

                    if (projectile.timeLeft < 30)
                        color = color * (projectile.timeLeft / 30f);

                    if (Main.rand.Next(10) == 0)
                    {
                        var d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.8f, 1.4f));
                        d.customData = Main.rand.NextFloat(0.4f, 1.5f);
                        d.fadeIn = 30;
                    }

                    if (Main.rand.Next(5) == 0)
                    {
                        var d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.4f, 0.6f));
                        d.customData = Main.rand.NextFloat(0.4f, 0.8f);
                        d.fadeIn = 30;
                    }
                }

                if(projectile.ai[1] != 1 && lifeTime == timeToMerge)
				{
                    for(int k = 0; k < 10; k++)
                        Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Glow>(), Vector2.One.RotatedByRandom(6.28f) * 5, 0, new Color(100, 200, 255), 0.25f);
				}
            }
            else
            {
                Timer -= 2;

                if (Timer <= 0)
                    projectile.timeLeft = 0;
            }
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
		{
            int lifeTime = 122 - projectile.timeLeft;
            int timeToMerge = (int)(Math.Min(0.4f, targetDist / 1200f) * 90);

            if (Math.Abs(lifeTime - timeToMerge) <= 5 && owner.HeldItem.modItem is VitricBossBow)
            {
                var mi = (owner.HeldItem.modItem as VitricBossBow);
                mi.manaCharge++;

                if (mi.manaCharge >= 5)
                {
                    Helper.PlayPitched("Magic/HolyCastShort", 1, 0, projectile.Center);

                    var d = Dust.NewDustPerfect(projectile.Center, DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(50, 150, 255), 1);
                    d.customData = 3f;
                    d.rotation = Main.rand.NextFloat(6.28f);
                }
            }
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
            var tex = GetTexture(AssetDirectory.MiscTextures + "DirectionalBeam");
            var tex2 = GetTexture(AssetDirectory.VitricItem + "BossBowArrow");
            var color = new Color(100 + (int)(projectile.ai[1] / 4f * 100), 200, 255);

            if (projectile.timeLeft < 30)
                color *= (projectile.timeLeft / 30f);

            spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, color * (Math.Min(Timer / (float)fadeIn, 1)), projectile.rotation + 1.57f, tex2.Size() / 2, 0.5f, 0, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color * (Math.Min(Timer / (float)fadeIn, 1) * 0.5f), projectile.rotation, new Vector2(tex.Width / 4f, tex.Height / 2f), 2, 0, 0);
		}
	}
}
