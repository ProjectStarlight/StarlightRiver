using Microsoft.Xna.Framework;
using StarlightRiver.Abilities;
using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Passive
{
    internal class DesertWisp : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Desert Wisp");
        }

        public override void SetDefaults()
        {
            npc.width = 8;
            npc.height = 8;
            npc.damage = 0;
            npc.defense = 0;
            npc.lifeMax = 1;
            npc.noGravity = true;
            npc.noTileCollide = true;
            npc.dontTakeDamage = true;
            npc.value = 0f;
            npc.knockBackResist = 0f;
            npc.aiStyle = 65;
        }

        public override void AI()
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            Vector2 distance = player.Center - npc.Center;

            Dust.NewDustPerfect(npc.Center, DustType<Dusts.Air>(), Vector2.Zero);

            if ((distance.Length() <= 180 && !(mp.wisp.Active)) || Main.dayTime) npc.ai[3] = 1;

            if (npc.ai[3] == 1)
            {
                npc.velocity.Y = 10;
                npc.velocity.X = 0;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => (spawnInfo.player.ZoneOverworldHeight && !Main.dayTime && spawnInfo.player.ZoneDesert) ? 0.2f : 0f;
    }

    internal class DesertWisp2 : DesertWisp
    {
        public override void AI()
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            Vector2 distance = player.Center - npc.Center;

            Dust.NewDustPerfect(npc.Center, DustType<Dusts.Air>(), Vector2.Zero);

            if (distance.Length() <= 120 && !mp.wisp.Active) npc.velocity += Vector2.Normalize(distance) * -10;
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo) => (spawnInfo.player.ZoneRockLayerHeight && spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneGlass) ? 1f : 0f;
    }
}