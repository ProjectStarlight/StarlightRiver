using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Helpers;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;


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
            NPC.width = 50;
            NPC.knockBackResist = 0f;
            NPC.height = 50;
            NPC.lifeMax = 200;
            NPC.HitSound = SoundID.NPCHit8;
            NPC.DeathSound = SoundID.NPCDeath33;
            NPC.noGravity = true;
            NPC.damage = 100;
            NPC.aiStyle = -1;
            NPC.lavaImmune = true;
        }

        public override bool CheckDead()
        {
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item14, NPC.Center);
            //Projectile.NewProjectile(NPC.Center, Vector2.Zero, ProjectileType<AOEExplosionHostile>(), NPC.damage, 3, 255, 128); TODO: New explosion
            return true;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            spriteBatch.Draw(Request<Texture2D>(Texture).Value, NPC.position - Main.screenPosition + new Vector2((float)Math.Sin(NPC.ai[0]) * 4f, 0), drawColor);
            return false;
        }

        public override void AI()
        {
            NPC.ai[0] += 0.02f;
            if (NPC.ai[0] >= 6.28f) NPC.ai[0] = 0;

            if (NPC.ai[1] == 0)
            {
                if (Main.player.Any(Player => Vector2.Distance(Player.Center, NPC.Center) <= 64)) //arm
                    NPC.ai[1] = 1;

                if (Main.player.Any(Player => Vector2.Distance(Player.Center, NPC.Center) <= 128)) //warning ring
                    Dust.NewDustPerfect(NPC.Center + Vector2.One.RotatedByRandom(6.28f) * 42, DustType<Dusts.Stamina>());
            }
            else
            {
                NPC.ai[2]++;
                if (NPC.ai[2] % 10 == 0) Terraria.Audio.SoundEngine.PlaySound(SoundID.MaxMana, (int)NPC.Center.X, (int)NPC.Center.Y, 1, 1, 0.5f); //warning beep
                if (NPC.ai[2] >= 45) NPC.Kill(); //detonate
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Player Player = spawnInfo.Player;
            Vector2 spawnPos = new Vector2(spawnInfo.spawnTileX * 16, spawnInfo.spawnTileY * 16);

            return Player.position.Y >= Main.maxTilesY - 200 && Main.tile[spawnInfo.spawnTileX, spawnInfo.spawnTileY].liquid != 0 &&
                !Main.npc.Any(NPC => NPC.active && NPC.type == NPCType<BoneMine>() && Vector2.Distance(NPC.Center, spawnPos) <= 128)
                ? 1
                : (float)0;
        }
    }
}