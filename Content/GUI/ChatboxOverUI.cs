using StarlightRiver.Content.NPCs.TownUpgrade;
using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	public class ChatboxOverUI : SmartUIState
	{
		public TownUpgrade activeUpgrade;

		private readonly TownButton button = new();

		public override bool Visible => Main.player[Main.myPlayer].talkNPC > 0 && Main.npcShop <= 0 && !Main.InGuideCraftMenu;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: NPC / Sign Dialog"));
		}

		public override void OnInitialize()
		{
			AddElement(button, Main.screenWidth / 2 - TextureAssets.ChatBack.Value.Width / 2 - 104, 100, 86, 28, this);
		}

		public void SetState(TownUpgrade state)
		{
			activeUpgrade = state;

			if (state != null)
				button.displayString = state.buttonName;

			OnInitialize();
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (activeUpgrade != null)
				base.Draw(spriteBatch);
		}
	}

	public class TownButton : SmartUIElement
	{
		public string displayString = "ERROR";

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (IsMouseHovering)
				Main.LocalPlayer.mouseInterface = true;

			bool locked = displayString == "Locked";

			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/GUI/NPCButton").Value;
			spriteBatch.Draw(tex, GetDimensions().ToRectangle(), tex.Frame(), Color.White * (locked ? 0.4f : 0.8f));

			float x = FontAssets.ItemStack.Value.MeasureString(displayString).X;

			float scale = x < 70 ? 1 : 70 / x;
			Utils.DrawBorderString(spriteBatch, displayString, GetDimensions().ToRectangle().Center() + new Vector2(0, 3), Color.White * (locked ? 0.4f : 1), scale, 0.5f, 0.5f);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			if (Parent is ChatboxOverUI)
				(Parent as ChatboxOverUI).activeUpgrade?.ClickButton();
		}
	}
}
