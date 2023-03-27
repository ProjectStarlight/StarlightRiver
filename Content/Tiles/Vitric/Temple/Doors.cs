using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Vitric.Temple
{
	class DoorVertical : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + Name;

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(1, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ItemType<TempleKey>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override bool RightClick(int i, int j)
		{
			if (Main.LocalPlayer.inventory.ConsumeItems(n => n.type == ItemType<TempleKey>(), 1))
				WorldGen.KillTile(i, j);

			return true;
		}
	}

	class DoorVerticalItem : QuickTileItem
	{
		public DoorVerticalItem() : base("Vertical Temple Door", "Temple Door, But what if it was vertical?", "DoorVertical", ItemRarityID.Blue, AssetDirectory.Debug, true) { }
	}

	class DoorGears : ModTile
	{
		public override string Texture => AssetDirectory.VitricTile + "DoorVertical";

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(1, 7, DustType<Dusts.Air>(), SoundID.Tink, false, new Color(200, 180, 100), false, true);
		}

		public override void NearbyEffects(int i, int j, bool closer)
		{
			Framing.GetTileSafely(i, j).IsActuated = GearPuzzle.GearPuzzleHandler.solved;
		}
	}

	class DoorGearsItem : QuickTileItem
	{
		public DoorGearsItem() : base("Gear Puzzle Temple Door", "Temple Door, Opens if gear puzzle is solved", "DoorGears", ItemRarityID.Blue, AssetDirectory.Debug, true) { }
	}

	class DashableDoor : DummyTile
	{
		public override int DummyType => ProjectileType<DashableDoorDummy>();

		public override string Texture => AssetDirectory.Invisible;

		public override bool PreDraw(int i, int j, SpriteBatch spriteBatch)
		{
			return false;
		}

		public override void PostSpawnDummy(Projectile dummy)
		{
			dummy.position.X -= 8;
		}

		public override void SetStaticDefaults()
		{
			MinPick = int.MaxValue;
			TileID.Sets.DrawsWalls[Type] = true;
			this.QuickSetFurniture(2, 13, DustType<Dusts.GlassGravity>(), SoundID.Tink, false, new Color(100, 200, 255));
			Main.tileSolid[Type] = true;
		}
	}

	class DashableDoorDummy : Dummy
	{
		public DashableDoorDummy() : base(TileType<DashableDoor>(), 16 * 3, 16 * 13) { }

		public override void Collision(Player Player)
		{
			if (Player.Hitbox.Intersects(Projectile.Hitbox))
			{
				if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
				{
					WorldGen.KillTile(ParentX, ParentY);

					Player.GetModPlayer<AbilityHandler>().ActiveAbility?.Deactivate();
					Player.velocity = Vector2.Normalize(Player.velocity) * -10f;
					Player.velocity.Y -= 5;

					CameraSystem.shake += 10;

					Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Player.Center);
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Main.spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricTile + "TutorialDoor2").Value, Projectile.position + Vector2.UnitX * 8 - Main.screenPosition, lightColor);
			Main.spriteBatch.Draw(Request<Texture2D>(AssetDirectory.VitricTile + "TutorialDoor2Glow").Value, Projectile.position + Vector2.UnitX * 8 - Main.screenPosition, Helper.IndicatorColor);
		}
	}

	class DashableDoorItem : QuickTileItem
	{
		public DashableDoorItem() : base("DashableDoor", "Debug Item", "DashableDoor", 1, AssetDirectory.Debug, true) { }
	}
}