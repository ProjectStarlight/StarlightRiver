using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Items;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Permafrost
{
    class AuroraBrick : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSet(this, 0, DustID.Ice, SoundID.Tink, new Color(100, 255, 255), ItemType<AuroraBrickItem>());

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            int off = i + j;

            float sin2 = (float)Math.Sin(StarlightWorld.rottime + off * 0.2f * 0.2f);
            float cos = (float)Math.Cos(StarlightWorld.rottime + off * 0.2f);
            drawColor = new Color(100 * (1 + sin2) / 255f, 140 * (1 + cos) / 255f, 180 / 255f);
            Lighting.AddLight(new Vector2(i, j) * 16, drawColor.ToVector3() * 0.02f);
        }
    }

    class AuroraBrickDoor : AuroraBrick
    {
        public override void NearbyEffects(int i, int j, bool closer) => Main.tile[i, j].inActive(StarlightWorld.SquidBossOpen);
    }

    class AuroraBrickItem : QuickTileItem
    {
        public AuroraBrickItem() : base("Aurora Brick", "Oooh... Preeetttyyy", TileType<AuroraBrick>(), ItemRarityID.White) { }
    }

    class AuroraBrickDoorItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public AuroraBrickDoorItem() : base("Debug Brick Placer", "", TileType<AuroraBrickDoor>(), ItemRarityID.White) { }
    }
}
