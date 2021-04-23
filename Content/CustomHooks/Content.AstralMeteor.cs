using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
    class AstralMeteor : HookGroup
    {
        //Swaps the vanilla meteor events out, could create conflicts if other mods attempt the same but shouldnt be anything fatal
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public override void Load()
        {
            On.Terraria.WorldGen.meteor += AluminumMeteor;
        }

        private bool AluminumMeteor(On.Terraria.WorldGen.orig_meteor orig, int i, int j)
        {
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().Shake += 80;
            Main.PlaySound(SoundID.DD2_ExplosiveTrapExplode);

            if (StarlightWorld.HasFlag(WorldFlags.AluminumMeteors))
            {
                Point16 target = new Point16();

                while (!CheckAroundMeteor(target))
                {
                    int x = Main.rand.Next(Main.maxTilesX);

                    for (int y = 0; y < Main.maxTilesY; y++)
                    {
                        if (Framing.GetTileSafely(x, y).active())
                        {
                            target = new Point16(x, y);
                            break;
                        }
                    }
                }

                for (int x = -35; x < 35; x++)
                    for (int y = -35; y < 35; y++)
                    {
                        if (WorldGen.InWorld(target.X + x, target.Y + y) && Framing.GetTileSafely(target.X + x, target.Y + y).collisionType == 1)
                        {
                            float dist = new Vector2(x, y).Length();
                            if (dist < 8) WorldGen.KillTile(target.X + x, target.Y + y);

                            if (dist > 8 && dist < 15)
                            {
                                WorldGen.PlaceTile(target.X + x, target.Y + y, ModContent.TileType<Content.Tiles.AstralMeteor.AluminumOre>(), true, true);
                                WorldGen.SlopeTile(target.X + x, target.Y + y, 0);
                            }

                            if (dist > 15 && dist < 30 && Main.rand.Next((int)dist - 15) == 0)
                            {
                                WorldGen.PlaceTile(target.X + x, target.Y + y, ModContent.TileType<Content.Tiles.AstralMeteor.AluminumOre> (), true, true);
                                WorldGen.SlopeTile(target.X + x, target.Y + y, 0);
                            }
                        }
                    }

                if (Main.netMode == NetmodeID.SinglePlayer)
                    Main.NewText("An asteroid has landed!", new Color(107, 233, 231));

                else if (Main.netMode == NetmodeID.Server)
                    NetMessage.BroadcastChatMessage(NetworkText.FromLiteral("An asteroid has landed!"), new Color(107, 233, 231));

                return true;
            }

            else return orig(i, j);
        }

        private bool CheckAroundMeteor(Point16 test)
        {
            if (test == Point16.Zero) return false;

            for (int x = -35; x < 35; x++)
                for (int y = -35; y < 35; y++)
                {
                    if (WorldGen.InWorld(test.X + x, test.Y + y))
                    {
                        Tile tile = Framing.GetTileSafely(test + new Point16(x, y));

                        if (tile.type == TileID.Containers || tile.type == TileID.Containers2)
                            return false;
                    }
                }

            if (Main.npc.Any(n => n.active && n.friendly && Vector2.Distance(n.Center, test.ToVector2() * 16) <= 35 * 16)) return false;
            else return true;
        }
    }
}