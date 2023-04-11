using StarlightRiver.Content.NPCs.TownUpgrade;
using StarlightRiver.Content.Tiles;
using StarlightRiver.Core.Loaders.UILoading;
using StarlightRiver.Core.Systems.NPCUpgradeSystem;
using StarlightRiver.Helpers;
using System.Collections.Generic;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
	class TownQuestList : SmartUIState
	{
		private readonly UIList quests = new();
		private readonly UIList ItemList = new();
		private readonly UIScrollbar questScroll = new();
		private readonly SubmitButton submitButton = new();

		public TownUpgrade activeQuest;

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
		}

		public override void OnInitialize()
		{
			quests.ListPadding = 6;
			AddElement(quests, -193, 0.5f, 0, 0.3f, 186, 0f, 300, 0f);

			questScroll.SetView(0, 300);
			quests.SetScrollbar(questScroll);
			AddElement(questScroll, -260, 0.5f, 0, 0.3f, 18, 0f, 300, 0f);

			ItemList.ListPadding = 0;
			AddElement(ItemList, 5, 0.5f, 75, 0.3f, 200, 0f, 200, 0f);

			AddElement(submitButton, 0, 0.5f, 310, 0.3f, 100, 0f, 28, 0f);
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var pos = new Vector2(Main.screenWidth / 2 + 10, Main.screenHeight * 0.3f);
			Texture2D panel = Request<Texture2D>("StarlightRiver/Assets/GUI/TownQuestPanel").Value;

			spriteBatch.Draw(panel, pos, panel.Frame(), Color.White * 0.8f, 0, Vector2.Zero, 1, 0, 0);
			if (activeQuest != null)
				Utils.DrawBorderString(spriteBatch, Helper.WrapString(activeQuest.questTip, 320, FontAssets.DeathText.Value, 0.6f), pos + new Vector2(10, 10), Color.White, 0.6f);

			Recalculate();
			base.Draw(spriteBatch);
		}

		public override void SafeUpdate(GameTime gameTime)
		{
			if (Main.LocalPlayer.talkNPC <= 0)
				Visible = false;
		}

		public void PopulateItems()
		{
			ItemList.Clear();

			int offY = 0;
			foreach (Loot loot in activeQuest.Requirements)
			{
				SmartUIElement element = new RequirementPreview(loot.type, loot.count);
				element.Top.Set(16, 0);
				element.Left.Set(16, 0);
				element.Width.Set(32, 0);
				element.Height.Set(32, 0);
				ItemList.Add(element);
				offY += 15;
			}
		}

		public void PopulateList()
		{
			quests.Clear();

			int offY = 0;
			foreach (KeyValuePair<string, bool> pair in NPCUpgradeSystem.townUpgrades)
			{
				if (TownUpgrade.FromString(pair.Key) != null)
					AddQuestButton(new TownQuestItem(TownUpgrade.FromString(pair.Key)), offY);
				offY += 28 + 6;
			}
		}

		internal void AddQuestButton(SmartUIElement element, float offY)
		{
			element.Left.Set(0, 0);
			element.Top.Set(offY, 0);
			element.Width.Set(186, 0);
			element.Height.Set(28, 0);
			quests.Add(element);
		}
	}

	class TownQuestItem : SmartUIElement
	{
		readonly TownUpgrade quest;

		public TownQuestItem(TownUpgrade ItemQuest)
		{
			quest = ItemQuest;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			var parent = Parent.Parent.Parent as TownQuestList;
			Vector2 pos = GetDimensions().ToRectangle().Center();

			Texture2D back = Request<Texture2D>("StarlightRiver/Assets/GUI/TownQuestBack").Value;
			Texture2D check = Request<Texture2D>("StarlightRiver/Assets/GUI/QuestCheck").Value;

			spriteBatch.Draw(back, pos, back.Frame(), Color.White * (parent.activeQuest == quest ? 1 : IsMouseHovering ? 0.7f : 0.5f), 0, back.Size() / 2, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch, quest.questName, pos + new Vector2(-16, 0), Color.White, 0.7f, 0.5f, 0.4f);

			if (quest.Unlocked)
				spriteBatch.Draw(check, pos + new Vector2(158, 0), back.Frame(), Color.White, 0, back.Size() / 2, 1, 0, 0);
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);

			var parent = Parent.Parent.Parent as TownQuestList;
			parent.activeQuest = quest;
			parent.PopulateItems();
		}
	}

	class SubmitButton : SmartUIElement
	{
		TownUpgrade Quest => (Parent as TownQuestList).activeQuest;

		public override void Draw(SpriteBatch spriteBatch)
		{
			if (Quest != null && !Quest.Unlocked)
			{
				Vector2 pos = GetDimensions().ToRectangle().Center();
				Texture2D back = Request<Texture2D>("StarlightRiver/Assets/GUI/NPCButton").Value;

				spriteBatch.Draw(back, pos, back.Frame(), Color.White * (IsMouseHovering ? 1 : 0.7f), 0, back.Size() / 2, 1, 0, 0);
				Utils.DrawBorderString(spriteBatch, "Submit", pos, Color.White, 0.7f, 0.5f, 0.4f);
			}
		}

		public override void SafeClick(UIMouseEvent evt)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);

			if (Quest == null || Quest.Unlocked)
				return;

			foreach (Loot loot in Quest.Requirements)
			{
				if (!Helper.HasItem(Main.LocalPlayer, loot.type, loot.count))
					return;
			}

			foreach (Loot loot in Quest.Requirements)
			{
				Helper.TryTakeItem(Main.LocalPlayer, loot.type, loot.count);
			}

			NPCUpgradeSystem.townUpgrades[Quest.NPCName] = !NPCUpgradeSystem.townUpgrades[Quest.NPCName];

			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item82);
		}
	}

	class RequirementPreview : SmartUIElement
	{
		private readonly int type;
		private readonly int count;
		private readonly string name;
		private readonly Texture2D icon;

		public RequirementPreview(int typ, int cnt)
		{
			type = typ;
			count = cnt;

			var Item = new Item();
			Item.SetDefaults(typ);

			if (type <= ItemID.Count)
				icon = TextureAssets.Item[type].Value;
			else
				icon = Request<Texture2D>(Item.ModItem.Texture).Value;

			name = Item.Name;
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Vector2 pos = GetDimensions().ToRectangle().Center();
			var rect = new Rectangle((int)pos.X - 16, (int)pos.Y - 16, 32, 32);

			spriteBatch.Draw(icon, pos, icon.Frame(), Color.White, 0, icon.Size() / 2, 1, 0, 0);
			Utils.DrawBorderString(spriteBatch, "x" + count, pos + new Vector2(36, 0), Color.White, 0.7f, 0.5f, 0.4f);

			if (rect.Contains(Main.MouseScreen.ToPoint()))
				Main.hoverItemName = name;
		}
	}
}