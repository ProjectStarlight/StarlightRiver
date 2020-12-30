using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleCorrupt
{
    class GrassJungleCorrupt : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.JungleCorruptTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSet(this, 0, 38, SoundID.Dig, new Color(98, 82, 148), ItemID.MudBlock);
            TileID.Sets.Grass[Type] = true;
            TileID.Sets.GrassSpecial[Type] = true;
            TileID.Sets.ChecksForMerge[Type] = true;
            SetModTree(new TreeJungleCorrupt());
        }

        public override void RandomUpdate(int i, int j)//grappling hook breaks the grass, its running killtile for some reason?
        {
            int x = Main.rand.Next(-4, 4);
            int y = Main.rand.Next(-4, 4);

            if (Main.tile[i + x, j + y].active() && Main.hardMode)//spread, using the clentaminator method
            {
                WorldGen.Convert(i + x, j + y, 1, 1);
            }

            if (!Main.tile[i, j + 1].active() && Main.tile[i, j].slope() == 0 && !Main.tile[i, j].halfBrick())//vines 
            {
                WorldGen.PlaceTile(i, j + 1, TileType<VineJungleCorrupt>(), true);
            }

            if (!Main.tile[i, j - 1].active() && Main.tile[i, j].slope() == 0 && !Main.tile[i, j].halfBrick())//grass
            {
                if (Main.rand.Next(2) == 0)
                {
                    WorldGen.PlaceTile(i, j - 1, TileType<TallgrassJungleCorrupt>(), true);
                    Main.tile[i, j - 1].frameY = (short)(Main.rand.Next(9) * 18);
                }
            }

            if (!Main.tile[i, j - 1].active() && !Main.tile[i, j - 2].active() && Main.tile[i, j].slope() == 0 && !Main.tile[i, j].halfBrick())//double grass
            {
                if (Main.rand.Next(4) == 0)
                {
                    WorldGen.PlaceTile(i, j - 2, TileType<TallgrassJungleCorrupt2>(), true);
                    int rand = Main.rand.Next(6);
                    Main.tile[i, j - 1].frameY = (short)(rand * 36);
                    Main.tile[i, j - 2].frameY = (short)(18 + rand * 36);
                }
            }
        }
        public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
        {
            effectOnly = true;
            WorldGen.PlaceTile(i, j, TileID.Mud, false, true);
        }
        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (Main.rand.Next(600) == 0 && !Main.tile[i, j + 1].active() && Main.tile[i, j].slope() == 0)
            {
                Dust.NewDustPerfect(new Vector2(i, j) * 16, mod.DustType("Corrupt2"), new Vector2(0, 0.6f));
            }
        }
    }
}