using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles.Vitric;
using StarlightRiver.Core;
using System;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
    internal class CrystalPopper : ModNPC
    {
        private const int animFramesLoop = 6; //amount of frames in the main loop
        private readonly float AnimSpeedMult = 0.3f;

        public override string Texture => AssetDirectory.VitricNpc + Name;

        public override bool Autoload(ref string name)
        {
            for (int k = 0; k <= 4; k++)
                mod.AddGore(AssetDirectory.VitricNpc + "Gore/CrystalPopperGore" + k);

            return base.Autoload(ref name);
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Sand Bat");
            Main.npcFrameCount[npc.type] = 7;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(npc.noGravity);
            writer.Write(npc.target);
            writer.WritePackedVector2(npc.velocity);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            npc.noGravity = reader.ReadBoolean();
            npc.target = reader.ReadInt32();
            npc.velocity = reader.ReadPackedVector2();
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
            npc.npcSlots = 1;

            npc.direction = Main.rand.Next(2) == 0 ? 1 : -1;
            npc.spriteDirection = npc.direction;
        }

        const int maxIgnoreDamage = 1;

        private void ExitSleep()
        {
            npc.ai[0] = 1;
            npc.noGravity = true;
            if (Main.netMode != NetmodeID.MultiplayerClient)
                npc.netUpdate = true;
        }

        public override void AI()
        {
            npc.TargetClosest(true);
            switch (npc.ai[0])
            {
                case 0://sleeping: in ground checking for player
                    npc.velocity.X *= 0.9f;
                    if (Vector2.Distance(Main.player[npc.target].Center, npc.Center) <= 180)
                        ExitSleep();
                    break;

                case 1://shoot out of ground and attack
                    npc.ai[1]++;

                    if (npc.ai[1] == 1) npc.velocity.Y = -20;

                    npc.velocity.Y += 0.6f;

                    for (int k = 0; k <= 3; k++)
                        Dust.NewDust(npc.position, 32, 32, DustID.Sandstorm);

                    if (npc.ai[1] >= 30)
                    {
                        npc.velocity.Y = 0;
                        npc.ai[1] = 0;
                        npc.ai[0] = 2;

                        if (Main.netMode != NetmodeID.MultiplayerClient)
                        {
                            for (int k = -1; k <= 1; k++)
                                Projectile.NewProjectile(npc.Center, Vector2.Normalize(Main.player[npc.target].Center - npc.Center).RotatedBy(k * 0.5f) * 6, ProjectileType<Bosses.VitricBoss.GlassSpike>(), 10, 0);
                        }

                        npc.velocity = Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * -5.5f;

                        if (Main.netMode == NetmodeID.Server)
                            npc.netUpdate = true;
                    }
                    break;

                case 2://seek and destroy
                    npc.velocity += Vector2.Normalize(Main.player[npc.target].Center - npc.Center) * 0.08f;

                    if (npc.velocity.LengthSquared() > 25) npc.velocity = Vector2.Normalize(npc.velocity) * 5f;

                    if (npc.collideX && Math.Abs(npc.velocity.X) > 1f) npc.velocity.X = Vector2.Normalize(-npc.velocity).X * 1.5f;
                    if (npc.collideY && Math.Abs(npc.velocity.Y) > 1f) npc.velocity.Y = Vector2.Normalize(-npc.velocity).Y * 1.5f;

                    npc.spriteDirection = Main.player[npc.target].Center.X - npc.Center.X < 0 ? -1 : 1;
                    break;
            }
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (npc.life <= 0 && Main.netMode != NetmodeID.Server)
            {
                for (int k = 0; k <= 4; k++)
                    Gore.NewGoreDirect(npc.position, Vector2.Zero, ModGore.GetGoreSlot(AssetDirectory.VitricNpc + "Gore/CrystalPopperGore" + k));
            }

            if (npc.ai[0] == 0 && damage > maxIgnoreDamage)
                ExitSleep();
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            Tile tile = Framing.GetTileSafely(spawnInfo.spawnTileX, spawnInfo.spawnTileY);
            return tile.active() && spawnInfo.spawnTileType != TileType<VitricSpike>() && spawnInfo.player.GetModPlayer<BiomeHandler>().ZoneGlass ? 95f : 0f;
        }

        public override void NPCLoot()
        {
            Item.NewItem(npc.getRect(), mod.ItemType("VitricSandItem"), Main.rand.Next(10, 12));
        }

        public override void FindFrame(int frameHeight)
        {
            switch (npc.ai[0])
            {
                case 0: npc.frame.Y = frameHeight * 6; break;
                case 1: npc.frame.Y = frameHeight * 0; break;
                case 2:
                    npc.frameCounter++;//skele frame-code
                    if ((int)(npc.frameCounter * AnimSpeedMult) >= animFramesLoop)
                        npc.frameCounter = 0;
                    npc.frame.Y = (int)(npc.frameCounter * AnimSpeedMult) * frameHeight; break;
            }
        }
    }

    internal class VitricBatBanner : ModBanner
    {
        public VitricBatBanner() : base("VitricBatBannerItem", NPCType<CrystalPopper>(), AssetDirectory.VitricNpc) { }
    }

    internal class VitricBatBannerItem : QuickBannerItem
    {
        public VitricBatBannerItem() : base("VitricBatBanner", "Sand Bat", AssetDirectory.VitricNpc) { }
    }
}