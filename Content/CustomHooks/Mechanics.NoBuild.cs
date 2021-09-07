using Microsoft.Xna.Framework;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Tiles.Permafrost;
using System.Collections.Generic;
using Terraria;
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
			return base.Autoload(ref name);
		}

		private void DontPickInZone(On.Terraria.Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
		{
            foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
                if (region.Contains(new Point(x, y)))
				{
                    FailFX();
                    return;
				}

            orig(self, x, y, pickPower);
        }

		public override bool CanUseItem(Item item, Player player)
		{
            if (player != Main.LocalPlayer)
                return base.CanUseItem(item, player);

            if (item.createTile != -1 || item.type == ItemID.WaterBucket || item.type == ItemID.LavaBucket || item.type == ItemID.HoneyBucket)
            {
                Tile tile = Framing.GetTileSafely(Player.tileTargetX, Player.tileTargetY);

                foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
                    if (region.Contains(new Point(Player.tileTargetX, Player.tileTargetY)))
                    {
                        player.AddBuff(BuffID.Cursed, 10, false);
                        FailFX();
                        return false;
                    }

                if (tile.wall == WallType<AuroraBrickWall>())
                {
                    for (int k = 0; k < Main.maxProjectiles; k++) //this is gross. Unfortunate.
                    {
                        Projectile proj = Main.projectile[k];

                        if (proj.active && proj.timeLeft > 10 && proj.modProjectile is InteractiveProjectile && (proj.modProjectile as InteractiveProjectile).CheckPoint(Player.tileTargetX, Player.tileTargetY))
                        {
                            return true;
                        }
                    }
                    player.AddBuff(BuffID.Cursed, 10, false);
                    FailFX();
                    return false;
                }
            }

            return base.CanUseItem(item, player);
        }

        private void FailFX()
        {
            Main.PlaySound(SoundID.DD2_LightningBugZap);

            for (int k = 0; k < 10; k++)
                Dust.NewDust(new Vector2(Player.tileTargetX, Player.tileTargetY) * 16, 16, 16, DustType<Dusts.Glow>(), 0, 0, 0, Color.Red, 0.2f);
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