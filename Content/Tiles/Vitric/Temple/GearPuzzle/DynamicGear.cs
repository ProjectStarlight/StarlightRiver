using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Packets;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	class DynamicGear : GearTile
	{
		public override int DummyType => DummySystem.DummyType<DynamicGearDummy>();
	}

	class DynamicGearDummy : GearTileDummy
	{
		public DynamicGearDummy() : base(ModContent.TileType<DynamicGear>()) { }

		public override Rectangle? GetClickbox()
		{
			Rectangle box = Hitbox;
			int mag = 16 + GearSize * 8;
			box.Inflate(mag, mag);
			return box;
		}

		public override void RightClick(int i, int j)
		{
			DynamicGearDummy dummy = this;
			var entity = TileEntity.ByPosition[new Point16((int)dummy.Center.X / 16, (int)dummy.Center.Y / 16)] as GearTileEntity;

			if (entity is null)
				return;

			if (dummy is null || dummy.gearAnimation > 0)
				return;

			GearPuzzleClickPacket gearPacket = new GearPuzzleClickPacket((int)dummy.Center.X / 16, (int)dummy.Center.Y / 16, type);
			gearPacket.Send();

			return;
		}

		public override void RightClickHover(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<GearTilePlacer>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override void Update()
		{
			base.Update();

			if (GearEntity is null)
				return;

			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Lighting.AddLight(Center, new Vector3(0.1f, 0.2f, 0.3f) * GearSize);

			if (GearSize == 0)
				Lighting.AddLight(Center, new Vector3(0.65f, 0.4f, 0.1f));
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D pegTex = Assets.Tiles.Vitric.GearPeg.Value;
			Main.spriteBatch.Draw(pegTex, Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);

			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Texture2D tex = GearSize switch
			{
				0 => Assets.Invisible.Value,
				1 => Assets.Tiles.Vitric.MagicalGearSmall.Value,
				2 => Assets.Tiles.Vitric.MagicalGearMid.Value,
				3 => Assets.Tiles.Vitric.MagicalGearLarge.Value,
				_ => Assets.Tiles.Vitric.MagicalGearSmall.Value,
			};

			if (gearAnimation > 0) //switching between sizes animation
			{
				Texture2D texOld = oldSize switch
				{
					0 => Assets.Invisible.Value,
					1 => Assets.Tiles.Vitric.MagicalGearSmall.Value,
					2 => Assets.Tiles.Vitric.MagicalGearMid.Value,
					3 => Assets.Tiles.Vitric.MagicalGearLarge.Value,
					_ => Assets.Tiles.Vitric.MagicalGearSmall.Value,
				};

				if (gearAnimation > 20)
				{
					float progress = Helpers.Helper.BezierEase((gearAnimation - 20) / 20f);
					Main.spriteBatch.Draw(texOld, Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, texOld.Size() / 2, progress, 0, 0);
				}
				else
				{
					float progress = Helpers.Helper.SwoopEase(1 - gearAnimation / 20f);
					Main.spriteBatch.Draw(tex, Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, tex.Size() / 2, progress, 0, 0);
				}

				return;
			}

			Main.spriteBatch.Draw(tex, Center - Main.screenPosition, null, Color.White * 0.75f, Rotation, tex.Size() / 2, 1, 0, 0);

			if (GearPuzzleHandler.Solved) //draws the crystal gear once the puzzle is finished
			{
				tex = GearSize switch
				{
					0 => Assets.Invisible.Value,
					1 => Assets.Tiles.Vitric.CrystalGearSmall.Value,
					2 => Assets.Tiles.Vitric.CrystalGearMid.Value,
					3 => Assets.Tiles.Vitric.CrystalGearLarge.Value,
					_ => Assets.Tiles.Vitric.CrystalGearSmall.Value,
				};
				Effect effect = Terraria.Graphics.Effects.Filters.Scene["MoltenForm"].GetShader().Shader;
				effect.Parameters["sampleTexture2"].SetValue(Assets.Bosses.VitricBoss.ShieldMap.Value);
				effect.Parameters["uTime"].SetValue(GearPuzzleHandler.solveTimer / 180f * 2);
				effect.Parameters["sourceFrame"].SetValue(new Vector4(0, 0, tex.Width, tex.Height));
				effect.Parameters["texSize"].SetValue(tex.Size());

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, Main.DefaultSamplerState, default, RasterizerState.CullNone, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(tex, Center - Main.screenPosition, null, Color.White, Rotation, tex.Size() / 2, 1, 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, Main.DefaultSamplerState, default, RasterizerState.CullNone, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}
	}

	[SLRDebug]
	class GearTilePlacer : QuickTileItem
	{
		public GearTilePlacer() : base("Gear puzzle", "{{Debug}} Item", "DynamicGear", 8, AssetDirectory.VitricTile) { }
	}
}