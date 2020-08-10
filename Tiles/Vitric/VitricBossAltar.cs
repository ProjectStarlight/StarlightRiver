using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.NPCs.Boss.VitricBoss;
using StarlightRiver.Projectiles.Dummies;
using System;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Tiles.Vitric
{
    internal class VitricBossAltar : DummyTile
    {
        public override int DummyType => ProjectileType<VitricBossAltarDummy>();

        public override bool SpawnConditions(int i, int j)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            return (tile.frameX % 90 == 0 && tile.frameY == 0);
        }

        public override void SetDefaults()
        {
            QuickBlock.QuickSetFurniture(this, 5, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 113, 113), false, false, "Ceiro's Altar");
            minPick = int.MaxValue;
        }

        public override bool NewRightClick(int i, int j)
        {
            (Dummy.modProjectile as VitricBossAltarDummy).SpawnBoss();
            return true;
        }
    }

    internal class VitricBossAltarDummy : Dummy
    {
        public VitricBossAltarDummy() : base(TileType<VitricBossAltar>(), 80, 112) { }

        public override void Collision(Player player)
        {
            Point16 parentPos = new Point16((int)projectile.position.X / 16, (int)projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

            if (parent.frameX == 0 && Abilities.AbilityHelper.CheckDash(player, projectile.Hitbox))
            {
                Main.PlaySound(SoundID.Shatter);
                for (int k = 0; k < 100; k++) Dust.NewDust(projectile.position, projectile.width, projectile.height, DustType<Dusts.Glass2>(), 0, 0, 0, default, 1.2f);

                for (int x = parentPos.X; x < parentPos.X + 5; x++)
                    for (int y = parentPos.Y; y < parentPos.Y + 7; y++)
                    {
                        Framing.GetTileSafely(x, y).frameX += 90;
                    }
            }
        }

        public override void Update()
        {
            Point16 parentPos = new Point16((int)projectile.position.X / 16, (int)projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

            if (parent.frameX == 90 && !StarlightWorld.GlassBossOpen)
            {
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 1;
                Dust.NewDust(projectile.Center + new Vector2(-632, projectile.height / 2), 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f));
                Dust.NewDust(projectile.Center + new Vector2(72, projectile.height / 2), 560, 1, DustType<Dusts.Sand>(), 0, Main.rand.NextFloat(-5f, -1f), Main.rand.Next(255), default, Main.rand.NextFloat(1.5f));
                //Main.PlaySound(SoundID.); TODO: Rumble sound

                projectile.ai[1]++;
                if (projectile.ai[1] > 120)
                {
                    StarlightWorld.GlassBossOpen = true;
                    if (Main.LocalPlayer.GetModPlayer<BiomeHandler>().ZoneGlass)
                    {
                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMovePan = projectile.Center + new Vector2(0, -400);
                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = projectile.Center;
                        Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = VitricBackdropLeft.Risetime + 120;
                    }
                    projectile.ai[1] = 0;
                }
            }

            //This controls spawning the rest of the arena
            if (!Main.npc.Any(n => n.active && (n.type == NPCType<VitricBackdropLeft>() || n.type == NPCType<VitricBoss>()))) //TODO: Need to find a better check
            {
                Vector2 center = projectile.Center + new Vector2(0, 60);
                int timerset = StarlightWorld.GlassBossOpen ? 360 : 0; //the arena should already be up if it was opened before

                int index = NPC.NewNPC((int)center.X + 352, (int)center.Y, NPCType<VitricBackdropRight>(), 0, timerset);
                if (StarlightWorld.GlassBossOpen && Main.npc[index].modNPC is VitricBackdropRight) (Main.npc[index].modNPC as VitricBackdropRight).SpawnPlatforms(false);

                index = NPC.NewNPC((int)center.X - 352, (int)center.Y, NPCType<VitricBackdropLeft>(), 0, timerset);
                if (StarlightWorld.GlassBossOpen && Main.npc[index].modNPC is VitricBackdropLeft) (Main.npc[index].modNPC as VitricBackdropLeft).SpawnPlatforms(false);
            }

            //controls the drawing of the barriers
            if (projectile.ai[0] < 120 && Main.npc.Any(n => n.active && n.type == NPCType<VitricBoss>()))
            {
                projectile.ai[0]++;
                if (projectile.ai[0] % 3 == 0) Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 2; //screenshake
                if (projectile.ai[0] == 119) //hitting the top
                {
                    Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 25;
                    for (int k = 0; k < 5; k++) Main.PlaySound(Terraria.ID.SoundID.Tink);
                }
            }
            else if (!Main.npc.Any(n => n.active && n.type == NPCType<VitricBoss>())) projectile.ai[0] = 0; //TODO fix this later
        }

        public override void PostDraw(SpriteBatch spriteBatch, Color lightColor) //actually drawing the barriers and item indicator
        {
            Point16 parentPos = new Point16((int)projectile.position.X / 16, (int)projectile.position.Y / 16);
            Tile parent = Framing.GetTileSafely(parentPos.X, parentPos.Y);

            if (parent.frameX >= 90 && !NPC.AnyNPCs(NPCType<VitricBoss>()))
                Helper.DrawSymbol(spriteBatch, projectile.Center - Main.screenPosition + new Vector2(0, (float)Math.Sin(StarlightWorld.rottime) * 5 - 20), new Color(150, 220, 250));

            else if (parent.frameX < 90)
            {
                Texture2D glow = GetTexture("StarlightRiver/Tiles/Vitric/VitricBossAltarGlow");
                spriteBatch.Draw(glow, projectile.position - Main.screenPosition + new Vector2(3, -1), glow.Frame(), Color.White * (float)Math.Sin(StarlightWorld.rottime), 0, Vector2.Zero, 1, 0, 0);
            }

            //Barriers
            Vector2 center = projectile.Center + new Vector2(0, 56);
            Texture2D tex = GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/VitricBossBarrier");
            Color color = new Color(180, 225, 255);
            int off = (int)(projectile.ai[0] / 120f * tex.Height);

            spriteBatch.Draw(tex, new Rectangle((int)center.X - 790 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off),
                new Rectangle(0, 0, tex.Width, off), color);

            spriteBatch.Draw(tex, new Rectangle((int)center.X + 606 - (int)Main.screenPosition.X, (int)center.Y - off - 16 - (int)Main.screenPosition.Y, tex.Width, off),
                new Rectangle(0, 0, tex.Width, off), color);
        }

        public void SpawnBoss() => NPC.NewNPC((int)projectile.Center.X, (int)projectile.Center.Y + 500, NPCType<VitricBoss>());
    }
}