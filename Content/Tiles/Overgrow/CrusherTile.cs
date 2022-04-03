using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.NPCs.Overgrow;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Overgrow
{
	internal class CrusherTile : ModTile
    {
        public override bool Autoload(ref string name, ref string texture)
        {
            texture = AssetDirectory.OvergrowTile + name;
            return true;
        }

        public override void SetDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLighted[Type] = false;
            Main.tileMerge[Type][TileType<GrassOvergrow>()] = true;
            Main.tileMerge[Type][Mod.GetTile("BrickOvergrow").Type] = true;
            Main.tileFrameImportant[Type] = true;
            dustType = Mod.DustType("Gold2");
            AddMapEntry(new Color(81, 77, 71));
        }

        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref Color drawColor, ref int nextSpecialDrawIndex)
        {
            Dust.NewDustPerfect(new Vector2(4 + i * 16, 4 + j * 16), DustType<Content.Dusts.GoldWithMovement>());
            Lighting.AddLight(new Vector2(i * 16, j * 16), new Vector3(255, 200, 110) * 0.001f);
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Vector2 pos = new Vector2(4 + i * 16, 4 + j * 16);
            if (!Main.npc.Any(NPC => NPC.type == NPCType<Crusher>() && (NPC.ModNPC as Crusher).Parent == Main.tile[i, j] && NPC.active))
            {
                int crusher = NPC.NewNPC((int)pos.X + 4, (int)pos.Y + 21, NPCType<Crusher>());
                if (Main.npc[crusher].ModNPC is Crusher) (Main.npc[crusher].ModNPC as Crusher).Parent = Main.tile[i, j];
            }
        }
    }

    public class CrusherOvergrowItem : QuickTileItem { public CrusherOvergrowItem() : base("Crusher Trap", "", TileType<CrusherTile>(), 0, AssetDirectory.OvergrowTile) { } }
}