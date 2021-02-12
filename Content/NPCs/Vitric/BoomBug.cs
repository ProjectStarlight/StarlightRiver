using Microsoft.Xna.Framework;

using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Tiles.Vitric;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class BoomBug : ModNPC
    {
        private const int animFramesLoop = 6; //amount of frames in the main loop
        private readonly float AnimSpeedMult = 0.3f;

        //public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/BoomBug";
        public override string Texture => AssetDirectory.Debug;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("[PH]BoomBug");
        }

        public override void SetDefaults()
        {
            npc.width = 50;
            npc.height = 42;
            npc.knockBackResist = 0.8f;
            npc.lifeMax = 80;
            npc.noGravity = false;
            npc.noTileCollide = false;
            npc.damage = 10;
            npc.aiStyle = -1;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath4;

            npc.direction = Main.rand.Next(2) == 0 ? 1 : -1;
            npc.spriteDirection = npc.direction;
        }

        public override void AI()
        {
            
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return 0;
        }

        public override void NPCLoot()
        {

        }
    }
}