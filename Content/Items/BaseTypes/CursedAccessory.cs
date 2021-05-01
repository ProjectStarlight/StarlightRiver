using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.BaseTypes
{
    public abstract class CursedAccessory : SmartAccessory
    {
        private readonly Texture2D Glow = null;
        private Vector2 drawpos = Vector2.Zero;

        private static ParticleSystem.Update UpdateCursed => UpdateCursedBody;
        public static ParticleSystem CursedSystem = new ParticleSystem("StarlightRiver/Assets/GUI/Dark", UpdateCursed);

        protected CursedAccessory(Texture2D glow) : base("Unnamed Cursed Accessory", "You forgot to give this a display name dingus!")
        {
            Glow = glow;
        }

        private static void UpdateCursedBody(Particle particle)
        {
            float alpha = particle.Timer * 0.053f - 0.00088f * (float)Math.Pow(particle.Timer, 2);
            particle.Color = Color.White * alpha;
            particle.Scale *= 0.97f;
            particle.Position += particle.Velocity;
            particle.Timer--;
        }

        public override void PostDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            Color color = Color.White * (float)Math.Sin(StarlightWorld.rottime);
            spriteBatch.Draw(Glow, position, new Rectangle(0, 0, 32, 32), color, 0, origin, scale, SpriteEffects.None, 0);

            Vector2 pos = position + frame.Size() / 4 - Vector2.One + (Vector2.One * Main.rand.Next(12)).RotatedBy(Main.rand.NextFloat(0, 6.28f)) + new Vector2(0, 10);
            CursedSystem.AddParticle(new Particle(pos, new Vector2(0, -0.4f), 0, 1.25f, Color.White * 0.1f, 60, Vector2.Zero));

            drawpos = position + frame.Size() / 4;
        }

        public override bool CanEquipAccessory(Player player, int slot)
        {
            Main.PlaySound(SoundID.NPCHit55);
            Main.PlaySound(SoundID.Item123);
            for (int k = 0; k <= 50; k++)
                CursedSystem.AddParticle(new Particle(drawpos, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(0.75f), 0, 3, Color.White * 0.1f, 60, Vector2.Zero));

            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            TooltipLine line = new TooltipLine(mod, "StarlightRiverCursedWarning", "Cursed, Cannot be removed normally once equipped")
            {
                overrideColor = new Color(200, 100, 255)
            };
            tooltips.Add(line);
        }

		public override bool PreDrawTooltipLine(DrawableTooltipLine line, ref int yOffset)
		{
            if(line.mod == "Terraria" && line.Name == "ItemName")
			{
                var effect = Filters.Scene["CursedTooltip"].GetShader().Shader;

                effect.Parameters["speed"].SetValue(20);
                effect.Parameters["power"].SetValue(0.02f);
                effect.Parameters["uTime"].SetValue(Main.GameUpdateCount / 100f);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, default, default, default, effect, Main.UIScaleMatrix);

                Utils.DrawBorderString(Main.spriteBatch, line.text, new Vector2(line.X, line.Y), Color.Red);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

                return false;
			}

			return base.PreDrawTooltipLine(line, ref yOffset);
		}
	}
}