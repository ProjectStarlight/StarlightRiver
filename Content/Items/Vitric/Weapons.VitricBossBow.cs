using Microsoft.Xna.Framework;
using System.Linq;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using StarlightRiver.Core;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Dusts;
using Terraria.Graphics.Effects;
using System.Collections.Generic;

namespace StarlightRiver.Content.Items.Vitric
{
    class VitricBossBow : ModItem
    {
        public override string Texture => AssetDirectory.VitricItem + Name;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Coalescence");
            Tooltip.SetDefault("Charge for a volley of brilliant magic");
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

            item.useTurn = true;
        }

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
            tooltips.Find(n => n.Name == "Speed" && n.mod == "Terraria").text = "Slow charge";
		}

		public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            if (!Main.projectile.Any(n => n.active && n.type == ProjectileType<VitricBowProjectile>()))
                Projectile.NewProjectile(position, new Vector2(speedX, speedY) / 4f, item.shoot, damage, knockBack, player.whoAmI);

            return false;
        }
    }

	internal class VitricBowProjectile : ModProjectile, IDrawAdditive
	{
        public float chargePercent => charge / 120f;

        public override string Texture => AssetDirectory.VitricItem + "VitricBossBow";

        Player owner => Main.player[projectile.owner];

        private int charge = 0;

        public override void SetDefaults()
		{
            projectile.width = 32;
            projectile.height = 32;
            projectile.tileCollide = false;
		}

		public override void AI()
		{
            projectile.rotation = (owner.Center - Main.MouseWorld).ToRotation() + MathHelper.Pi;
            projectile.Center = owner.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * 24;
            owner.heldProj = projectile.whoAmI;

            if (owner.channel && projectile.ai[0] == 0)
            {                          
                if (charge < 120)
                    charge++;

                for (int k = 1; k <= 4; k++)
				{
                    if (charge == 30 * k + 1)
					{
                        Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy((k - 1) * 0.3f), ProjectileType<VitricBowShard>(), projectile.damage, 1, projectile.owner, 0, k);

                        if(k > 1)
                            Projectile.NewProjectile(projectile.Center, Vector2.UnitX.RotatedBy((k - 1) * -0.3f), ProjectileType<VitricBowShard>(), projectile.damage, 1, projectile.owner, 0, k);
                    }
				}
            }

            else if (charge > 0)
            {
                projectile.ai[0] = 1;
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
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 15f), null, color3 * prog4, Main.GameUpdateCount / 10f, texStar.Size() / 2, prog1, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, Main.GameUpdateCount / 10f), null, color1 * prog4, Main.GameUpdateCount / 15f, texStar.Size() / 2, prog2, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, Main.GameUpdateCount / 25f), null, color3 * prog4, Main.GameUpdateCount / 8f, texStar.Size() / 2, prog1, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 35f), null, color2 * prog4, Main.GameUpdateCount / 6f, texStar.Size() / 2, prog2, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog4 * 60), 20, 80, -Main.GameUpdateCount / 20f), null, color1 * prog4, Main.GameUpdateCount / 12f, texStar.Size() / 2, prog3, 0, 0);

            var prog5 = RangeLerp(chargePercent, 0.5f, 0.8f) + (float)Math.Sin(Main.GameUpdateCount / 20f + 3) * 0.2f;
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 12f), null, color2 * prog5, Main.GameUpdateCount / 18f, texStar.Size() / 2, prog3 * 1.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, -Main.GameUpdateCount / 18f), null, color3 * prog5, Main.GameUpdateCount / 12f, texStar.Size() / 2, prog3 * 1.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, -Main.GameUpdateCount / 42f), null, color1 * prog5, Main.GameUpdateCount / 10f, texStar.Size() / 2, prog2 * 1.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 32f), null, color3 * prog5, Main.GameUpdateCount / 6f, texStar.Size() / 2, prog2 * 1.2f, 0, 0);
            spriteBatch.Draw(texStar, PosRing(projectile.Center + offset * (-30 + prog5 * 100), 25, 100, Main.GameUpdateCount / 25f), null, color3 * prog5, Main.GameUpdateCount / 19f, texStar.Size() / 2, prog3 * 1.2f, 0, 0);

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

        private float storedAngle = 6;
        private float storedAngle2 = 0;
        private float storedAngle3 = 0;

		public override void SetDefaults()
		{
            projectile.width = 16;
            projectile.height = 16;
            projectile.tileCollide = false;
            projectile.timeLeft = 122;
            projectile.friendly = true;
            projectile.penetrate = 20;

            projectile.usesLocalNPCImmunity = true;
            projectile.localNPCHitCooldown = -1;
        }

        public override bool? CanHitNPC(NPC target)
		{
            return projectile.timeLeft < 120;
		}

		public override void AI()
		{
            Timer++;

            if (owner.channel && projectile.timeLeft >= 120)
			{
                projectile.rotation = projectile.velocity.ToRotation() + (owner.Center - Main.MouseWorld).ToRotation() + 3.14f;

                projectile.timeLeft = 121;
                projectile.Center = owner.Center + Vector2.UnitX.RotatedBy(projectile.rotation) * (80 + (float)Math.Sin(Main.GameUpdateCount / 10f + projectile.velocity.X * 6) * 10);

                storedAngle2 = (owner.Center - Main.MouseWorld).ToRotation() + 3.14f;
            }
			else if (Timer >= 30)
			{
                if (storedAngle == 6)
                    storedAngle = projectile.velocity.ToRotation();

                projectile.velocity = Vector2.UnitX.RotatedBy(projectile.rotation) * 25;

                projectile.rotation = projectile.velocity.ToRotation();

                if (projectile.timeLeft > 90)
                    projectile.rotation -= storedAngle * 0.075f;

                if (projectile.timeLeft == 90)
                    storedAngle3 = projectile.rotation;

                if (projectile.timeLeft < 100 && projectile.timeLeft >= 85)
                    projectile.rotation += storedAngle * 0.085f;


                if (Main.rand.Next(4) == 0)
                {
                    var color = new Color(100 + (int)(projectile.ai[1] / 4f * 100), 200, 255);
                    var d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.8f, 1.4f));
                    d.customData = Main.rand.NextFloat(0.4f, 1.5f);
                    d.fadeIn = 30;
                }

                if (Main.rand.Next(2) == 0)
                {
                    var color = new Color(100 + (int)(projectile.ai[1] / 4f * 100), 200, 255);
                    var d = Dust.NewDustPerfect(projectile.Center + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(10), DustType<Dusts.Aurora>(), Vector2.Zero, 0, color * Main.rand.NextFloat(0.4f, 0.6f));
                    d.customData = Main.rand.NextFloat(0.4f, 0.8f);
                    d.fadeIn = 30;
                }
            }
			else
			{
                Timer -= 2;

                if (Timer <= 0)
                    projectile.timeLeft = 0;
			}
		}

        public void DrawAdditive(SpriteBatch spriteBatch)
		{
            var tex = GetTexture(AssetDirectory.MiscTextures + "DirectionalBeam");
            var tex2 = GetTexture(AssetDirectory.VitricItem + "BossBowArrow");
            var color = new Color(100 + (int)(projectile.ai[1] / 4f * 100), 200, 255);

            spriteBatch.Draw(tex2, projectile.Center - Main.screenPosition, null, color * (Math.Min(Timer / 30f, 1)), projectile.rotation + 1.57f, tex2.Size() / 2, 0.5f, 0, 0);
            spriteBatch.Draw(tex, projectile.Center - Main.screenPosition, null, color * (Math.Min(Timer / 30f, 1) * 0.5f), projectile.rotation, new Vector2(tex.Width / 4f, tex.Height / 2f), 2, 0, 0);
		}
	}
}
