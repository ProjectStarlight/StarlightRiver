using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Hostile
{
    internal class CrystalSlime : ModNPC
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Crystal Slime");
            Main.npcFrameCount[npc.type] = 2;
        }

        public override void SetDefaults()
        {
            npc.width = 48;
            npc.height = 32;
            npc.damage = 10;
            npc.defense = 5;
            npc.lifeMax = 25;
            npc.HitSound = SoundID.NPCHit42;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.value = 10f;
            npc.knockBackResist = 0.6f;
            npc.aiStyle = 1;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Lighting.GetColor((int)npc.position.X / 16, (int)npc.position.Y / 16) * 0.75f;
        }

        private bool shielded = true;

        public override void AI()
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();

            if (AbilityHelper.CheckDash(player, npc.Hitbox))
            {
                if (shielded)
                {
                    shielded = false;
                    npc.velocity += player.velocity * 0.5f;
                    mp.dash.Active = false;
                    player.velocity *= (player.velocity.X == 0) ? -0.4f : -0.2f;
                    player.immune = true;
                    player.immuneTime = 10;

                    Main.PlaySound(SoundID.Shatter, npc.Center);
                    for (int k = 0; k <= 20; k++)
                    {
                        Dust.NewDust(npc.position, 48, 32, mod.DustType("Glass2"), Main.rand.Next(-3, 2), -3, 0, default, 1.7f);
                    }
                }
            }

            if (shielded)
            {
                npc.immortal = true;
                npc.HitSound = SoundID.NPCHit42;
            }
            else
            {
                npc.immortal = false;
                npc.HitSound = SoundID.NPCHit1;
            }
        }

        public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
        {
            if (AbilityHelper.CheckDash(target, npc.Hitbox))
            {
                target.immune = true;
                target.immuneTime = 5;
            }
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            return (spawnInfo.player.ZoneRockLayerHeight && spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneGlass) ? 1f : 0f;
        }

        public override void NPCLoot()
        {
            if (Main.rand.NextFloat() < 0.50f) { Item.NewItem(npc.getRect(), ItemType<Items.Vitric.VitricOre>(), Main.rand.Next(4, 5)); }
            Item.NewItem(npc.getRect(), ItemID.Gel, Main.rand.Next(5, 6));
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (shielded)
            {
                Color color = new Color(255, 255, 255) * (float)Math.Sin(StarlightWorld.rottime);
                spriteBatch.Draw(GetTexture("StarlightRiver/NPCs/Hostile/Crystal"), npc.position - Main.screenPosition + new Vector2(-2, -5), Lighting.GetColor((int)npc.position.X / 16, (int)npc.position.Y / 16));
                spriteBatch.Draw(GetTexture("StarlightRiver/NPCs/Hostile/CrystalGlow"), npc.position - Main.screenPosition + new Vector2(-3, -6), color);
            }
        }
    }
}