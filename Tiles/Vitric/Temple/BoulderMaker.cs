using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;
using Terraria;
using Terraria.ID;
using Microsoft.Xna.Framework;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Items;
using Terraria.Audio;

namespace StarlightRiver.Tiles.Vitric.Temple
{
    class BoulderMaker : ModTile
    {
        public override void SetDefaults() => QuickBlock.QuickSetFurniture(this, 6, 1, DustType<Dusts.Sand>(), SoundID.Tink, false, new Color(100, 80, 10));

        public override void NearbyEffects(int i, int j, bool closer)
        {
            Tile tile = Framing.GetTileSafely(i, j);

            if(StarlightWorld.DesertOpen && tile.frameX == 0 && !Main.npc.Any(n => n.active && n.type == NPCType<Boulder>())) NPC.NewNPC(i * 16 + 48, j * 16, NPCType<Boulder>(), 0, j * 16);
        }
    }

    class BoulderMakerItem : QuickTileItem
    {
        public override string Texture => "StarlightRiver/MarioCumming";

        public BoulderMakerItem() : base("Boulder Maker", "Titties", TileType<BoulderMaker>(), 1) { }
    }

    class Boulder : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Temple Boulder");
        }

        public override void SetDefaults()
        {
            npc.width = 80;
            npc.height = 80;
            npc.noTileCollide = true;
            npc.dontTakeDamage = true;
            npc.knockBackResist = 0f;
            npc.lifeMax = 2;
            npc.damage = 80;
            npc.aiStyle = -1;
            npc.behindTiles = true;
        }
       
        public override void AI()
        {
            if (npc.position.Y > npc.ai[0]) npc.noTileCollide = false;

            if (npc.velocity.Y == 0 && npc.velocity.X < 15) npc.velocity.X += 0.05f;

            if (npc.collideX) npc.Kill();

            npc.rotation += npc.velocity.X / 40f;
        }

        public override void NPCLoot()
        {
            for(int k = 0; k < 100; k++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, DustID.Stone);
            }

            Main.PlaySound(SoundID.NPCHit42.SoundId, (int)npc.Center.X, (int)npc.Center.Y, 42, 1, -0.8f);
        }
    }
}
