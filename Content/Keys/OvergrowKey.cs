﻿using StarlightRiver.Content.Biomes;
using System;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Keys
{
	internal class OvergrowKey : Key
	{
		public override bool ShowCondition => Main.LocalPlayer.InModBiome(GetInstance<OvergrowBiome>());

		public OvergrowKey() : base("Overgrowth Key", "StarlightRiver/Assets/Keys/OvergrowKey") { }

		public override void PreDraw(SpriteBatch spriteBatch)
		{
			spriteBatch.End();
			spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.Additive, SamplerState.PointClamp, default, default);

			Texture2D tex = Assets.Keys.Glow.Value;
			spriteBatch.Draw(tex, Position + Vector2.One * 16 - Main.screenPosition, tex.Frame(), new Color(255, 255, 200) * 0.3f, StarlightWorld.visualTimer, tex.Frame().Size() / 2, 1 + (float)Math.Cos(StarlightWorld.visualTimer) * 0.25f, 0, 0);
			spriteBatch.Draw(tex, Position + Vector2.One * 16 - Main.screenPosition, tex.Frame(), new Color(255, 255, 200) * 0.5f, StarlightWorld.visualTimer, tex.Frame().Size() / 2, 0.7f + (float)Math.Cos(StarlightWorld.visualTimer + 0.5f) * 0.15f, 0, 0);
			spriteBatch.Draw(tex, Position + Vector2.One * 16 - Main.screenPosition, tex.Frame(), new Color(255, 255, 200) * 0.7f, StarlightWorld.visualTimer, tex.Frame().Size() / 2, 0.5f + (float)Math.Cos(StarlightWorld.visualTimer + 1) * 0.1f, 0, 0);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);
		}

		public override void PreUpdate()
		{
			if (Main.rand.NextBool(4))
				Dust.NewDust(Position + new Vector2(0, (float)Math.Sin(StarlightWorld.visualTimer) * 5), 32, 32, DustType<Dusts.GoldWithMovement>(), 0, 0, 0, default, 0.5f);

			Lighting.AddLight(Position, new Vector3(1, 1, 0.8f) * 0.6f);
		}

		public override void OnPickup()
		{
			CombatText.NewText(Hitbox, Color.White, "Got: Overgrowth Key");
		}
	}
}