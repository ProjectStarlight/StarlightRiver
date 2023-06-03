using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class BossBarOverlay : SmartUIState
	{
		public static bool visible;

		public static NPC tracked;
		public static string text;
		public static Texture2D texture = Request<Texture2D>(AssetDirectory.GUI + "BossBarFrame", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
		public static Color glowColor = Color.Transparent;

		public static bool? forceInvulnerabilityVisuals = null;

		private readonly BarOverlay bar = new();

		public override bool Visible => visible;

		public float Priority => 1f;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			bar.Left.Set(-258, 0.5f);
			bar.Top.Set(-76, 1);
			bar.Width.Set(516, 0);
			bar.Height.Set(46, 0);
			Append(bar);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			Recalculate();

			if (tracked is null)
				visible = false;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			base.Draw(spriteBatch);
		}

		public static void SetTracked(NPC NPC, string text = "", Texture2D tex = default)
		{
			tracked = NPC;
			BossBarOverlay.text = text;
			visible = true;

			if (tex != default)
				texture = tex;
		}
	}

	public class BarOverlay : SmartUIElement
	{
		public override void Draw(SpriteBatch spriteBatch)
		{
			NPC NPC = BossBarOverlay.tracked;

			if (NPC is null)
				return;

			Vector2 pos = GetDimensions().ToRectangle().TopLeft() + new Vector2(0, 1);
			var off = new Vector2(30, 12);
			bool shouldDrawChains = BossBarOverlay.forceInvulnerabilityVisuals != false && (NPC.dontTakeDamage || NPC.immortal) || BossBarOverlay.forceInvulnerabilityVisuals == true;

			if (NPC is null || !NPC.active || !Main.LocalPlayer.active)
			{
				BossBarOverlay.visible = false;
				BossBarOverlay.forceInvulnerabilityVisuals = null;
				return;
			}

			Texture2D texGlow = Request<Texture2D>(AssetDirectory.GUI + "BossbarGlow").Value;

			int progress = (int)(BossBarOverlay.tracked?.life / (float)BossBarOverlay.tracked?.lifeMax * 456);

			if (shouldDrawChains)
			{
				Texture2D texFill = Request<Texture2D>(AssetDirectory.GUI + "BossbarFillImmune").Value;
				Texture2D texEdge = Request<Texture2D>(AssetDirectory.GUI + "BossbarEdgeImmune").Value;

				spriteBatch.Draw(texFill, new Rectangle((int)(pos.X + off.X), (int)(pos.Y + off.Y) + 2, progress, texFill.Height - 4), Color.White);
				spriteBatch.Draw(texEdge, pos + off + Vector2.UnitX * progress, Color.White);
			}

			spriteBatch.End();
			spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

			spriteBatch.Draw(texGlow, pos + off, BossBarOverlay.glowColor * 0.5f);
			spriteBatch.Draw(texGlow, new Rectangle((int)(pos.X + off.X), (int)(pos.Y + off.Y), progress, 22), new Rectangle(0, 0, progress, 22), BossBarOverlay.glowColor);

			spriteBatch.End();
			spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

			Utils.DrawBorderString(spriteBatch, NPC.FullName + BossBarOverlay.text + ": " + NPC.life + "/" + NPC.lifeMax, pos + new Vector2(516 / 2, -20), Color.White, 1, 0.5f, 0);

			//spriteBatch.Draw(BossBarOverlay.Texture, pos, Color.White);           

			if (shouldDrawChains)
				spriteBatch.Draw(Request<Texture2D>(AssetDirectory.GUI + "BossbarChains").Value, pos, Color.White);
		}
	}
}