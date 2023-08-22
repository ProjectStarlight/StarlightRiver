﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.SpaceEvent;
using StarlightRiver.Core.Systems.CameraSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.CrashTech
{
	class CrashPod : DummyTile, IHintable
	{
		public override string Texture => "StarlightRiver/Assets/Tiles/CrashTech/CrashPod";

		public override int DummyType => DummySystem.DummyType<CrashPodDummy>();

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 2, 4, DustID.Lava, SoundID.Shatter, false, new Color(255, 200, 40), false, false, "Crashed Pod", new AnchorData(AnchorType.SolidWithTop | AnchorType.SolidTile, 2, 0));
			MinPick = int.MaxValue;
			Main.tileLighted[Type] = true;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<Items.Hovers.WindsHover>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 1;
			g = 0.5f;
			b = 0.2f;
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
			{
				Dummy dummy = Dummy(i, j);

				if (dummy is null)
					return;

				Texture2D tex = Request<Texture2D>(Texture + "_Glow").Value;
				Texture2D tex2 = Request<Texture2D>(Texture + "_Glow2").Value;

				spriteBatch.Draw(tex, (Helper.TileAdj + new Vector2(i, j)) * 16 - Main.screenPosition, null, Color.White);
				spriteBatch.Draw(tex2, (Helper.TileAdj + new Vector2(i, j)) * 16 + new Vector2(-1, 0) - Main.screenPosition, null, Helper.IndicatorColorProximity(150, 300, dummy.Center));

			}
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ModContent.ItemType<Astroscrap>(), Main.rand.Next(10, 20));
		}

		public override bool CanKillTile(int i, int j, ref bool blockDamaged)
		{
			return !WorldGen.generatingWorld;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public string GetHint()
		{
			return "A fallen droppod, made of metal rich in binding Starlight. You'd have to use a Starlight power of equal strength...";
		}
	}

	internal class CrashPodDummy : Dummy
	{
		public override void OnLoad(Mod mod)
		{
			for (int k = 1; k < 6; k++)
				GoreLoader.AddGoreFromTexture<SimpleModGore>(mod, "StarlightRiver/Assets/Tiles/CrashTech/CrashPodGore" + k);
		}

		public CrashPodDummy() : base(TileType<CrashPod>(), 32, 48) { }

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Hitbox))
			{
				WorldGen.KillTile(ParentX, ParentY);
				NetMessage.SendTileSquare(Player.whoAmI, (int)(position.X / 16f), (int)(position.Y / 16f), 2, 3, TileChangeType.None);

				CameraSystem.shake += 4;

				for (int k = 1; k <= 5; k++)
					Gore.NewGoreDirect(GetSource_Death(), position + new Vector2(Main.rand.Next(width), Main.rand.Next(height)), (Vector2.Normalize(Player.velocity) * Main.rand.NextFloat(4)).RotatedByRandom(MathHelper.ToRadians(35f)), StarlightRiver.Instance.Find<ModGore>("CrashPodGore" + k).Type);

				for (int i = 0; i < 17; i++)
				{
					//for some reason the BuzzSpark dust spawns super offset 
					Dust.NewDustPerfect(Center + new Vector2(0f, 28f) + Main.rand.NextVector2Circular(15, 25), DustType<Dusts.BuzzSpark>(), (Vector2.Normalize(Player.velocity) * Main.rand.NextFloat(9.5f)).RotatedByRandom(MathHelper.ToRadians(5f)), 0, new Color(255, 255, 60) * 0.8f, 1.15f);

					Dust.NewDustPerfect(Center + Main.rand.NextVector2Circular(15, 25), DustType<Dusts.Glow>(), (Vector2.Normalize(Player.velocity) * Main.rand.NextFloat(9)).RotatedByRandom(MathHelper.ToRadians(15f)), 0, new Color(150, 80, 40), Main.rand.NextFloat(0.25f, 0.5f));
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);
			}
		}
	}

	public class CrashPodGTile : GlobalTile
	{
		public override bool CanExplode(int i, int j, int type)
		{
			if (Main.tile[i, j - 1].TileType == TileType<CrashPod>())
				return false;

			return base.CanExplode(i, j, type);
		}

		public override bool CanKillTile(int i, int j, int type, ref bool blockDamaged)
		{
			if (Main.tile[i, j - 1].TileType == TileType<CrashPod>())
				return !WorldGen.generatingWorld;

			return base.CanKillTile(i, j, type, ref blockDamaged);
		}

		public override void RandomUpdate(int i, int j, int type)
		{
			if (j < Main.worldSurface * 0.35f && Main.rand.NextBool(10000))
			{
				Tile tile1 = Main.tile[i, j];
				Tile tile2 = Main.tile[1, j];

				if (tile1.HasTile && tile2.HasTile && Main.tileSolid[tile1.TileType] && Main.tileSolid[tile2.TileType])
				{
					if (tile1.BlockType == BlockType.Solid && tile2.BlockType == BlockType.Solid && Helper.CheckAirRectangle(new Point16(i, j - 4), new Point16(2, 4)))
					{
						Helper.PlaceMultitile(new Point16(i, j - 4), TileType<CrashPod>());
					}
				}
			}
		}
	}
}