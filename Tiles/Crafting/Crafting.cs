using Microsoft.Xna.Framework;
using StarlightRiver.GUI;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Crafting
{
    internal class Oven : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 3, 2, DustID.Stone, SoundID.Dig, false, new Color(113, 113, 113), false, false, "Oven");

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => (r, g, b) = (0.4f, 0.2f, 0.05f);

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<Items.Crafting.OvenItem>());
    }

    internal class HerbStation : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 3, 2, DustID.t_LivingWood, SoundID.Dig, false, new Color(151, 107, 75), false, false, "Herbologist's Bench");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<Items.Crafting.HerbStationItem>());
    }

    internal class CookStation : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 6, 4, DustID.t_LivingWood, SoundID.Dig, false, new Color(151, 107, 75), false, false, "Cooking Station");

        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Item.NewItem(new Vector2(i, j) * 16, ItemType<Items.Crafting.CookStationItem>());

        public override bool NewRightClick(int i, int j)
        {
            if (!Cooking.Visible) { Cooking.Visible = true; Main.PlaySound(SoundID.MenuOpen); }
            else { Cooking.Visible = false; Main.PlaySound(SoundID.MenuClose); }
            return true;
        }
    }
}