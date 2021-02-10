using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.NPCs.TownUpgrade;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Helpers;
using StarlightRiver.Content.Tiles;

namespace StarlightRiver.Content.GUI
{
    class TownQuestList : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        private readonly UIList quests = new UIList();
        private readonly UIList itemList = new UIList();
        private readonly UIScrollbar questScroll = new UIScrollbar();
        private readonly SubmitButton submitButton = new SubmitButton();

        public TownUpgrade activeQuest;

        public override void OnInitialize()
        {
            AddElement(quests, -193, 0, 0.5f, 0.3f, 186, 300, this);
            quests.ListPadding = 6;

            AddElement(questScroll, -260, 0, 0.5f, 0.3f, 18, 300, this);
            questScroll.SetView(0, 300);
            quests.SetScrollbar(questScroll);

            AddElement(itemList, 5, 75, 0.5f, 0.3f, 200, 200, this);
            itemList.ListPadding = 0;

            AddElement(submitButton, 0, 310, 0.5f, 0.3f, 100, 28, this);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 pos = new Vector2(Main.screenWidth / 2 + 10, Main.screenHeight * 0.3f);
            Texture2D panel = GetTexture("StarlightRiver/Assets/GUI/TownQuestPanel");

            spriteBatch.Draw(panel, pos, panel.Frame(), Color.White * 0.8f, 0, Vector2.Zero, 1, 0, 0);
            if (activeQuest != null) Utils.DrawBorderString(spriteBatch, Helper.WrapString(activeQuest._questTip, 320, Main.fontDeathText, 0.6f), pos + new Vector2(10, 10), Color.White, 0.6f);

            Recalculate();
            base.Draw(spriteBatch);
        }

        public override void Update(GameTime gameTime)
        {
            if (Main.LocalPlayer.talkNPC <= 0)
                Visible = false;
        }

        internal void AddElement(UIElement element, int x, int y, float xPercent, float yPercent, int width, int height, UIElement appendTo)
        {
            element.Left.Set(x, xPercent);
            element.Top.Set(y, yPercent);
            element.Width.Set(width, 0);
            element.Height.Set(height, 0);
            appendTo.Append(element);
        }

        public void PopulateItems()
        {
            itemList.Clear();

            int offY = 0;
            foreach (Loot loot in activeQuest.Requirements)
            {
                UIElement element = new RequirementPreview(loot.Type, loot.Count);
                element.Top.Set(16, 0);
                element.Left.Set(16, 0);
                element.Width.Set(32, 0);
                element.Height.Set(32, 0);
                itemList.Add(element);
                offY += 15;
            }
        }

        public void PopulateList()
        {
            quests.Clear();

            int offY = 0;
            foreach (KeyValuePair<string, bool> pair in StarlightWorld.TownUpgrades)
            {
                if (TownUpgrade.FromString(pair.Key) != null) AddQuestButton(new TownQuestItem(TownUpgrade.FromString(pair.Key)), offY);
                offY += 28 + 6;
            }
        }

        internal void AddQuestButton(UIElement element, float offY)
        {
            element.Left.Set(0, 0);
            element.Top.Set(offY, 0);
            element.Width.Set(186, 0);
            element.Height.Set(28, 0);
            quests.Add(element);
        }
    }

    class TownQuestItem : UIElement
    {
        readonly TownUpgrade quest;

        public TownQuestItem(TownUpgrade itemQuest) => quest = itemQuest;

        public override void Draw(SpriteBatch spriteBatch)
        {
            TownQuestList parent = Parent.Parent.Parent as TownQuestList;
            Vector2 pos = GetDimensions().ToRectangle().Center();

            Texture2D back = GetTexture("StarlightRiver/Assets/GUI/TownQuestBack");
            Texture2D check = GetTexture("StarlightRiver/Assets/GUI/QuestCheck");

            spriteBatch.Draw(back, pos, back.Frame(), Color.White * (parent.activeQuest == quest ? 1 : IsMouseHovering ? 0.7f : 0.5f), 0, back.Size() / 2, 1, 0, 0);
            Utils.DrawBorderString(spriteBatch, quest._questName, pos + new Vector2(-16, 0), Color.White, 0.7f, 0.5f, 0.4f);

            if (quest.Unlocked) spriteBatch.Draw(check, pos + new Vector2(158, 0), back.Frame(), Color.White, 0, back.Size() / 2, 1, 0, 0);
        }

        public override void Click(UIMouseEvent evt)
        {
            Main.PlaySound(SoundID.MenuTick);

            TownQuestList parent = Parent.Parent.Parent as TownQuestList;
            parent.activeQuest = quest;
            parent.PopulateItems();
        }
    }

    class SubmitButton : UIElement
    {
        TownUpgrade Quest => (Parent as TownQuestList).activeQuest;

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (Quest != null && !Quest.Unlocked)
            {
                Vector2 pos = GetDimensions().ToRectangle().Center();
                Texture2D back = GetTexture("StarlightRiver/Assets/GUI/NPCButton");

                spriteBatch.Draw(back, pos, back.Frame(), Color.White * (IsMouseHovering ? 1 : 0.7f), 0, back.Size() / 2, 1, 0, 0);
                Utils.DrawBorderString(spriteBatch, "Submit", pos, Color.White, 0.7f, 0.5f, 0.4f);
            }
        }

        public override void Click(UIMouseEvent evt)
        {
            Main.PlaySound(SoundID.MenuTick);

            if (Quest == null || Quest.Unlocked) return;
            foreach (Loot loot in Quest.Requirements) if (!Helper.HasItem(Main.LocalPlayer, loot.Type, loot.Count)) return;

            foreach (Loot loot in Quest.Requirements) Helper.TryTakeItem(Main.LocalPlayer, loot.Type, loot.Count);

            StarlightWorld.TownUpgrades[Quest._npcName] = !StarlightWorld.TownUpgrades[Quest._npcName];

            Main.PlaySound(SoundID.Item82);
        }
    }

    class RequirementPreview : UIElement
    {
        private readonly int type;
        private readonly int count;
        private readonly string name;
        private readonly Texture2D icon;

        public RequirementPreview(int typ, int cnt)
        {
            type = typ;
            count = cnt;

            Item item = new Item();
            item.SetDefaults(typ);

            if (type <= ItemID.Count) icon = Main.itemTexture[type];
            else icon = GetTexture(item.modItem.Texture);

            name = item.Name;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 pos = GetDimensions().ToRectangle().Center();
            Rectangle rect = new Rectangle((int)pos.X - 16, (int)pos.Y - 16, 32, 32);

            spriteBatch.Draw(icon, pos, icon.Frame(), Color.White, 0, icon.Size() / 2, 1, 0, 0);
            Utils.DrawBorderString(spriteBatch, "x" + count, pos + new Vector2(36, 0), Color.White, 0.7f, 0.5f, 0.4f);

            if (rect.Contains(Main.MouseScreen.ToPoint())) Main.hoverItemName = name;
        }
    }
}
