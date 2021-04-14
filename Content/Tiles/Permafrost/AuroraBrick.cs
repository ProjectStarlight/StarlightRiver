using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.Tiles.Permafrost
{
    class AuroraBrick : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrick";
            return true;
        }

        public override void SetDefaults() => QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(81, 192, 240), ItemType<AuroraBrickItem>());

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            float off = (float)Math.Sin((i + j) * 0.2f) * 300 + (float)Math.Cos(j * 0.15f) * 200;

            float sin2 = (float)Math.Sin(StarlightWorld.rottime + off * 0.01f * 0.2f);
            float cos = (float)Math.Cos(StarlightWorld.rottime + off * 0.01f);
            Color color = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
            float mult = Lighting.Brightness(i, j);
            //if (mult > 0.1f) mult = 0.1f;

            drawColor = color.MultiplyRGB(Color.White * mult);
        }
    }

    class AuroraBrickDoor : AuroraBrick
    {
        public override void NearbyEffects(int i, int j, bool closer) => Main.tile[i, j].inActive(StarlightWorld.HasFlag(WorldFlags.SquidBossOpen));
    }

    class AuroraBrickItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/Assets/Tiles/Permafrost/AuroraBrickItem";

        public AuroraBrickItem() : base("Aurora Brick", "Oooh... Preeetttyyy", TileType<AuroraBrick>(), ItemRarityID.White) { }
    }

    class AuroraBrickDoorItem : QuickTileItem
    {
        public override string Texture => AssetDirectory.Debug;

        public AuroraBrickDoorItem() : base("Debug Brick Placer", "", TileType<AuroraBrickDoor>(), ItemRarityID.White) { }
    }
}
