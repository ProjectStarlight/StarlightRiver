using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs.Hostile
{
    internal class JungleCorruptSpore : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bloated Spore");
            Main.npcFrameCount[npc.type] = 4;
        }

        public override void SetDefaults()
        {
            npc.width = 32;
            npc.knockBackResist = 0f;
            npc.height = 32;
            npc.lifeMax = 10;
            npc.HitSound = SoundID.NPCHit8;
            npc.DeathSound = SoundID.NPCDeath12;
            npc.noGravity = true;
            npc.damage = 55;
            npc.aiStyle = -1;
        }

        public override bool CheckDead()
        {
            Main.PlaySound(SoundID.NPCDeath13, npc.Center);
            Projectile.NewProjectile(npc.position + new Vector2(8, 8), Vector2.Zero, mod.ProjectileType("GasPoison"), 25, 0);
            for (int k = 0; k <= 50; k++)
            {
                Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType("Corrupt"), Main.rand.Next(-5, 5), Main.rand.Next(-5, 5), 0, default, 1.4f);
            }
            return true;
        }

        public override void AI()
        {
            Dust.NewDust(npc.position, npc.width, npc.height, mod.DustType("Corrupt"), 0, 0, 0, default, 0.6f);
        }

        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            Helper.Kill(npc);
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return (spawnInfo.player.ZoneRockLayerHeight && !Main.tile[spawnInfo.spawnTileX, spawnInfo.spawnTileY].active() && spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneJungleCorrupt) ? 1f : 0f;
        }

        public int Framecounter = 0;
        public int Gameframecounter = 0;

        public override void FindFrame(int frameHeight)
        {
            if (Gameframecounter++ == 6)
            {
                Framecounter++;
                Gameframecounter = 0;
            }
            npc.frame.Y = 36 * Framecounter;
            if (Framecounter >= 3)
            {
                Framecounter = 0;
            }
        }
    }
}