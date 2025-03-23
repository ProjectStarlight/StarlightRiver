using ReLogic.Content;
using StarlightRiver.Content.Backgrounds;
using System;

namespace StarlightRiver.Content.Menus
{
	internal class DefaultStarlightMenu : ModMenu
	{
		static int timer = 0;

		public float activationTimer;
		public Vector2 targetPos;
		public Vector2 targetPos2;

		public override string DisplayName => "Starlight";
		public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/StarBird");

		public override void Update(bool isOnTitleScreen)
		{
			StarlightRiverBackground.forceActiveTimer = 15;
		}

		public override bool PreDrawLogo(SpriteBatch spriteBatch, ref Vector2 logoDrawCenter, ref float logoRotation, ref float logoScale, ref Color drawColor)
		{
			logoScale = 1.0f;
			Main.dayTime = false;
			Main.time = 10000;

			timer++;

			if (Main.menuMode == 0 && activationTimer < 1)
				activationTimer += (1 - activationTimer) * 0.05f;

			if (Main.menuMode != 0 && activationTimer > 0)
				activationTimer -= activationTimer * 0.05f;

			targetPos += (targetPos - Main.MouseScreen) * -0.05f;
			targetPos2 += (targetPos2 - Main.MouseScreen) * -0.01f;

			RenderTarget2D tex = StarlightRiverBackground.starsTarget.RenderTarget;
			spriteBatch.Draw(tex, Main.ScreenSize.ToVector2() / 2f, null, Color.White, 0, tex.Size() / 2f, Main.UIScale, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, null, Main.UIScaleMatrix);

			Vector2 pos2 = Main.ScreenSize.ToVector2() / 2f;
			Vector2 mouseOff = activationTimer * (targetPos - Main.ScreenSize.ToVector2() / 2f);

			//Main.screenPosition = activationTimer * (targetPos2 - Main.ScreenSize.ToVector2() / 2f) * 0.25f;
			Main.screenPosition.X = timer * 2;

			Color back = new Color(40, 50, 65);
			Color front = new Color(80, 85, 90);

			Color color1 = Color.Lerp(back, front, 0.3f * activationTimer);
			Color color2 = Color.Lerp(back, front, 0.65f * activationTimer);
			Color color3 = Color.Lerp(back, front, 1f * activationTimer);

			Texture2D smallRing = Assets.NPCs.BossRush.ArmillaryRing1.Value;
			Texture2D mediumRing = Assets.NPCs.BossRush.ArmillaryRing2.Value;
			Texture2D largeRing = Assets.NPCs.BossRush.ArmillaryRing3.Value;

			Texture2D smallRingRunes = Assets.NPCs.BossRush.ArmillaryRingRunes1.Value;
			Texture2D mediumRingRunes = Assets.NPCs.BossRush.ArmillaryRingRunes2.Value;
			Texture2D largeRingRunes = Assets.NPCs.BossRush.ArmillaryRingRunes3.Value;

			float scale = 1 + activationTimer;

			spriteBatch.Draw(smallRing, pos2 + mouseOff * 0.1f, null, color1, -timer * 0.005f, smallRing.Size() * 0.5f, scale, SpriteEffects.None, 0);
			spriteBatch.Draw(smallRingRunes, pos2 + mouseOff * 0.1f, null, new Color(0.1f, 1f, 1f) * (0.5f + (float)Math.Sin(timer * 0.05f) * 0.4f), -timer * 0.005f, smallRingRunes.Size() * 0.5f, scale, SpriteEffects.None, 0);

			spriteBatch.Draw(mediumRing, pos2 + mouseOff * 0.2f, null, color2, timer * 0.005f, mediumRing.Size() * 0.5f, scale, SpriteEffects.None, 0);
			spriteBatch.Draw(mediumRingRunes, pos2 + mouseOff * 0.2f, null, new Color(0.1f, 0.8f, 1f) * (0.5f + (float)Math.Sin((timer + 15) * 0.05f) * 0.4f), timer * 0.005f, mediumRingRunes.Size() * 0.5f, scale, SpriteEffects.None, 0);

			spriteBatch.Draw(largeRing, pos2 + mouseOff * 0.3f, null, color3, -timer * 0.005f, largeRing.Size() * 0.5f, scale, SpriteEffects.None, 0);
			spriteBatch.Draw(largeRingRunes, pos2 + mouseOff * 0.3f, null, new Color(0.1f, 0.6f, 1f) * (0.5f + (float)Math.Sin((timer + 30) * 0.05f) * 0.4f), -timer * 0.005f, largeRingRunes.Size() * 0.5f, scale, SpriteEffects.None, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearClamp, default, default, default, Main.UIScaleMatrix);

			return false;
		}
	}
}