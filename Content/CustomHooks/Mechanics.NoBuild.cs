﻿using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core.Systems.BossRushSystem;
using System.Collections.Generic;
using System.IO;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader.IO;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
	class ProtectionGlobalTime : GlobalTile
	{
		public override bool CanExplode(int i, int j, int type)
		{
			if (IsProtected(i, j))
				return false;

			return base.CanExplode(i, j, type);
		}

		public bool IsProtected(int x, int y) //TODO: move this to an easily accessible class later (not a fucking global item)
		{
			if (StarlightRiver.debugMode)
				return false;

			if (!Main.gameMenu || Main.dedServ) //shouldnt trigger while generating the world from the menu
			{
				foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
				{
					if (region.Contains(new Point(x, y)))
						return true;
				}

				foreach (Ref<Rectangle> region in ProtectionWorld.RuntimeProtectedRegions)
				{
					if (region.Value.Contains(new Point(x, y)))
						return true;
				}

				Tile tile = Framing.GetTileSafely(x, y);

				// Let the player break their own tombstones atleast.
				if (tile.TileType == TileID.Tombstones)
					return false;

				if (tile.WallType == WallType<Content.Tiles.Vitric.Temple.VitricTempleWall>())
					return true;

				if (tile.WallType == WallType<AuroraBrickWall>())
				{
					for (int k = 0; k < Main.maxProjectiles; k++) //this is gross. Unfortunate.
					{
						Projectile proj = Main.projectile[k];

						if (proj.active && proj.timeLeft > 10 && proj.ModProjectile is InteractiveProjectile && (proj.ModProjectile as InteractiveProjectile).CheckPoint(x, y))
							return false;
					}

					return true;
				}
			}

			if (BossRushSystem.isBossRush)
				return true;

			return false;
		}
	}

	public class ProtectionGlobalItem : GlobalItem
	{
		public override void Load()
		{
			On_Player.PickTile += DontPickInZone;
			On_Player.PickWall += DontPickWallInZone;
			On_WorldGen.PlaceTile += DontManuallyPlaceInZone;
			On_WorldGen.PoundTile += DontPoundTile;
			On_WorldGen.PlaceWire += DontPlaceWire;
			On_WorldGen.PlaceWire2 += DontPlaceWire2;
			On_WorldGen.PlaceWire3 += DontPlaceWire3;
			On_WorldGen.PlaceWire4 += DontPlaceWire4;
			On_WorldGen.PlaceActuator += DontPlaceActuator;
			On_WorldGen.KillTile += DontExplodeAtRuntime;
			On_Player.CheckForGoodTeleportationSpot += DontTeleport;
		}

		/// <summary>
		/// Returns true if a protected region contains given coordinates
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool IsProtected(int x, int y)
		{
			if (StarlightRiver.debugMode)
				return false;

			if (!Main.gameMenu || Main.dedServ) //shouldnt trigger while generating the world from the menu
			{
				foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
				{
					if (region.Contains(new Point(x, y)))
						return true;
				}

				foreach (Ref<Rectangle> region in ProtectionWorld.RuntimeProtectedRegions)
				{
					if (region.Value.Contains(new Point(x, y)))
						return true;
				}

				Tile tile = Framing.GetTileSafely(x, y);

				// Let the player break their own tombstones atleast.
				if (tile.TileType == TileID.Tombstones)
					return false;

				if (tile.WallType == WallType<Content.Tiles.Vitric.Temple.VitricTempleWall>())
					return true;

				if (tile.WallType == WallType<AuroraBrickWall>())
				{
					for (int k = 0; k < Main.maxProjectiles; k++) //this is gross. Unfortunate.
					{
						Projectile proj = Main.projectile[k];

						if (proj.active && proj.timeLeft > 10 && proj.ModProjectile is InteractiveProjectile && (proj.ModProjectile as InteractiveProjectile).CheckPoint(x, y))
							return false;
					}

					return true;
				}
			}

			if (BossRushSystem.isBossRush)
				return true;

			return false;
		}

		private bool DontPoundTile(On_WorldGen.orig_PoundTile orig, int x, int y)
		{
			if (IsProtected(x, y))
			{
				FailFX(new Point16(x, y));
				return false;
			}

			return orig(x, y);
		}

		private bool DontPlaceWire(On_WorldGen.orig_PlaceWire orig, int x, int y)
		{
			if (IsProtected(x, y))
			{
				FailFX(new Point16(x, y));
				return false;
			}

			return orig(x, y);
		}

		private bool DontPlaceWire2(On_WorldGen.orig_PlaceWire2 orig, int x, int y)
		{
			if (IsProtected(x, y))
			{
				FailFX(new Point16(x, y));
				return false;
			}

			return orig(x, y);
		}

		private bool DontPlaceWire3(On_WorldGen.orig_PlaceWire3 orig, int x, int y)
		{
			if (IsProtected(x, y))
			{
				FailFX(new Point16(x, y));
				return false;
			}

			return orig(x, y);
		}

		private bool DontPlaceWire4(On_WorldGen.orig_PlaceWire4 orig, int x, int y)
		{
			if (IsProtected(x, y))
			{
				FailFX(new Point16(x, y));
				return false;
			}

			return orig(x, y);
		}

		private bool DontPlaceActuator(On_WorldGen.orig_PlaceActuator orig, int x, int y)
		{
			if (IsProtected(x, y))
			{
				FailFX(new Point16(x, y));
				return false;
			}

			return orig(x, y);
		}

		private void DontPickWallInZone(On_Player.orig_PickWall orig, Player self, int x, int y, int damage)
		{
			if (IsProtected(x, y))
			{
				FailFX(new Point16(x, y));
				return;
			}

			orig(self, x, y, damage);
		}

		private void DontPickInZone(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
		{
			if (IsProtected(x, y))
			{
				FailFX(new Point16(x, y));
				return;
			}

			orig(self, x, y, pickPower);
		}

		private bool DontManuallyPlaceInZone(On_WorldGen.orig_PlaceTile orig, int i, int j, int type, bool mute, bool forced, int plr, int style)
		{
			if (IsProtected(i, j))
			{
				FailFX(new Point16(i, j));
				return false;
			}

			return orig(i, j, type, mute, forced, plr, style);
		}

		private void DontExplodeAtRuntime(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
		{
			if (IsProtected(i, j) && !WorldGen.generatingWorld)
			{
				// Disabling for now since this breaks dashables... we will need a better fix for bombs only later.
				//FailFX(new Point16(i, j));
				//return;
			}

			orig(i, j, fail, effectOnly, noItem);
		}

		private Vector2 DontTeleport(On_Player.orig_CheckForGoodTeleportationSpot orig, Player self, ref bool canSpawn, int teleportStartX, int teleportRangeX, int teleportStartY, int teleportRangeY, Player.RandomTeleportationAttemptSettings settings)
		{
			Vector2 result = orig(self, ref canSpawn, teleportStartX, teleportRangeX, teleportStartY, teleportRangeY, settings);

			// If invalid spot, recurse untill a valid one is found
			if (IsProtected((int)result.X, (int)result.Y))
			{
				settings.attemptsBeforeGivingUp--;
				result = self.CheckForGoodTeleportationSpot(ref canSpawn, teleportStartX, teleportRangeX, teleportStartY, teleportRangeY, settings);
			}

			return result;
		}

		public override bool CanUseItem(Item Item, Player player)
		{
			if (player != Main.LocalPlayer)
				return base.CanUseItem(Item, player);

			//list of Item ids that don't place Items in the normal way so we need to specifically take them out
			var forbiddenItemIds = new List<int>{ ItemID.WaterBucket, ItemID.LavaBucket, ItemID.HoneyBucket, ItemID.BottomlessBucket,
														ItemID.Wrench, ItemID.BlueWrench, ItemID.GreenWrench, ItemID.YellowWrench, ItemID.MulticolorWrench,
														ItemID.ActuationRod, ItemID.Actuator, ItemID.WireKite, ItemID.WireCutter, ItemID.WireBulb,
														ItemID.Paintbrush, ItemID.PaintRoller, ItemID.PaintScraper,
														ItemID.SpectrePaintbrush, ItemID.SpectrePaintRoller, ItemID.SpectrePaintScraper};

			if (Item.createTile != -1 || Item.createWall != -1 || forbiddenItemIds.Contains(Item.type))
			{
				Point16 targetPoint = Main.SmartCursorIsUsed ? new Point16(Main.SmartCursorX, Main.SmartCursorY) : new Point16(Player.tileTargetX, Player.tileTargetY);

				if (IsProtected(targetPoint.X, targetPoint.Y))
				{
					player.AddBuff(BuffID.Cursed, 10, false);
					FailFX(targetPoint);
					return false;
				}
			}

			return base.CanUseItem(Item, player);
		}

		private void FailFX(Point16 pos)
		{
			Terraria.Audio.SoundEngine.PlaySound(SoundID.DD2_LightningBugZap, pos.ToVector2() * 16);

			for (int k = 0; k < 10; k++)
				Dust.NewDust(pos.ToVector2() * 16, 16, 16, DustType<Dusts.Glow>(), 0, 0, 0, Color.Red, 0.2f);
		}
	}

	public class ProtectionGlobalProjectile : GlobalProjectile //gravestones shouldnt do terrible things
	{
		public override bool AppliesToEntity(Projectile entity, bool lateInstantiation)
		{
			return entity.aiStyle == 17;
		}

		public override void PostAI(Projectile Projectile)
		{

			foreach (Rectangle region in ProtectionWorld.ProtectedRegions)
			{
				if (region.Contains(new Point((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16)))
				{
					Projectile.active = false;
				}
			}

			foreach (Ref<Rectangle> region in ProtectionWorld.RuntimeProtectedRegions)
			{
				if (region.Value.Contains(new Point((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16)))
				{
					Projectile.active = false;
				}
			}
		}
	}

	public class ProtectionWorld : ModSystem
	{
		public static List<Rectangle> ProtectedRegions = new();

		private static readonly Dictionary<Point16, Ref<Rectangle>> RuntimeRegionsByPoint = new();
		public static readonly List<Ref<Rectangle>> RuntimeProtectedRegions = new();

		public override void PostDrawTiles()
		{
			if (!StarlightRiver.debugMode)
				return;

			Main.spriteBatch.Begin(default, default, SamplerState.PointWrap, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);

			foreach (Rectangle rect in ProtectedRegions)
			{
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;
				var target = new Rectangle(rect.X * 16 - (int)Main.screenPosition.X, rect.Y * 16 - (int)Main.screenPosition.Y, rect.Width * 16, rect.Height * 16);
				Main.spriteBatch.Draw(tex, target, Color.Red * 0.25f);
			}

			foreach (Ref<Rectangle> rectRef in RuntimeProtectedRegions)
			{
				Rectangle rect = rectRef.Value;
				Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/MagicPixel").Value;
				var target = new Rectangle(rect.X * 16 - (int)Main.screenPosition.X, rect.Y * 16 - (int)Main.screenPosition.Y, rect.Width * 16, rect.Height * 16);
				Main.spriteBatch.Draw(tex, target, Color.Blue * 0.25f);
			}

			Main.spriteBatch.End();
		}

		public override void PreWorldGen()
		{
			ProtectedRegions.Clear();
			RuntimeProtectedRegions.Clear();
			RuntimeRegionsByPoint.Clear();
		}

		public override void LoadWorldData(TagCompound tag)
		{
			ProtectedRegions.Clear();
			RuntimeProtectedRegions.Clear();
			RuntimeRegionsByPoint.Clear();

			int length = tag.GetInt("RegionCount");

			for (int k = 0; k < length; k++)
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

		public override void SaveWorldData(TagCompound tag)
		{
			tag["RegionCount"] = ProtectedRegions.Count;

			for (int k = 0; k < ProtectedRegions.Count; k++)
			{
				Rectangle region = ProtectedRegions[k];
				tag.Add("x" + k, region.X);
				tag.Add("y" + k, region.Y);
				tag.Add("w" + k, region.Width);
				tag.Add("h" + k, region.Height);
			}
		}

		public override void NetSend(BinaryWriter writer)
		{
			writer.Write(ProtectedRegions.Count);

			for (int i = 0; i < ProtectedRegions.Count; i++)
			{
				Rectangle region = ProtectedRegions[i];
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

		public static void AddRegionBySource(Point16 source, Rectangle region)
		{
			if (!RuntimeRegionsByPoint.ContainsKey(source))
			{
				var refRect = new Ref<Rectangle>(region);
				RuntimeRegionsByPoint.Add(source, refRect);
				RuntimeProtectedRegions.Add(refRect);
			}
		}

		public static void RemoveRegionBySource(Point16 source)
		{
			if (RuntimeRegionsByPoint.TryGetValue(source, out Ref<Rectangle> refRect))
			{
				RuntimeProtectedRegions.Remove(refRect);
				RuntimeRegionsByPoint.Remove(source);
			}
		}
	}
}