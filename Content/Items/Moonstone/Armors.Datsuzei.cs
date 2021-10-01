using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Helpers;

using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Moonstone
{
    public class Datsuzei : ModItem
    {
        static int activationTimer = 0; //static since this is clientside only and there really shouldnt ever be more than one of these in that context

        public static ParticleSystem sparkles = new ParticleSystem(AssetDirectory.Dust + "Aurora", updateSparkles);

        public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override bool Autoload(ref string name)
		{
            On.Terraria.Main.DrawInterface_30_Hotbar += OverrideHotbar;
            return true;
		}

        private static void updateSparkles(Particle particle)
        {
            particle.Timer--;

            if (particle.Velocity.X == 0)
            {
                particle.Scale = (float)(Math.Sin(particle.Timer / 120f * 3.14f)) * particle.StoredPosition.X;
                particle.Color = new Color(180, 100 + (byte)(particle.Timer / 120f * 155), 255) * (float)(Math.Sin(particle.Timer / 120f * 3.14f));
            }

            else
            {
                particle.Scale = particle.Timer / 60f * particle.StoredPosition.X;
                particle.Color = new Color(180, 100 + (byte)(particle.Timer / 60f * 155), 255) * (particle.Timer / 45f);
                particle.Color *= (float)(0.5f + Math.Sin(particle.Timer / 20f * 6.28f + particle.StoredPosition.Y) * 0.5f);
            }

            particle.Rotation += 0.05f;
            particle.Position += particle.Velocity;
        }

        private void OverrideHotbar(On.Terraria.Main.orig_DrawInterface_30_Hotbar orig, Main self)
		{
            orig(self);

            if(activationTimer > 0 && !Main.playerInventory)
			{
                var activationTimerNoCurve = Datsuzei.activationTimer;
                var activationTimer = Helper.BezierEase(Math.Min(1, activationTimerNoCurve / 60f));

                var hideTarget = new Rectangle(20, 20, 446, 52);
                Main.spriteBatch.Draw(Main.screenTarget, hideTarget, hideTarget, Color.White * activationTimer);

                var backTex = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbar");
                var target = new Rectangle(91, 20, (int)(backTex.Width * activationTimer), backTex.Height);
                var source = new Rectangle(0, 0, (int)(backTex.Width * activationTimer), backTex.Height);

                Main.spriteBatch.Draw(backTex, target, source, Color.White);


                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

                var glowTex = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbarGlow");

                var glowColor = new Color(200, (byte)(255 - 100 * activationTimer), 255);

                if (activationTimerNoCurve > 50)
                    glowColor *= (60 - activationTimerNoCurve) / 10f;

                if (activationTimerNoCurve < 10)
                    glowColor *= activationTimerNoCurve / 10f;

                Main.spriteBatch.Draw(glowTex, target.TopLeft() + new Vector2(target.Width, backTex.Height / 2), null, glowColor, 0, glowTex.Size() / 2, 1, 0, 0);

                if (activationTimer >= 1)
                {
                    var glowTex2 = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbarGlow2");
                    var glowColor2 = new Color(200, (byte)(200 - 50 * (float)Math.Sin(Main.GameUpdateCount * 0.05f)), 255) * (Math.Min(1, (activationTimerNoCurve - 60) / 60f));

                    Main.spriteBatch.Draw(glowTex2, target.Center.ToVector2() + Vector2.UnitY * -1, null, glowColor2 * 0.8f, 0, glowTex2.Size() / 2, 1, 0, 0);
                }

                sparkles.DrawParticles(Main.spriteBatch);

                //the shader for the flames
                var effect1 = Terraria.Graphics.Effects.Filters.Scene["MagicalFlames"].GetShader().Shader;
                effect1.Parameters["sampleTexture1"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap1"));
                effect1.Parameters["sampleTexture2"].SetValue(GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiFlameMap2"));
                effect1.Parameters["uTime"].SetValue(Main.GameUpdateCount * 0.008f);

                if (activationTimerNoCurve > 85)
                {
                    Main.spriteBatch.End();
                    Main.spriteBatch.Begin(default, default, default, default, default, effect1, Main.UIScaleMatrix);

                    var spearTex = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbarSprite");
                    Main.spriteBatch.Draw(spearTex, target.Center() + new Vector2(0, -40), null, Color.White, 0, spearTex.Size() / 2, 1, 0, 0);
                }
                
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

                if (activationTimerNoCurve >= 80)
                {
                    int overlayTime = activationTimerNoCurve - 80;
                    float overlayAlpha = 1;

                    if (overlayTime < 5)
                        overlayAlpha = 1 - overlayTime / 5f;

                    else if (overlayTime <= 25)
                        overlayAlpha = 0;

                    else if (overlayTime > 25)
                        overlayAlpha = (overlayTime - 25) / 15f;

                    else
                        overlayAlpha = 1;

                    var spearShapeTex = GetTexture(AssetDirectory.MoonstoneItem + "DatsuzeiHotbarSpriteShape");
                    Main.spriteBatch.Draw(spearShapeTex, target.Center() + new Vector2(0, -40), null, Color.White * (1 - overlayAlpha), 0, spearShapeTex.Size() / 2, 1, 0, 0);
                }

                //particles!
                if (activationTimer < 1)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        sparkles.AddParticle(
                            new Particle(
                                new Vector2(91 + backTex.Width * activationTimer, 20 + Main.rand.Next(backTex.Height)),
                                new Vector2(Main.rand.NextFloat(-0.6f, -0.3f), Main.rand.NextFloat(3f)),
                                Main.rand.NextFloat(6.28f),
                                1,
                                Color.White,
                                60,
                                new Vector2(Main.rand.NextFloat(0.15f, 0.2f), Main.rand.NextFloat(6.28f)),
                                new Rectangle(0, 0, 100, 100)));
                    }
                }

                if (Main.rand.Next(4) == 0)
                {
                    sparkles.AddParticle(new Particle(new Vector2(91, 20) + new Vector2(Main.rand.Next(backTex.Width), Main.rand.Next(backTex.Height)), new Vector2(0, Main.rand.NextFloat(0.4f)), 0, 0, new Color(255, 230, 0), 120, new Vector2(Main.rand.NextFloat(0.05f, 0.15f), 0.02f), new Rectangle(0, 0, 100, 100)));
                }
			}
		}

		public override void SetStaticDefaults()
		{
            DisplayName.SetDefault("Datsuzei");
            Tooltip.SetDefault("Unleash the fucking moon");
		}

		public override void SetDefaults()
        {
            item.damage = 110;
            item.width = 16;
            item.height = 16;
            item.useStyle = ItemUseStyleID.Stabbing;
            item.useTime = 12;
            item.useAnimation = 12;
            item.crit = 10;
        }

		public override void HoldItem(Player player)
		{
            if (activationTimer < 120)
                activationTimer++;
		}

		public override void UpdateInventory(Player player)
		{
            if (player.HeldItem != item)
            {
                if (activationTimer > 0)
                    activationTimer -= 2;
                else
                {
                    activationTimer = 0;
                    sparkles.ClearParticles();
                }
            }
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips)
		{
            tooltips[0].overrideColor = new Color(100, 255, 255);
		}
	}
}
