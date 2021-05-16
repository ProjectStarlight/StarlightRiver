using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using StarlightRiver.Helpers;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class CrystalSlime : ModNPC
    {
        public override string Texture => "StarlightRiver/Assets/NPCs/Vitric/CrystalSlime";

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
            npc.immortal = true;
        }

        public override Color? GetAlpha(Color drawColor)
        {
            return Lighting.GetColor((int)npc.position.X / 16, (int)npc.position.Y / 16) * 0.75f;
        }

        private bool shielded { get => npc.ai[1] == 0; set => npc.ai[1] = value ? 0 : 1; }

        public override void AI()
        {
            npc.TargetClosest(true);
            Player player = Main.player[npc.target];
            AbilityHandler mp = player.GetHandler();

            if (AbilityHelper.CheckDash(player, npc.Hitbox))
                if (shielded)
                {
                    shielded = false;
                    npc.velocity += player.velocity * 0.5f;

                    mp.ActiveAbility?.Deactivate();
                    player.velocity = Vector2.Normalize(player.velocity) * -10f;

                    player.immune = true;
                    player.immuneTime = 10;

                    Main.PlaySound(SoundID.Shatter, npc.Center);
                    for (int k = 0; k <= 20; k++)
                        Dust.NewDust(npc.position, 48, 32, ModContent.DustType<Dusts.GlassGravity>(), Main.rand.Next(-3, 2), -3, 0, default, 1.7f);
                    npc.netUpdate = true;
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

        public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
        {
            if (shielded)
                damage = 0;
            return base.StrikeNPC(ref damage, defense, ref knockback, hitDirection, ref crit);
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
            return spawnInfo.player.GetHandler().Unlocked<Dash>() && spawnInfo.player.ZoneRockLayerHeight && spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneGlass ? 1f : 0f;
        }

        public override void NPCLoot()
        {
            if (Main.rand.NextFloat() < 0.50f) Item.NewItem(npc.getRect(), ItemType<Items.Vitric.VitricOre>(), Main.rand.Next(4, 5)); Item.NewItem(npc.getRect(), ItemID.Gel, Main.rand.Next(5, 6));
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color drawColor)
        {
            if (shielded)
            {
                Color color = Helper.IndicatorColor;
                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/NPCs/Vitric/Crystal"), npc.position - Main.screenPosition + new Vector2(-2, -5), Lighting.GetColor((int)npc.position.X / 16, (int)npc.position.Y / 16));
                spriteBatch.Draw(GetTexture("StarlightRiver/Assets/NPCs/Vitric/CrystalGlow"), npc.position - Main.screenPosition + new Vector2(-3, -6), color);
            }
        }
    }
}