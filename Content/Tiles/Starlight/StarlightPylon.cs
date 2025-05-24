using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems.CutsceneSystem;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader.Default;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Starlight
{
	public class StarlightPylon : ModPylon
	{
		public const int CrystalVerticalFrameCount = 8;

		public override string Texture => AssetDirectory.StarlightTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;

			TEModdedPylon moddedPylon = ModContent.GetInstance<StarlightPylonEntity>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.PreventsSandfall[Type] = true;
			TileID.Sets.AvoidedByMeteorLanding[Type] = true;

			AddToArray(ref TileID.Sets.CountsAsPylon);

			LocalizedText pylonName = CreateMapEntryName();
			AddMapEntry(Color.White, pylonName);
		}

		public override void MouseOver(int i, int j)
		{
			Main.LocalPlayer.cursorItemIconEnabled = true;
			Main.LocalPlayer.cursorItemIconID = ModContent.ItemType<StarlightPylonItem>();
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			ModContent.GetInstance<StarlightPylonEntity>().Kill(i, j);
		}

		public override NPCShop.Entry GetNPCShopEntry()
		{
			return null;
		}

		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
		{
			return true;
		}

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return true;
		}

		public override void ValidTeleportCheck_DestinationPostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey)
		{
			destinationPylonValid = ObservatorySystem.observatoryOpen;
			errorKey = "This pylon is dormant...";
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (ObservatorySystem.pylonAppearsOn)
			{
				r = 0.2f;
				g = 0.45f;
				b = 1f;
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (ObservatorySystem.pylonAppearsOn)
			{
				DefaultDrawPylonCrystal(spriteBatch, i, j, Assets.Tiles.Starlight.StarlightPylon_Crystal, Assets.Tiles.Starlight.StarlightPylon_CrystalHighlight, new Vector2(0f, -12f), Color.White * 0.1f, new Color(120, 200, 255), 4, CrystalVerticalFrameCount);
			}
		}

		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale)
		{
			if (ObservatorySystem.pylonAppearsOn)
			{
				bool mouseOver = DefaultDrawMapIcon(ref context, Assets.Tiles.Starlight.StarlightPylon_MapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
				DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<StarlightPylonItem>().DisplayName.Key, ref mouseOverText);
			}
			else
			{
				bool mouseOver = DefaultDrawMapIcon(ref context, Assets.Tiles.Starlight.StarlightPylon_MapIconDead, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
				DefaultMapClickHandle(mouseOver, pylonInfo, ModContent.GetInstance<StarlightPylonItem>().DisplayName.Key, ref mouseOverText);
			}
		}

		public override void EmitParticles(int i, int j, Tile tile, short tileFrameX, short tileFrameY, Color tileLight, bool visible)
		{
			base.EmitParticles(i, j, tile, tileFrameX, tileFrameY, tileLight, visible);
		}

		/// <summary>
		/// This is a re-implementation of the Dummy Tile class' behavior because we have some inheritence problems.
		/// Oh well.
		/// </summary>
		/// <param name="i"></param>
		/// <param name="j"></param>
		/// <param name="closer"></param>
		public sealed override void NearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Main.tile[i, j];

			if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			{
				int type = DummySystem.DummyType<StarlightPylonDummy>();
				Dummy dummy = DummyTile.GetDummy(i, j, type);

				if (dummy is null || !dummy.active)
				{
					if (Main.netMode == Terraria.ID.NetmodeID.MultiplayerClient)
					{
						var packet = new SpawnDummy(Main.myPlayer, type, i, j);
						packet.Send(-1, -1, false);
						return;
					}

					Vector2 spawnPos = new Vector2(i, j) * 16 + DummySystem.prototypes[type].Size / 2;
					Dummy newDummy = DummySystem.NewDummy(type, spawnPos);

					var key = new Point16(i, j);
					DummyTile.dummiesByPosition[key] = newDummy;
				}
			}
		}
	}

	public sealed class StarlightPylonEntity : TEModdedPylon { }

	public class StarlightPylonDummy : Dummy
	{
		public StarlightPylonDummy() : base(ModContent.TileType<StarlightPylon>(), 48, 48) { }

		public override void DrawBehindTiles()
		{
			if (ObservatorySystem.pylonAppearsOn)
				return;

			Texture2D tex = Assets.Tiles.Starlight.StarlightPylon_CrystalDead.Value;
			var frame = new Rectangle(0, 0, tex.Width, tex.Height / 8);
			Vector2 pos = Center + new Vector2(46, 32) - Main.screenPosition;
			float rot = 1.2f;

			if (Main.LocalPlayer.InCutscene<StarlightPylonActivateCutscene>())
			{
				int timer = Main.LocalPlayer.GetActiveCutscene().timer;

				Vector2 targetPos = Center - Main.screenPosition;
				float targetRot = 0f;

				if (timer > 120)
				{
					frame.Y = tex.Height / 8 * (int)((timer - 120) / 10f % 8);
				}

				if (timer > 120 && timer < 180)
				{
					float posProg = Eases.EaseQuadInOut((timer - 120) / 60f);
					pos = Vector2.Lerp(pos, targetPos, posProg);
					pos.Y -= MathF.Sin(posProg * 3.14f) * 32;
				}

				if (timer > 130 && timer < 210)
				{
					float rotProg = Eases.SwoopEase((timer - 130) / 80f);
					rot = float.Lerp(rot, targetRot, rotProg);
				}

				if (timer > 180)
				{
					pos = targetPos;
					pos.Y += MathF.Sin((timer - 180) / 30f * 3.14f) * 4;
				}

				if (timer > 210)
				{
					rot = targetRot;
				}
			}

			Main.spriteBatch.Draw(tex, pos, frame, new Color(Lighting.GetSubLight(pos + Main.screenPosition)), rot, frame.Size() / 2f, 1, 0, 0);
		}

		public override void Update()
		{
			if (Main.LocalPlayer.InCutscene<StarlightPylonActivateCutscene>())
			{
				int timer = Main.LocalPlayer.GetActiveCutscene().timer;

				if (timer > 120 && timer < 210)
				{
					Vector2 pos = Center + new Vector2(46, 32);
					Vector2 targetPos = Center;

					float posProg = Eases.EaseQuadInOut((timer - 120) / 60f);
					pos = Vector2.Lerp(pos, targetPos, posProg);
					pos.Y -= MathF.Sin(posProg * 3.14f) * 32;

					if (timer > 180)
					{
						pos = targetPos;
						pos.Y += MathF.Sin((timer - 180) / 30f * 3.14f) * 4;
					}

					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, new Color(Main.rand.Next(200), 200 + Main.rand.Next(55), 255, 0), Main.rand.NextFloat(0.2f, 0.4f));
				}

				if (timer > 160 && timer < 210)
				{
					float scale = 1f - (timer - 160) / 80f;

					Vector2 off = Main.rand.NextVector2Circular(1, 1);
					Dust.NewDustPerfect(Center + off * 128 * scale, ModContent.DustType<Dusts.PixelatedGlow>(), off * -Main.rand.NextFloat(6), 0, new Color(Main.rand.Next(200), 200 + Main.rand.Next(55), 255, 0), Main.rand.NextFloat(0.2f, 0.4f) * scale);

					off = Main.rand.NextVector2Circular(1, 1);
					Dust.NewDustPerfect(Center + off * 128 * scale, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), off * -Main.rand.NextFloat(6), 0, new Color(100, 200 + Main.rand.Next(55), 255, 0), Main.rand.NextFloat(0.1f, 0.2f) * scale);
				}

				if (timer > 60 && timer < 240)
				{
					Lighting.AddLight(Center, new Vector3(0.2f, 0.45f, 1f) * Math.Min(1, (timer - 60) / 60f));
				}
			}
		}
	}

	public sealed class StarlightPylonItem : QuickTileItem
	{
		public StarlightPylonItem() : base("Mysterious Pylon", "You shouldn't have this!", "StarlightPylon", 0, AssetDirectory.Debug, true, 0) { }
	}
}