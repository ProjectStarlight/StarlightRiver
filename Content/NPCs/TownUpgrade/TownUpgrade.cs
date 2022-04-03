using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Tiles;
using StarlightRiver.Core;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.TownUpgrade
{
	public abstract class TownUpgrade
    {
        public readonly string _buttonName;
        public readonly string _NPCName;
        public readonly string _questName;
        public readonly string _questTip;
        public readonly string _title;
        public readonly Texture2D icon;

        protected TownUpgrade(string NPCName, string questName, string questTip, string buttonName, string title)
        {
            _buttonName = buttonName;
            _NPCName = NPCName;
            _questName = questName;
            _questTip = questTip;
            _title = title;

            icon = TextureExists("StarlightRiver/Assets/NPCs/TownUpgrade/" + NPCName + "Icon") ?
                Request<Texture2D>("StarlightRiver/Assets/NPCs/TownUpgrade/" + NPCName + "Icon").Value :
                Terraria.Main.sunTexture;
        }

        public bool Unlocked => StarlightWorld.TownUpgrades.TryGetValue(_NPCName, out bool unlocked) && unlocked;

        public virtual List<Loot> Requirements => new List<Loot>() { new Loot(ItemID.DirtBlock, 1) };

        public virtual void ClickButton() { }

        public static TownUpgrade FromString(string input)
        {
            TownUpgrade town;

            switch (input)
            {
                case "Guide": town = new GuideUpgrade(); break;
                case "Merchant": town = new MerchantUpgrade(); break;
                default: town = null; break;
            }

            return town;
        }
    }
}
