using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.JungleCorrupt
{
    internal class SporeJungleCorrupt : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.JungleCorruptTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileLavaDeath[Type] = false;
            Main.tileFrameImportant[Type] = true;
            Main.tileCut[Type] = true;

            dustType = DustType<Dusts.Corrupt>();
            soundType = SoundID.NPCDeath13.SoundId;
            animationFrameHeight = 16;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Corrupt Spore");
            AddMapEntry(new Color(117, 86, 106), name);
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int offset = Main.tileFrame[Type] + i;
            if (i % 2 == 0) { offset += 2 * 16; }
            if (j % 2 == 0) { offset += 1 * 16; }
            if (i % 3 == 0) { offset += 1 * 16; }
            if (j % 3 == 0) { offset += 3 * 16; }
            frameYOffset = (offset % 11) * 16;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (++frameCounter >= 8)
            {
                frameCounter = 0;
                if (++frame >= 10)
                {
                    frame = 0;
                }
            }
        }
    }

    public class ThornJungleCorrupt : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.JungleCorruptTile + name;
            return base.Autoload(ref name, ref texture);
        }

        public override void SetDefaults()
        {
            Main.tileCut[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x1);
            soundType = SoundID.Grass;
            dustType = 14;

            ModTranslation name = CreateMapEntryName();
            name.SetDefault("Thorns");
            AddMapEntry(new Color(46, 41, 58), name);
        }

        public override void RandomUpdate(int i, int j)
        {
            int x = Main.rand.Next(-1, 1);
            int y = Main.rand.Next(-1, 1);
            if (!Main.tile[i + x, j + y].active())
            {
                if (Main.rand.Next(1) == 0)
                {
                    WorldGen.PlaceTile(i + x, j + y, TileType<ThornJungleCorrupt>(), true);
                }
            }
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            closer = true;
            if (closer)
            {
                Rectangle rect = new Rectangle(i * 16, j * 16, 16, 16);
                if (Main.LocalPlayer.Hitbox.Intersects(rect))
                {
                    Main.LocalPlayer.Hurt(PlayerDeathReason.ByCustomReason(Main.LocalPlayer.name + " was maimed"), 80, 0);
                    Main.tile[i, j].ClearTile();
                }
            }
        }
    }
}