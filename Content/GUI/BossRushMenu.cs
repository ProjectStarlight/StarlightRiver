using StarlightRiver.Content.PersistentData;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.BossRushSystem;
using StarlightRiver.Core.Systems.PersistentDataSystem;
using System;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameContent.UI.Elements;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.IO;
using Terraria.UI;

namespace StarlightRiver.Content.GUI
{
	internal class BossRushButton : SmartUIState
	{
		public UIText button;

		public bool Unlocked => BossRushDataStore.UnlockedBossRush;

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
				if (Unlocked)
				{
					BossRushGUIHack.inMenu = true;
					SoundEngine.PlaySound(SoundID.MenuOpen);

					foreach (UIElement element in Children)
					{
						if (element is BossRushUnlockInfo info)
							info.animationTimer = 0;
					}
				}
				else
				{
					SoundEngine.PlaySound(SoundID.Unlock);
				}
			};

			Append(new BossRushUnlockInfo(BossrushUnlockFlag.Auroracle, "Auroracle", "StarlightRiver/Assets/Bosses/SquidBoss/SquidBoss_Head_Boss", 40));
			Append(new BossRushUnlockInfo(BossrushUnlockFlag.Glassweaver, "Glassweaver", "StarlightRiver/Assets/Bosses/GlassMiniboss/Glassweaver_Head_Boss", 70));
			Append(new BossRushUnlockInfo(BossrushUnlockFlag.Ceiros, "Ceiros", "StarlightRiver/Assets/Bosses/VitricBoss/VitricBoss_Head_Boss", 100));

			Append(button);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = button.GetDimensions().ToRectangle();

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = Unlocked ? (button.IsMouseHovering ? 1 : 0.75f) : 0.75f;
			Color color = (Unlocked ? new Color(73, 94, 171) : new Color(80, 80, 80)) * opacity;

			button.TextColor = Unlocked ? (button.IsMouseHovering ? Color.Yellow : Color.White) : new Color(80, 80, 80);

			Utils.DrawSplicedPanel(spriteBatch, background, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);

			base.Draw(spriteBatch);

			if (!Unlocked)
			{
				Texture2D lockTex = Assets.GUI.BossRushLock.Value;
				spriteBatch.Draw(lockTex, dims.Center.ToVector2(), null, Color.White, 0, lockTex.Size() / 2f, 1, 0, 0);
			}

