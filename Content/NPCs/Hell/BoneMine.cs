using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;
using StarlightRiver.Core;
using StarlightRiver.Helpers;


namespace StarlightRiver.Content.NPCs.Hell
{
    internal class BoneMine : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Hell/BoneMine";

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Bone Mine");
        }

        public override void SetDefaults()
        {
            npc.width = 50;
            npc.knockBackResist = 0f;
            npc.height = 50;
            npc.lifeMax = 200;
            npc.HitSound = SoundID.NPCHit8;
            npc.DeathSound = SoundID.NPCDeath33;
            npc.noGravity = true;
            npc.damage = 100;
            npc.aiStyle = -1;
            npc.lavaImmune = true;
        }

        public override bool CheckDead()
        {
            Main.PlaySound(SoundID.Item14, npc.Center);
            //Projectile.NewProjectile(npc.Center, Vector2.Zero, ProjectileType<AOEExplosionHostile>(), npc.damage, 3, 255, 128); TODO: New explosion
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            spriteBatch.Draw(GetTexture(Texture), npc.position - Main.screenPosition + new Vector2((float)Math.Sin(npc.ai[0]) * 4f, 0), drawColor);
            return false;
        }

        public override void AI()
        {
            npc.ai[0] += 0.02f;
            if (npc.ai[0] >= 6.28f) npc.ai[0] = 0;

            if (npc.ai[1] == 0)
            {
                if (Main.player.Any(player => Vector2.Distance(player.Center, npc.Center) <= 64)) //arm
                    npc.ai[1] = 1;

                if (Main.player.Any(player => Vector2.Distance(player.Center, npc.Center) <= 128)) //warning ring
                    Dust.NewDustPerfect(npc.Center + Vector2.One.RotatedByRandom(6.28f) * 42, DustType<Dusts.Stamina>());
            }
            else
            {
                npc.ai[2]++;
                if (npc.ai[2] % 10 == 0) Main.PlaySound(SoundID.MaxMana, (int)npc.Center.X, (int)npc.Center.Y, 1, 1, 0.5f); //warning beep
                if (npc.ai[2] >= 45) npc.Kill(); //detonate
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Player player = spawnInfo.player;
            Vector2 spawnPos = new Vector2(spawnInfo.spawnTileX * 16, spawnInfo.spawnTileY * 16);

            return player.position.Y >= Main.maxTilesY - 200 && Main.tile[spawnInfo.spawnTileX, spawnInfo.spawnTileY].liquid != 0 &&
                !Main.npc.Any(npc => npc.active && npc.type == NPCType<BoneMine>() && Vector2.Distance(npc.Center, spawnPos) <= 128)
                ? 1
                : (float)0;
        }
    }
}