using StarlightRiver.Content.Backgrounds;
using StarlightRiver.Content.PersistentData;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BossRushSystem;
using StarlightRiver.Core.Systems.PersistentDataSystem;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.Graphics.Effects;
using Terraria.ID;
using Terraria.IO;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	internal class BossRushButton : SmartUIState
	{
		public UIText button;

		public override bool Visible => Main.gameMenu && Main.menuMode == MenuID.FancyUI && Main.MenuUI.CurrentState is UIWorldSelect;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return 0;
		}

		public override void OnInitialize()
		{
			button = new UIText("Armillary Trial");
			button.Left.Set(350, 0.5f);
			button.Top.Set(230, 0);
			button.Width.Set(140, 0);
			button.Height.Set(36, 0);
			button.SetPadding(10);
			button.OnMouseOver += (a, b) => SoundEngine.PlaySound(SoundID.MenuTick);

			button.OnLeftClick += (a, b) =>
			{
				BossRushGUIHack.inMenu = true;
				SoundEngine.PlaySound(SoundID.MenuOpen);
			};

			Append(button);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = button.GetDimensions().ToRectangle();

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = button.IsMouseHovering ? 1 : 0.75f;
			Color color = new Color(73, 94, 171) * opacity;

			button.TextColor = button.IsMouseHovering ? Color.Yellow : Color.White;

			Utils.DrawSplicedPanel(spriteBatch, background, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);

			base.Draw(spriteBatch);

			Recalculate();
		}
	}

	internal class BossRushUnlockInfo : SmartUIElement
	{
		public BossrushUnlockFlag flag;

		public string name;
		public string hint;
		public Asset<Texture2D> texture;
		public int seed;

		public float radiusTarget;
		public float rotTarget;

		public bool clicked;

		public bool Unlocked => BossRushDataStore.DownedBoss(flag);

		public BossRushUnlockInfo(BossrushUnlockFlag flag, string name, string hint, Asset<Texture2D> texture, float radiusTarget, float rotTarget)
		{
			this.flag = flag;
			this.name = name;
			this.hint = LocalizationHelper.WrapString(hint, 300, Terraria.GameContent.FontAssets.ItemStack.Value, 0.8f);
			this.texture = texture;
			this.radiusTarget = radiusTarget;
			this.rotTarget = rotTarget;

			this.seed = Main.rand.Next(60);

			Width.Set(48, 0);
			Height.Set(48, 0);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			Left.Set(MathF.Cos(rotTarget) * radiusTarget - 24, 0.5f);
			Top.Set(MathF.Sin(rotTarget) * radiusTarget - 24, 0.5f);

			if (IsMouseHovering && Main.mouseLeft && !clicked)
			{
				clicked = true;

				if (!Unlocked)
					BossRushDataStore.DefeatBoss(flag);
				else
					BossRushDataStore.ResetBoss(flag);
			}

			if (clicked && Main.mouseLeftRelease)
				clicked = false;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();

			Texture2D background = Assets.NPCs.BossRush.ArmillarySlot.Value;
			Texture2D icon = texture.Value;

			Color color = (Unlocked ? new Color(73, 94, 171) : new Color(80, 80, 80)) * 0.75f;

			float time = BossRushMenu.timer + seed * 4;
			Vector2 pos = dims.Center() + new Vector2(MathF.Cos(time / 60f * 6.28f * 0.24f), MathF.Sin(time / 60f * 6.28f * 0.3f)) * 6 * BossRushMenu.Fade;

			Color litColor = Color.Lerp(Main.tileColor, Color.White, 0.5f);

			spriteBatch.Draw(background, pos, null, litColor, 0, background.Size() / 2f, 1, 0, 0);
			spriteBatch.Draw(icon, pos, null, Unlocked ? Color.White : Color.Black, 0, icon.Size() / 2f, 1, 0, 0);
		}

		public void DrawText(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			float opacity = 1;// Ease(animationTimer / 30f);

			float time = BossRushMenu.timer + seed * 4;
			Vector2 pos = dims.Center() + new Vector2(MathF.Cos(time / 60f * 6.28f * 0.24f), MathF.Sin(time / 60f * 6.28f * 0.3f)) * 6 * BossRushMenu.Fade;

			Utils.DrawBorderString(spriteBatch, Unlocked ? name + " ✔" : "???", pos + new Vector2(0, 32), (Unlocked ? new Color(200, 255, 200) : Color.Gray) * opacity, 0.8f, 0.5f, 0);

			if (IsMouseHovering)
				Utils.DrawBorderString(spriteBatch, Unlocked ? "Boss defeated" : hint, Main.MouseScreen + new Vector2(16, 16), (Unlocked ? new Color(200, 255, 200) : Color.White) * opacity, 0.8f);
		}
	}

	internal class BossRushRelicToggle : SmartUIElement
	{
		public Func<bool> active;
		public string name;
		public string hint;
		public Asset<Texture2D> texture;
		public int seed;

		public float radiusTarget;
		public float rotTarget;

		public bool clicked;

		public BossRushRelicToggle(Func<bool> active, string name, string hint, Asset<Texture2D> texture, float radiusTarget, float rotTarget)
		{
			this.active = active;
			this.name = name;
			this.hint = LocalizationHelper.WrapString(hint, 300, Terraria.GameContent.FontAssets.ItemStack.Value, 0.8f);
			this.texture = texture;
			this.radiusTarget = radiusTarget;
			this.rotTarget = rotTarget;

			this.seed = Main.rand.Next(60);

			Width.Set(48, 0);
			Height.Set(48, 0);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			Left.Set(MathF.Cos(rotTarget) * radiusTarget - 24, 0.5f);
			Top.Set(MathF.Sin(rotTarget) * radiusTarget - 24, 0.5f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (!BossRushDataStore.UnlockedBossRush)
				return;

			var dims = GetDimensions().ToRectangle();

			Texture2D background = Assets.NPCs.BossRush.ArmillarySlot.Value;
			Texture2D icon = texture.Value;

			Color color = new Color(73, 94, 171) * 0.75f;

			float time = BossRushMenu.timer + seed * 4;
			Vector2 pos = dims.Center();

			Color litColor = Color.Lerp(Main.tileColor, Color.White, 0.5f);

			spriteBatch.Draw(icon, pos, null, litColor, 0, icon.Size() / 2f, 1, 0, 0);

			if (active())
			{
				Texture2D glow = Assets.Keys.GlowAlpha.Value;
				spriteBatch.Draw(glow, pos, null, new Color(100, 200, 255, 0), 0, glow.Size() / 2f, 0.8f, 0, 0);
			}
		}

		public void DrawText(SpriteBatch spriteBatch)
		{
			if (!BossRushDataStore.UnlockedBossRush)
				return;

			var dims = GetDimensions().ToRectangle();

			float time = BossRushMenu.timer + seed * 4;
			Vector2 pos = dims.Center();

			Utils.DrawBorderString(spriteBatch, name, pos + new Vector2(0, 32), active() ? new Color(100, 230, 255) : Color.Gray, 0.8f, 0.5f, 0);

			if (IsMouseHovering)
				Utils.DrawBorderString(spriteBatch, hint, Main.MouseScreen + new Vector2(16, 16), Color.White, 0.8f);
		}
	}

	internal class BossRushMenu : SmartUIState
	{
		public UIText button;

		public static int difficulty;

		public static int timer;
		public static float Fade => timer < 180 ? timer / 180f : 1;

		public bool Unlocked => BossRushDataStore.UnlockedBossRush;

		public static Color DifficultyColor => difficulty == 2 ? Color.Red : difficulty == 1 ? Color.Orange : Color.SkyBlue;
		public static string DifficultyString => difficulty == 2 ? "Master!" : difficulty == 1 ? "Expert" : "Classic";

		public override bool Visible => BossRushGUIHack.inMenu;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return 0;
		}

		public override void OnInitialize()
		{
			var difficulty = new BossRushDifficultySwitcher();
			difficulty.Width.Set(140, 0);
			difficulty.Height.Set(36, 0);
			difficulty.Left.Set(-70, 0.5f);
			difficulty.Top.Set(180, 0.5f);
			Append(difficulty);

			var start = new BossRushStart();
			start.Width.Set(140, 0);
			start.Height.Set(36, 0);
			start.Left.Set(-70, 0.5f);
			start.Top.Set(220, 0.5f);
			Append(start);

			button = new UIText("Back");
			button.Left.Set(-70, 0.5f);
			button.Top.Set(320, 0.5f);
			button.Width.Set(140, 0);
			button.Height.Set(36, 0);
			button.SetPadding(10);
			button.OnMouseOver += (a, b) => SoundEngine.PlaySound(SoundID.MenuTick);

			button.OnLeftClick += (a, b) =>
			{
				BossRushGUIHack.inMenu = false;

				Main.OpenWorldSelectUI();
				Main.menuMode = MenuID.FancyUI;

				SoundEngine.PlaySound(SoundID.MenuClose);
			};

			Append(button);

			Append(new BossRushUnlockInfo(BossrushUnlockFlag.Auroracle, "Auroracle", "Found by following the wisps in the ice biome", Assets.Bosses.SquidBoss.SquidBoss_Head_Boss, 220, -1.57f - 1f));
			Append(new BossRushUnlockInfo(BossrushUnlockFlag.Glassweaver, "Glassweaver", "Guards his forge deep below the desert", Assets.Bosses.GlassMiniboss.Glassweaver_Head_Boss, 220, -1.57f - 0.33f));
			Append(new BossRushUnlockInfo(BossrushUnlockFlag.Ceiros, "Ceiros", "Guards a temple deep below the desert", Assets.Bosses.VitricBoss.VitricBoss_Head_Boss, 220, -1.57f + 0.33f));
			Append(new BossRushUnlockInfo(BossrushUnlockFlag.Thinker, "The Thinker", "Born from the Brain of Cthulhu into the crimson caverns", Assets.Bosses.TheThinkerBoss.TheThinker_Head_Boss, 220, -1.57f + 1f));

			var speedRelic = new BossRushRelicToggle(() => BossRushSpeedupAddon.active, "Speed Relic", "Makes the game move 33% faster", Assets.NPCs.BossRush.SpeedRelic, 240, 1.57f - 0.66f);
			speedRelic.OnLeftClick += (a, b) => BossRushSpeedupAddon.active = !BossRushSpeedupAddon.active;
			Append(speedRelic);

			var frailRelic = new BossRushRelicToggle(() => BossRushFrailtyAddon.active, "Frailty Relic", "Causes you to die from a single hit", Assets.NPCs.BossRush.FrailRelic, 240, 1.57f + 0.66f);
			frailRelic.OnLeftClick += (a, b) => BossRushFrailtyAddon.active = !BossRushFrailtyAddon.active;
			Append(frailRelic);
		}

		public static void DrawMap(SpriteBatch spriteBatch)
		{
			if (BossRushDataStore.UnlockedBossRush)
			{
				float scale = 1f + 0.05f * MathF.Sin(timer / 120f * 6.28f);

				spriteBatch.Draw(Assets.Keys.GlowAlpha.Value, Main.ScreenSize.ToVector2() / 2f * (1f / Main.UIScale), null, new Color(1f, 1f, 1f, 0) * Fade, 0, Assets.Keys.GlowAlpha.Size() / 2f, 12 * scale, 0, 0);
				spriteBatch.Draw(Assets.Keys.GlowHarshAlpha.Value, Main.ScreenSize.ToVector2() / 2f * (1f / Main.UIScale), null, new Color(1f, 1f, 1f, 0) * Fade, 0, Assets.Keys.GlowHarshAlpha.Size() / 2f, 20 * scale, 0, 0);

				foreach (UIElement element in UILoader.GetUIState<BossRushMenu>().Children)
				{
					if (element is BossRushUnlockInfo)
						spriteBatch.Draw(Assets.Keys.GlowAlpha.Value, element.GetDimensions().Center(), null, new Color(1f, 1f, 1f, 0) * 0.5f * Fade, 0, Assets.Keys.GlowAlpha.Size() / 2f, 2 * scale, 0, 0);
				}
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			//RemoveAllChildren();
			//OnInitialize();

			var dims = button.GetDimensions().ToRectangle();

			Effect mapEffect = Filters.Scene["StarMap"].GetShader().Shader;
			mapEffect.Parameters["map"].SetValue(StarlightRiverBackground.starsMap.RenderTarget);
			mapEffect.Parameters["background"].SetValue(StarlightRiverBackground.starsTarget.RenderTarget);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearWrap, default, RasterizerState.CullNone, mapEffect, Main.UIScaleMatrix);

			spriteBatch.Draw(StarlightRiverBackground.starsMap.RenderTarget, Vector2.Zero, null, new Color(1f, 1f, 1f, 1));

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.PointWrap, default, default, default, Main.UIScaleMatrix);

			Texture2D ring3 = Assets.NPCs.BossRush.ArmillaryRing3.Value;
			Texture2D ring2 = Assets.NPCs.BossRush.ArmillaryRing2.Value;
			Texture2D ring1 = Assets.NPCs.BossRush.ArmillaryRing1.Value;
			Texture2D orb = Assets.NPCs.BossRush.BossRushOrb.Value;

			Texture2D runes3 = Assets.NPCs.BossRush.ArmillaryRingRunes3.Value;
			Texture2D runes2 = Assets.NPCs.BossRush.ArmillaryRingRunes2.Value;
			Texture2D runes1 = Assets.NPCs.BossRush.ArmillaryRingRunes1.Value;

			float rotProgress = Unlocked ? timer * (0.02f + difficulty * 0.01f) * Fade : 0;

			Color litColor = Color.Lerp(Main.tileColor, Color.White, 0.5f);

			spriteBatch.Draw(ring3, GetDimensions().Center(), null, litColor, rotProgress * 0.5f, ring3.Size() / 2f, 1, 0, 0);
			spriteBatch.Draw(ring2, GetDimensions().Center(), null, litColor, rotProgress * -0.75f, ring2.Size() / 2f, 1, 0, 0);
			spriteBatch.Draw(ring1, GetDimensions().Center(), null, litColor, rotProgress, ring1.Size() / 2f, 1, 0, 0);
			spriteBatch.Draw(orb, GetDimensions().Center(), null, litColor, 0, orb.Size() / 2f, 1, 0, 0);

			if (Unlocked)
			{
				spriteBatch.Draw(runes3, GetDimensions().Center(), null, DifficultyColor * Fade, rotProgress * 0.5f, runes3.Size() / 2f, 1, 0, 0);
				spriteBatch.Draw(runes2, GetDimensions().Center(), null, DifficultyColor * Fade, rotProgress * -0.75f, runes2.Size() / 2f, 1, 0, 0);
				spriteBatch.Draw(runes1, GetDimensions().Center(), null, DifficultyColor * Fade, rotProgress, runes1.Size() / 2f, 1, 0, 0);
			}

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = button.IsMouseHovering ? 1 : 0.75f;
			Color color = new Color(73, 94, 171) * opacity;

			button.TextColor = button.IsMouseHovering ? Color.Yellow : Color.White;

			base.Draw(spriteBatch);

			spriteBatch.End();
			spriteBatch.Begin(default, default, SamplerState.LinearWrap, default, default, default, Main.UIScaleMatrix);

			Utils.DrawSplicedPanel(spriteBatch, background, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);

			foreach (UIElement element in Children)
			{
				if (element is BossRushUnlockInfo info)
					info.DrawText(spriteBatch);

				if (element is BossRushDifficultySwitcher diff)
					diff.DrawText(spriteBatch);

				if (element is BossRushStart start)
					start.DrawText(spriteBatch);

				if (element is BossRushRelicToggle toggle)
					toggle.DrawText(spriteBatch);
			}

			Recalculate();
		}
	}

	internal class BossRushStart : SmartUIElement
	{
		public bool Unlocked => BossRushDataStore.UnlockedBossRush;

		public override void SafeClick(UIMouseEvent evt)
		{
			if (Unlocked)
			{
				BossRushSystem.isBossRush = true;
				BossRushSystem.bossRushDifficulty = BossRushMenu.difficulty;

				BossRushSystem.Reset();

				Main.mapEnabled = false;

				var temp = new WorldFileData(ModLoader.ModPath + "/BossRushWorld.wld", false);
				temp.SetAsActive();

				BossRushGUIHack.inMenu = false;

				Main.ActiveWorldFileData = temp;
				WorldGen.playWorld();

				Main.MenuUI.SetState(null);
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = Unlocked ? (IsMouseHovering ? 1 : 0.75f) : 0.75f;
			Color color = (Unlocked ? new Color(73, 94, 171) : new Color(80, 80, 80)) * opacity;

			Utils.DrawSplicedPanel(spriteBatch, background, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);

			base.Draw(spriteBatch);

			if (!Unlocked)
			{
				Texture2D lockTex = Assets.GUI.BossRushLock.Value;
				spriteBatch.Draw(lockTex, dims.Center.ToVector2(), null, Color.White, 0, lockTex.Size() / 2f, 1, 0, 0);
			}

			Recalculate();
		}

		public void DrawText(SpriteBatch spriteBatch)
		{
			if (Unlocked)
			{
				var dims = GetDimensions().ToRectangle();

				Color textColor = Unlocked ? (IsMouseHovering ? Color.Yellow : Color.White) : new Color(80, 80, 80);
				Utils.DrawBorderString(spriteBatch, "Start", dims.Center(), textColor, 1, 0.5f, 0.4f);
			}
		}
	}

	internal class BossRushDifficultySwitcher : SmartUIElement
	{
		public bool Unlocked => BossRushDataStore.UnlockedBossRush;

		public override void SafeClick(UIMouseEvent evt)
		{
			if (Unlocked)
			{
				BossRushMenu.difficulty++;
				BossRushMenu.difficulty %= 3;
			}
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = Unlocked ? (IsMouseHovering ? 1 : 0.75f) : 0.75f;
			Color color = (Unlocked ? new Color(73, 94, 171) : new Color(80, 80, 80)) * opacity;

			Utils.DrawSplicedPanel(spriteBatch, background, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);

			base.Draw(spriteBatch);

			Recalculate();
		}

		public void DrawText(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();

			Color textColor = Unlocked ? BossRushMenu.DifficultyColor : new Color(80, 80, 80);
			Utils.DrawBorderString(spriteBatch, Unlocked ? BossRushMenu.DifficultyString : "Locked", dims.Center(), textColor, 1, 0.5f, 0.4f);
		}
	}
}