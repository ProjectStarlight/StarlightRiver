using StarlightRiver.Content;
using StarlightRiver.Content.Bosses.SquidBoss;
using StarlightRiver.Content.Tiles.Permafrost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ID;

namespace StarlightRiver.Core.Systems.NoBuildingSystem;

public class NoBuildGlobalItem : GlobalItem
{
	public static List<int> blacklist = [ItemID.WaterBucket,
		ItemID.LavaBucket,
		ItemID.HoneyBucket,
		ItemID.BottomlessBucket,
		ItemID.Wrench,
		ItemID.BlueWrench,
		ItemID.GreenWrench,
		ItemID.YellowWrench,
		ItemID.MulticolorWrench,
		ItemID.ActuationRod,
		ItemID.Actuator,
		ItemID.WireKite,
		ItemID.WireCutter,
		ItemID.WireBulb,
		ItemID.Paintbrush,
		ItemID.PaintRoller,
		ItemID.PaintScraper,
		ItemID.SpectrePaintbrush,
		ItemID.SpectrePaintRoller,
		ItemID.SpectrePaintScraper
	];

	public static List<int> whitelist = [ItemID.AcornAxe];

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

	private bool DontPoundTile(On_WorldGen.orig_PoundTile orig, int x, int y)
	{
		if (NoBuildSystem.IsTileProtected(x, y))
		{
			FailFX(new Point16(x, y));
			return false;
		}

		return orig(x, y);
	}

	private bool DontPlaceWire(On_WorldGen.orig_PlaceWire orig, int x, int y)
	{
		if (NoBuildSystem.IsTileProtected(x, y))
		{
			FailFX(new Point16(x, y));
			return false;
		}

		return orig(x, y);
	}

	private bool DontPlaceWire2(On_WorldGen.orig_PlaceWire2 orig, int x, int y)
	{
		if (NoBuildSystem.IsTileProtected(x, y))
		{
			FailFX(new Point16(x, y));
			return false;
		}

		return orig(x, y);
	}

	private bool DontPlaceWire3(On_WorldGen.orig_PlaceWire3 orig, int x, int y)
	{
		if (NoBuildSystem.IsTileProtected(x, y))
		{
			FailFX(new Point16(x, y));
			return false;
		}

		return orig(x, y);
	}

	private bool DontPlaceWire4(On_WorldGen.orig_PlaceWire4 orig, int x, int y)
	{
		if (NoBuildSystem.IsTileProtected(x, y))
		{
			FailFX(new Point16(x, y));
			return false;
		}

		return orig(x, y);
	}

	private bool DontPlaceActuator(On_WorldGen.orig_PlaceActuator orig, int x, int y)
	{
		if (NoBuildSystem.IsTileProtected(x, y))
		{
			FailFX(new Point16(x, y));
			return false;
		}

		return orig(x, y);
	}

	private void DontPickWallInZone(On_Player.orig_PickWall orig, Player self, int x, int y, int damage)
	{
		if (NoBuildSystem.IsTileProtected(x, y))
		{
			FailFX(new Point16(x, y));
			return;
		}

		orig(self, x, y, damage);
	}

	private void DontPickInZone(On_Player.orig_PickTile orig, Player self, int x, int y, int pickPower)
	{
		if (NoBuildSystem.IsTileProtected(x, y))
		{
			FailFX(new Point16(x, y));
			return;
		}

		orig(self, x, y, pickPower);
	}

	private bool DontManuallyPlaceInZone(On_WorldGen.orig_PlaceTile orig, int i, int j, int type, bool mute, bool forced, int plr, int style)
	{
		if (NoBuildSystem.IsTileProtected(i, j))
		{
			FailFX(new Point16(i, j));
			return false;
		}

		return orig(i, j, type, mute, forced, plr, style);
	}

	private void DontExplodeAtRuntime(On_WorldGen.orig_KillTile orig, int i, int j, bool fail, bool effectOnly, bool noItem)
	{
		if (NoBuildSystem.IsTileProtected(i, j) && !WorldGen.generatingWorld)
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
		if (NoBuildSystem.IsTileProtected((int)result.X, (int)result.Y))
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

		if (whitelist.Contains(Item.type))
			return base.CanUseItem(Item, player);

		if (Item.createTile != -1 || Item.createWall != -1 || blacklist.Contains(Item.type))
		{
			Point16 targetPoint = Main.SmartCursorIsUsed ? new Point16(Main.SmartCursorX, Main.SmartCursorY) : new Point16(Player.tileTargetX, Player.tileTargetY);

			if (NoBuildSystem.IsTileProtected(targetPoint))
			{
				//player.AddBuff(BuffID.Cursed, 10, false);
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
			Dust.NewDust(pos.ToVector2() * 16, 16, 16, ModContent.DustType<Content.Dusts.Glow>(), 0, 0, 0, Color.Red, 0.2f);
	}
}