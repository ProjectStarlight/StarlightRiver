using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Misc
{
    public class DullBlade : QuickMaterial
    {
        public override string Texture => AssetDirectory.MiscItem + Name;

        public DullBlade() : base("Dull Blade", "Doesn't seem very sharp... yet", 1, Item.sellPrice(gold: 1), ItemRarityID.Orange) { } //rarity might need changing, not sure where this falls into progression
    }
}
