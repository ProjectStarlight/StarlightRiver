using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
	public class ProtectionGlobalItem : GlobalItem
	{
		public override bool Autoload(ref string name)
		{
            On.Terraria.Player.PickTile += DontPickInZone;
            On.Terraria.WorldGen.PlaceTile += DontManuallyPlaceInZone;
            On.Terraria.WorldGen.PlaceWire += DontPlaceWire;
            On.Terraria.WorldGen.PlaceWire2 += DontPlaceWire2;
            On.Terraria.WorldGen.PlaceWire3 += DontPlaceWire3;
            On.Terraria.WorldGen.PlaceWire4 += DontPlaceWire4;
            On.Terraria.WorldGen.PlaceActuator += DontPlaceActuator;
            return base.Autoload(ref name);
		}

        /// <summary>
        /// Returns true if a protected region contains given coordinates
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private bool IsProtected(int x, int y)
        {
            if (!Main.gameMenu || Main.dedServ) //shouldnt trigger while generating the world from the menu
            {
                foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
                {
                    if (region.Contains(new Point(x, y)))
                        return true;
                }
            }

            return false;
        }


        private bool DontPlaceWire(On.Terraria.WorldGen.orig_PlaceWire orig, int x, int y)
        {
            if (IsProtected(x, y))
            {
                FailFX(new Point16(x, y));
                return false;
            }
            return orig(x, y);
        }

        private bool DontPlaceWire2(On.Terraria.WorldGen.orig_PlaceWire2 orig, int x, int y)
        {
            if (IsProtected(x, y))
            {
                FailFX(new Point16(x, y));
                return false;
            }
            return orig(x, y);
        }

        private bool DontPlaceWire3(On.Terraria.WorldGen.orig_PlaceWire3 orig, int x , int y)
        {
            if (IsProtected(x, y))
            {
                FailFX(new Point16(x, y));
                return false;
            }
            return orig(x, y);
        }

        private bool DontPlaceWire4(On.Terraria.WorldGen.orig_PlaceWire4 orig, int x, int y)
        {
            if (IsProtected(x, y))
            {
                FailFX(new Point16(x, y));
                return false;
            }
            return orig(x, y);
        }

        private bool DontPlaceActuator(On.Terraria.WorldGen.orig_PlaceActuator orig, int x, int y)
        {
            if (IsProtected(x, y))
            {
                FailFX(new Point16(x, y));
                return false;
            }
            return orig(x, y);
        }

        private void DontPickInZone(On.Terraria.Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
		{
            if (IsProtected(x, y))
			{
                FailFX(new Point16(x, y));
                return;
			}

            orig(self, x, y, pickPower);
        }

        private bool DontManuallyPlaceInZone(On.Terraria.WorldGen.orig_PlaceTile orig, int i, int j, int type, bool mute, bool forced, int plr, int style)
        {
            if (IsProtected(i, j)) {
                FailFX(new Point16(i, j));
                return false;
            }

            return orig(i, j, type, mute, forced, plr, style);
        }

        public override bool CanUseItem(Item item, Player player)
		{
            if (player != Main.LocalPlayer)
                return base.CanUseItem(item, player);

            //list of item ids that don't place items in the normal way so we need to specifically take them out
            List<int> forbiddenItemIds = new List<int>{ ItemID.WaterBucket, ItemID.LavaBucket, ItemID.HoneyBucket, ItemID.BottomlessBucket,
                                                        ItemID.Wrench, ItemID.BlueWrench, ItemID.GreenWrench, ItemID.YellowWrench, ItemID.MulticolorWrench,
                                                        ItemID.ActuationRod, ItemID.Actuator, ItemID.WireKite, ItemID.WireCutter, ItemID.WireBulb,
                                                        ItemID.Paintbrush, ItemID.PaintRoller, ItemID.PaintScraper,
                                                        ItemID.SpectrePaintbrush, ItemID.SpectrePaintRoller, ItemID.SpectrePaintScraper};

            if (item.createTile != -1 || item.createWall != -1 || forbiddenItemIds.Contains(item.type))
            {
                Point16 targetPoint = Main.SmartCursorEnabled ? new Point16(Main.SmartCursorX, Main.SmartCursorY) : new Point16(Player.tileTargetX, Player.tileTargetY);

                Tile tile = Framing.GetTileSafely(targetPoint.X, targetPoint.Y);

                if (tile?.wall == WallType<AuroraBrickWall>())
                {
                    for (int k = 0; k < Main.maxProjectiles; k++) //this is gross. Unfortunate.
                    {
                        Projectile proj = Main.projectile[k];

                        if (proj.active && proj.timeLeft > 10 && proj.modProjectile is InteractiveProjectile && (proj.modProjectile as InteractiveProjectile).CheckPoint(targetPoint.X, targetPoint.Y))
                        {
                            return base.CanUseItem(item, player);
                        }
                    }
                    player.AddBuff(BuffID.Cursed, 10, false);
                    FailFX(targetPoint);
                    return false;
                }

                if (IsProtected(targetPoint.X, targetPoint.Y))
                {
                    player.AddBuff(BuffID.Cursed, 10, false);
                    FailFX(targetPoint);
                    return false;
                }
            }

            return base.CanUseItem(item, player);
        }

        private void FailFX(Point16 pos)
        {
            Main.PlaySound(SoundID.DD2_LightningBugZap, pos.ToVector2() * 16);

            for (int k = 0; k < 10; k++)
                Dust.NewDust(pos.ToVector2() * 16, 16, 16, DustType<Dusts.Glow>(), 0, 0, 0, Color.Red, 0.2f);
        }
    }

    public class ProtectionGlobalProjectile : GlobalProjectile //gravestones shouldnt do terrible things
	{
		public override void PostAI(Projectile projectile)
		{
            if(projectile.aiStyle == 17)
			{
                foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
                    if (region.Contains(new Point((int)projectile.Center.X / 16, (int)projectile.Center.Y / 16)))
                    {
                        projectile.active = false;
                    }
            }
		}
	}

    public class ProtectionWorld : ModWorld
	{
        public static List<Rectangle> ProtectedRegions = new List<Rectangle>();

        public override void Load(TagCompound tag)
		{
            ProtectedRegions.Clear();

            int length = tag.GetInt("RegionCount");

            for(int k = 0; k < length; k++)
			{
                ProtectedRegions.Add(new Rectangle
                    (
                    tag.GetInt("x" + k),
                    tag.GetInt("y" + k),
                    tag.GetInt("w" + k),
                    tag.GetInt("h" + k)
                    ));
			}
		}

		public override TagCompound Save()
		{
            var tag = new TagCompound()
            {
                ["RegionCount"] = ProtectedRegions.Count
            };

            for(int k = 0; k < ProtectedRegions.Count; k++)
			{
                var region = ProtectedRegions[k];
                tag.Add("x" + k, region.X);
                tag.Add("y" + k, region.Y);
                tag.Add("w" + k, region.Width);
                tag.Add("h" + k, region.Height);
            }

            return tag;
		}

        public override void NetSend(BinaryWriter writer)
        {
            writer.Write(ProtectedRegions.Count);

            for (int i = 0; i < ProtectedRegions.Count; i++)
            {
                var region = ProtectedRegions[i];
                writer.Write(region.X);
                writer.Write(region.Y);
                writer.Write(region.Width);
                writer.Write(region.Height);
            }
        }

        public override void NetReceive(BinaryReader reader)
        {
            ProtectedRegions.Clear();

            int numRegions = reader.ReadInt32();

            for (int i = 0; i < numRegions; i++)
            {
                ProtectedRegions.Add(new Rectangle
                {
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32(),
                    Width = reader.ReadInt32(),
                    Height = reader.ReadInt32()
                });
            }
        }
    }
}