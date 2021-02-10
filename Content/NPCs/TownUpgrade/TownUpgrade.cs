using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles;

namespace StarlightRiver.NPCs.TownUpgrade
{
    public abstract class TownUpgrade
    {
        public readonly string _buttonName;
        public readonly string _npcName;
        public readonly string _questName;
        public readonly string _questTip;
        public readonly string _title;
        public readonly Texture2D icon;

        protected TownUpgrade(string npcName, string questName, string questTip, string buttonName, string title)
        {
            _buttonName = buttonName;
            _npcName = npcName;
            _questName = questName;
            _questTip = questTip;
            _title = title;

            icon = TextureExists("StarlightRiver/Assets/NPCs/TownUpgrade/" + npcName + "Icon") ?
                GetTexture("StarlightRiver/Assets/NPCs/TownUpgrade/" + npcName + "Icon") :
                Terraria.Main.sunTexture;
        }

        public bool Unlocked => StarlightWorld.TownUpgrades.TryGetValue(_npcName, out bool unlocked) && unlocked;

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
