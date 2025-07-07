using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Map;
using Terraria.ModLoader.Default;
using Terraria.ModLoader.IO;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.BaseTypes
{
	public class ProximityActivatedPylonSystem : ModSystem
	{
		public static HashSet<string> activePylons = [];

		public override void SaveWorldData(TagCompound tag)
		{
			tag["activePylons"] = activePylons.ToList();
		}

		public override void LoadWorldData(TagCompound tag)
		{
			activePylons = tag.GetList<string>("activePylons").ToHashSet();
		}
	}

	public abstract class ProximityActivatedPylon : ModPylon
	{
		public const int CrystalVerticalFrameCount = 8;

		public abstract string ID { get; }
		public abstract Asset<Texture2D> MapIcon { get; }
		public abstract Asset<Texture2D> CrystalTexture { get; }
		public abstract Asset<Texture2D> CrystalHighlightTexture { get; }
		public abstract Color PrimaryColor { get; }
		public abstract Color SecondaryColor { get; }

		public bool Active => ProximityActivatedPylonSystem.activePylons.Contains(ID);

		public override void SetStaticDefaults()
		{
			Main.tileLighted[Type] = true;
			Main.tileFrameImportant[Type] = true;

			TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
			TileObjectData.newTile.LavaDeath = false;
			TileObjectData.newTile.DrawYOffset = 2;
			TileObjectData.newTile.StyleHorizontal = true;

			TEModdedPylon moddedPylon = ModContent.GetInstance<ProximityActivatedPylonEntity>();
			TileObjectData.newTile.HookCheckIfCanPlace = new PlacementHook(moddedPylon.PlacementPreviewHook_CheckIfCanPlace, 1, 0, true);
			TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(moddedPylon.Hook_AfterPlacement, -1, 0, false);

			TileObjectData.addTile(Type);

			TileID.Sets.PreventsSandfall[Type] = true;

			MinPick = int.MaxValue;

			AddToArray(ref TileID.Sets.CountsAsPylon);

			LocalizedText pylonName = CreateMapEntryName();
			AddMapEntry(Color.White, pylonName);
		}

		public override NPCShop.Entry GetNPCShopEntry()
		{
			return null;
		}

		public override bool ValidTeleportCheck_NPCCount(TeleportPylonInfo pylonInfo, int defaultNecessaryNPCCount)
		{
			return Active;
		}

		public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
		{
			return Active;
		}

		public override void ValidTeleportCheck_DestinationPostCheck(TeleportPylonInfo destinationPylonInfo, ref bool destinationPylonValid, ref string errorKey)
		{
			destinationPylonValid = Active;
			errorKey = "This pylon is dormant...";
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			if (Active)
			{
				r = PrimaryColor.R / 255f;
				g = PrimaryColor.G / 255f;
				b = PrimaryColor.B / 255f;
			}
		}

		public override void SpecialDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (Active)
			{
				DefaultDrawPylonCrystal(spriteBatch, i, j, CrystalTexture, CrystalHighlightTexture, new Vector2(0f, -12f), Color.White * 0.1f, PrimaryColor, 4, CrystalVerticalFrameCount);
			}
		}

		public override void DrawMapIcon(ref MapOverlayDrawContext context, ref string mouseOverText, TeleportPylonInfo pylonInfo, bool isNearPylon, Color drawColor, float deselectedScale, float selectedScale)
		{
			if (Active)
			{
				bool mouseOver = DefaultDrawMapIcon(ref context, MapIcon, pylonInfo.PositionInTiles.ToVector2() + new Vector2(1.5f, 2f), drawColor, deselectedScale, selectedScale);
				DefaultMapClickHandle(mouseOver, pylonInfo, $"Mods.StarlightRiver.Pylons.{ID}", ref mouseOverText);
			}
		}

		public sealed override void NearbyEffects(int i, int j, bool closer)
		{
			Tile tile = Main.tile[i, j];

			if (tile.TileFrameX == 0 && tile.TileFrameY == 0)
			{
				int type = DummySystem.DummyType<ProximityActivatedPylonDummy>();
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

		public void Activate()
		{
			ProximityActivatedPylonSystem.activePylons.Add(ID);
		}
	}

	public class ProximityActivatedPylonDummy : Dummy
	{
		public int timer;
		public bool activating;

		public ProximityActivatedPylon ParentPrototype => ModContent.GetModTile(Parent.type) as ProximityActivatedPylon;

		public ProximityActivatedPylonDummy() : base(-1, 48, 48) { }

		public override bool ValidTile(Tile tile)
		{
			return tile.HasTile && ModContent.GetModTile(tile.type) is ProximityActivatedPylon;
		}

		public override void DrawBehindTiles()
		{
			if (ParentPrototype?.Active ?? true)
				return;

			Texture2D tex = Assets.Tiles.Pylons.Pylon_CrystalDead.Value;
			var frame = new Rectangle(0, 0, tex.Width, tex.Height / 8);
			Vector2 pos = Center + new Vector2(46, 32) - Main.screenPosition;
			float rot = 1.2f;

			if (activating)
			{
				Vector2 targetPos = Center - Main.screenPosition;
				float targetRot = 0f;

				frame.Y = tex.Height / 8 * (int)(timer / 10f % 8);

				if (timer < 60)
				{
					float posProg = Eases.EaseQuadInOut(timer / 60f);
					pos = Vector2.Lerp(pos, targetPos, posProg);
					pos.Y -= MathF.Sin(posProg * 3.14f) * 32;
				}

				if (timer > 10 && timer < 90)
				{
					float rotProg = Eases.SwoopEase((timer - 10) / 80f);
					rot = float.Lerp(rot, targetRot, rotProg);
				}

				if (timer > 60)
				{
					pos = targetPos;
					pos.Y += MathF.Sin((timer - 60) / 30f * 3.14f) * 4;
				}

				if (timer > 90)
				{
					rot = targetRot;
				}
			}

			Main.spriteBatch.Draw(tex, pos, frame, new Color(Lighting.GetSubLight(pos + Main.screenPosition)), rot, frame.Size() / 2f, 1, 0, 0);
		}

		public override void Update()
		{
			if (ParentPrototype != null && !ParentPrototype.Active)
			{
				if (Main.player.Any(n => n.active && !n.dead && Vector2.Distance(n.Center, Center) < 32))
					activating = true;
			}

			if (activating && ParentPrototype != null)
			{
				timer++;

				if (timer == 1)
				{
					SoundHelper.PlayPitched("World/StoneOpen", 1f, 0.5f, Center);
				}

				if (timer < 90)
				{
					Vector2 pos = Center + new Vector2(46, 32);
					Vector2 targetPos = Center;

					float posProg = Eases.EaseQuadInOut(timer / 60f);
					pos = Vector2.Lerp(pos, targetPos, posProg);
					pos.Y -= MathF.Sin(posProg * 3.14f) * 32;

					if (timer > 60)
					{
						pos = targetPos;
						pos.Y += MathF.Sin((timer - 60) / 30f * 3.14f) * 4;
					}

					var color = Color.Lerp(ParentPrototype.PrimaryColor, ParentPrototype.SecondaryColor, Main.rand.NextFloat());
					color.A = 0;

					Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, color, Main.rand.NextFloat(0.2f, 0.4f));
				}

				if (timer > 40 && timer < 110)
				{
					float scale = 1f - (timer - 40) / 80f;

					var color = Color.Lerp(ParentPrototype.PrimaryColor, ParentPrototype.SecondaryColor, Main.rand.NextFloat());
					color.A = 0;

					Vector2 off = Main.rand.NextVector2Circular(1, 1);
					Dust.NewDustPerfect(Center + off * 128 * scale, ModContent.DustType<Dusts.PixelatedGlow>(), off * -Main.rand.NextFloat(6), 0, color, Main.rand.NextFloat(0.2f, 0.4f) * scale);

					off = Main.rand.NextVector2Circular(1, 1);
					Dust.NewDustPerfect(Center + off * 128 * scale, ModContent.DustType<Dusts.PixelatedImpactLineDust>(), off * -Main.rand.NextFloat(6), 0, color, Main.rand.NextFloat(0.1f, 0.2f) * scale);
				}

				if (timer < 120)
					Lighting.AddLight(Center, ParentPrototype.PrimaryColor.ToVector3() * Math.Min(1, timer / 60f));

				if (timer == 120)
				{
					ParentPrototype.Activate();

					SoundHelper.PlayPitched("Magic/HolyCastShort", 1f, 0.5f, Center);
					SoundHelper.PlayPitched("Magic/Shadow1", 1f, 0.0f, Center);
					SoundHelper.PlayPitched("Magic/Shadow2", 1f, 0.25f, Center);

					for (int k = 0; k < 20; k++)
					{
						var color = Color.Lerp(ParentPrototype.PrimaryColor, ParentPrototype.SecondaryColor, Main.rand.NextFloat());
						color.A = 0;

						Vector2 off = Main.rand.NextVector2Circular(1, 1);
						Dust.NewDustPerfect(Center + off * 16, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), off * Main.rand.NextFloat(15), 0, color, Main.rand.NextFloat(0.1f, 0.2f));
					}

					for (int k = 0; k < 4; k++)
					{
						Color color = ParentPrototype.PrimaryColor;
						color.A = 0;

						Vector2 off = Vector2.UnitX.RotatedBy(k / 4f * 6.28f);
						Dust.NewDustPerfect(Center + off * 16, ModContent.DustType<Dusts.PixelatedImpactLineDustGlow>(), off * 25, 0, color, Main.rand.NextFloat(0.15f, 0.25f));
					}
				}
			}
		}
	}

	public class ProximityActivatedPylonEntity : TEModdedPylon { }
}