			Recalculate();
		}
	}

	internal class BossRushUnlockInfo : SmartUIElement
	{
		public BossrushUnlockFlag flag;

		public string name;
		public string texture;

		public int animationTimer;
		public int yOffsetTarget;
		public int yOffset;

		public bool Unlocked => BossRushDataStore.DownedBoss(flag);

		public BossRushUnlockInfo(BossrushUnlockFlag flag, string name, string texture, int yOffsetTarget)
		{
			this.flag = flag;
			this.name = name;
			this.texture = texture;
			this.yOffsetTarget = yOffsetTarget;

			Left.Set(360, 0.5f);
			Top.Set(240, 0);
			Width.Set(100, 0);
			Height.Set(16, 0);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			var parent = Parent as BossRushButton;

			if (parent is null)
				return;

			if (parent.button.IsMouseHovering && animationTimer < 30)
				animationTimer++;

			if (!parent.button.IsMouseHovering && animationTimer > 0)
				animationTimer--;

			yOffset = (int)(Ease(animationTimer / 30f) * yOffsetTarget);
		}

		public float Ease(float input)
		{
			float c1 = 1.70158f;
			float c3 = c1 + 1;

			return 1 + c3 * (float)Math.Pow(input - 1, 3) + c1 * (float)Math.Pow(input - 1, 2);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var dims = GetDimensions().ToRectangle();
			dims.Inflate(10, 4);

			dims.Offset(new Point(0, yOffset));

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			Texture2D icon = ModContent.Request<Texture2D>(texture).Value;

			float opacity = Ease(animationTimer / 30f);
			Color color = (Unlocked ? new Color(73, 94, 171) : new Color(80, 80, 80)) * 0.75f * opacity;

			Utils.DrawSplicedPanel(spriteBatch, background, dims.X, dims.Y, dims.Width, dims.Height, 10, 10, 10, 10, color);
			spriteBatch.Draw(icon, dims.TopLeft() + new Vector2(10, 10), null, (Unlocked ? Color.White : Color.Black) * opacity, 0, icon.Size() / 2f, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch, Unlocked ? name : "???", dims.TopLeft() + new Vector2(icon.Width + 10, 4), (Unlocked ? Color.White : Color.Gray) * opacity, 0.8f);
		}
	}

	internal class BossRushMenu : SmartUIState
	{
		public UIText button;

		public override bool Visible => BossRushGUIHack.inMenu;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return 0;
		}

		public override void OnInitialize()
		{
			var normal = new BossRushChoice("Boss Rush",
				" - Fight all Starlight River bosses in order! NEWBLOCK" +
				" - Normal difficulty NEWBLOCK" +
				" - Full heal between bosses", 0);
			normal.Left.Set(-150, 0.25f);
			normal.Top.Set(-300, 0.5f);
			Append(normal);

			var expert = new BossRushChoice("Boss Blitz",
				" - Fight all Starlight River bosses in order! NEWBLOCK" +
				" - Expert difficulty NEWBLOCK" +
				" - Heal 200 life between bosses NEWBLOCK" +
				" - Game moves at 1.25x speed NEWBLOCK" +
				" - 2x Score multiplier", 1);
			expert.Left.Set(-150, 0.5f);
			expert.Top.Set(-300, 0.5f);
			Append(expert);

			var master = new BossRushChoice("Starlight\nShowdown",
				" - Theoretically Possible! NEWBLOCK" +
				" - Master difficulty NEWBLOCK" +
				" - No healing between bosses NEWBLOCK" +
				" - Game moves at 1.5x speed NEWBLOCK" +
				" - Healing potions disabled NEWBLOCK" +
				" - Teleportation disabled NEWBLOCK" +
				" - 3x Score multiplier", 2);
			master.Left.Set(-150, 0.75f);
			master.Top.Set(-300, 0.5f);
			Append(master);

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

	internal class BossRushChoice : UIElement
	{
		public string name;

		public string rules;

		public int difficulty;

		public BossRushChoice(string name, string rules, int difficulty)
		{
			Width.Set(300, 0);
			Height.Set(600, 0);

			this.name = name;
			this.rules = rules;
			this.difficulty = difficulty;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			CalculatedStyle dims = GetDimensions();
			Vector2 pos = dims.Position();

			Texture2D background = Main.Assets.Request<Texture2D>("Images/UI/CharCreation/PanelGrayscale").Value;
			float opacity = IsMouseHovering ? 1 : 0.75f;

			Utils.DrawSplicedPanel(spriteBatch, background, (int)dims.X, (int)dims.Y, (int)dims.Width, (int)dims.Height, 10, 10, 10, 10, new Color(73, 94, 171) * opacity);

			Utils.DrawSplicedPanel(spriteBatch, background, (int)dims.X + 10, (int)dims.Y + 140, (int)dims.Width - 20, 360, 10, 10, 10, 10, Color.Black * 0.2f);
			Utils.DrawSplicedPanel(spriteBatch, background, (int)dims.X + 10, (int)dims.Y + 520, (int)dims.Width - 20, 60, 10, 10, 10, 10, Color.Black * 0.2f);

			Utils.DrawBorderStringBig(spriteBatch, name, pos + new Vector2(dims.Width / 2f, 20), Color.White, 1, 0.5f);

			ReLogic.Graphics.DynamicSpriteFont font = Terraria.GameContent.FontAssets.MouseText.Value;
			string rulesWrapped = Helpers.Helper.WrapString(rules, 240, font, 0.8f);

			Utils.DrawBorderString(spriteBatch, rulesWrapped, pos + new Vector2(dims.Width / 2f, 160), Color.White, 0.8f, 0.5f);

			int sourceScore =
				difficulty == 0 ? PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().normalScore :
				difficulty == 1 ? PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().expertScore :
				difficulty == 2 ? PersistentDataStoreSystem.GetDataStore<BossRushDataStore>().masterScore :
				0;

			string scoreString = sourceScore > 0 ? $"Score: {sourceScore}" : "No score!";
			Color scoreColor = sourceScore > 0 ? Color.Yellow : Color.Gray;
			Utils.DrawBorderStringBig(spriteBatch, scoreString, pos + new Vector2(dims.Width / 2f, 536), scoreColor, 0.5f, 0.5f);
		}

		public override void MouseOver(UIMouseEvent evt)
		{
			SoundEngine.PlaySound(SoundID.MenuTick);
		}

		public override void LeftClick(UIMouseEvent evt)
		{
			BossRushSystem.isBossRush = true;
			BossRushSystem.bossRushDifficulty = difficulty;

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
}