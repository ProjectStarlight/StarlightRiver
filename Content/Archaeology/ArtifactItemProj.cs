using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using System;

namespace StarlightRiver.Content.Archaeology
{
	public class ArtifactItemProj : ModProjectile
	{
		public override string Texture => itemTexture;

		public string itemTexture = AssetDirectory.Invisible;

		public Color glowColor = Color.Gold;

		public int itemType = -1;

        public Vector2 size = Vector2.Zero;

        public float fadeIn;

        public float Fade => fadeIn * (float)Math.Pow(Projectile.timeLeft / 340f, 0.3f);


        public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Artifact");
		}
		public override void SetDefaults()
		{
			Projectile.width = 2;
			Projectile.height = 2;
			Projectile.timeLeft = 340;
			Projectile.friendly = false;
		}

        public override void AI()
        {
            if (fadeIn < 1)
                fadeIn+= 0.01f;

            Lighting.AddLight(Projectile.Center + (size / 2), glowColor.ToVector3() * Fade);
        }

        public override bool PreDraw(ref Color lightColor)
        {

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            Texture2D mainTex = ModContent.Request<Texture2D>(Texture).Value;
            var tex2 = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;

            var pos = Projectile.Center + (mainTex.Size() / 2) - Main.screenPosition;
            Main.spriteBatch.Draw(tex2, pos, null, glowColor * (0.9f + ((float)Math.Sin(Main.GameUpdateCount / 50f) * 0.1f)), 0f, tex2.Size() / 2, (mainTex.Size() / tex2.Size()) * 1.4f * Fade, SpriteEffects.None, 0f);

            var texShine = ModContent.Request<Texture2D>("StarlightRiver/Assets/Keys/Shine").Value;

            Main.spriteBatch.Draw(texShine, pos, null, glowColor * 2 * (1 - GetProgress(0)), Main.GameUpdateCount / 250f, new Vector2(texShine.Width / 2, texShine.Height), 0.16f * GetProgress(0) * Fade, 0, 0);
            Main.spriteBatch.Draw(texShine, pos, null, glowColor * 2 * (1 - GetProgress(34)), Main.GameUpdateCount / 350f + 2.2f, new Vector2(texShine.Width / 2, texShine.Height), 0.18f * GetProgress(34) * Fade, 0, 0);
            Main.spriteBatch.Draw(texShine, pos, null, glowColor * 2 * (1 - GetProgress(70)), Main.GameUpdateCount / 320f + 5.4f, new Vector2(texShine.Width / 2, texShine.Height), 0.18f * GetProgress(70) * Fade, 0, 0);
            Main.spriteBatch.Draw(texShine, pos, null, glowColor * 2 * (1 - GetProgress(15)), Main.GameUpdateCount / 300f + 3.14f, new Vector2(texShine.Width / 2, texShine.Height), 0.16f * GetProgress(15) * Fade, 0, 0);
            Main.spriteBatch.Draw(texShine, pos, null, glowColor * 2 * (1 - GetProgress(98)), Main.GameUpdateCount / 360f + 4.0f, new Vector2(texShine.Width / 2, texShine.Height), 0.18f * GetProgress(98) * Fade, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(mainTex, pos, null, lightColor, 0f, mainTex.Size() / 2, Projectile.scale, SpriteEffects.None, 0f);
            return false;
        }

        private float GetProgress(float off)
        {
            return (Main.GameUpdateCount + off * 3) % 300 / 300f;
        }

        public override void Kill(int timeLeft)
        {
            Rectangle itemDrop = new Rectangle((int)Projectile.position.X + (int)(size.X / 2), (int)Projectile.position.Y + (int)(size.Y / 2), 1, 1);
            Item.NewItem(new EntitySource_Misc("Artifact"), itemDrop, itemType);
        }
    }
}