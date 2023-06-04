using StarlightRiver.Content.Biomes;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems;
using Terraria.DataStructures;

namespace StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle
{
	class DynamicGear : GearTile
	{
		public override int DummyType => ModContent.ProjectileType<DynamicGearDummy>();

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<GearTilePlacer>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override bool RightClick(int i, int j)
		{
			var dummy = Dummy(i, j).ModProjectile as GearTileDummy;

			var entity = TileEntity.ByPosition[new Point16(i, j)] as GearTileEntity;

			if (entity is null)
				return false;

			if (dummy is null || dummy.gearAnimation > 0)
				return false;

			entity.Disengage();

			dummy.oldSize = dummy.Size;
			dummy.Size++;
			dummy.gearAnimation = 40;

			GearPuzzleHandler.engagedObjectives = 0;

			GearPuzzleHandler.PuzzleOriginEntity?.Engage(2);

			return true;
		}
	}

	class DynamicGearDummy : GearTileDummy, IHintable
	{
		public DynamicGearDummy() : base(ModContent.TileType<DynamicGear>()) { }

		public override void Update()
		{
			base.Update();

			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Lighting.AddLight(Projectile.Center, new Vector3(0.1f, 0.2f, 0.3f) * Size);

			if (Size == 0)
				Lighting.AddLight(Projectile.Center, new Vector3(0.65f, 0.4f, 0.1f));
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D pegTex = ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "GearPeg").Value;
			Main.spriteBatch.Draw(pegTex, Projectile.Center - Main.screenPosition, null, lightColor, 0, pegTex.Size() / 2, 1, 0, 0);

			if (!Main.LocalPlayer.InModBiome<VitricTempleBiome>())
				return;

			Texture2D tex = Size switch
			{
				0 => ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value,
				1 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value,
				2 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearMid").Value,
				3 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearLarge").Value,
				_ => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value,
			};

			if (gearAnimation > 0) //switching between sizes animation
			{
				Texture2D texOld = oldSize switch
				{
					0 => ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value,
					1 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value,
					2 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearMid").Value,
					3 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearLarge").Value,
					_ => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "MagicalGearSmall").Value,
				};

				if (gearAnimation > 20)
				{
					float progress = Helpers.Helper.BezierEase((gearAnimation - 20) / 20f);
					Main.spriteBatch.Draw(texOld, Projectile.Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, texOld.Size() / 2, progress, 0, 0);
				}
				else
				{
					float progress = Helpers.Helper.SwoopEase(1 - gearAnimation / 20f);
					Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * 0.75f * progress, 0, tex.Size() / 2, progress, 0, 0);
				}

				return;
			}

			Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White * 0.75f, Rotation, tex.Size() / 2, 1, 0, 0);

			if (GearPuzzleHandler.Solved) //draws the crystal gear once the puzzle is finished
			{
				tex = Size switch
				{
					0 => ModContent.Request<Texture2D>(AssetDirectory.Invisible).Value,
					1 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CrystalGearSmall").Value,
					2 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CrystalGearMid").Value,
					3 => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CrystalGearLarge").Value,
					_ => ModContent.Request<Texture2D>(AssetDirectory.VitricTile + "CrystalGearSmall").Value,
				};
				Effect effect = Terraria.Graphics.Effects.Filters.Scene["MoltenForm"].GetShader().Shader;
				effect.Parameters["sampleTexture2"].SetValue(ModContent.Request<Texture2D>("StarlightRiver/Assets/Bosses/VitricBoss/ShieldMap").Value);
				effect.Parameters["uTime"].SetValue(GearPuzzleHandler.solveTimer / 180f * 2);
				effect.Parameters["sourceFrame"].SetValue(new Vector4(0, 0, tex.Width, tex.Height));
				effect.Parameters["texSize"].SetValue(tex.Size());

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, default, default, default, effect, Main.GameViewMatrix.TransformationMatrix);

				Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, null, Color.White, Rotation, tex.Size() / 2, 1, 0, 0);

				Main.spriteBatch.End();
				Main.spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.TransformationMatrix);
			}
		}
		public string GetHint()
		{
			return "A magical gear that can change its shape...";
		}
	}

	[SLRDebug]
	class GearTilePlacer : QuickTileItem
	{
		public GearTilePlacer() : base("Gear puzzle", "Debug Item", "DynamicGear", 8, AssetDirectory.VitricTile) { }
	}
}