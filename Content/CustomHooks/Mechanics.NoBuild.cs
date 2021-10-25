using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
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
			return base.Autoload(ref name);
		}

		private void DontPickInZone(On.Terraria.Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
		{
            foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
                if (region.Contains(new Point(x, y)))
				{
                    FailFX(new Point16(x, y));
                    return;
				}

            orig(self, x, y, pickPower);
        }

        private bool DontManuallyPlaceInZone(On.Terraria.WorldGen.orig_PlaceTile orig, int i, int j, int type, bool mute, bool forced, int plr, int style)
        {
            if(!Main.gameMenu) //shouldnt trigger while generating the world from the menu
			{
                foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
                    if (region.Contains(new Point(i, j)))
                    {
                        FailFX(new Point16(i, j));
                        return false;
                    }
            }

            return orig(i, j, type, mute, forced, plr, style);
        }

        public override bool CanUseItem(Item item, Player player)
		{
            if (player != Main.LocalPlayer)
                return base.CanUseItem(item, player);

            if (item.createTile != -1 || item.type == ItemID.WaterBucket || item.type == ItemID.LavaBucket || item.type == ItemID.HoneyBucket)
            {
                Point16 targetPoint = Main.SmartCursorEnabled ? new Point16(Main.SmartCursorX, Main.SmartCursorY) : new Point16(Player.tileTargetX, Player.tileTargetY);

                Tile tile = Framing.GetTileSafely(targetPoint.X, targetPoint.Y);                

                foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
                    if (region.Contains(new Point(targetPoint.X, targetPoint.Y)))
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
	}
}