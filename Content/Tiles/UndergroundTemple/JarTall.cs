using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class JarTall : DummyTile
	{
		public override int DummyType => ProjectileType<JarDummy>();

		public override string Texture => AssetDirectory.UndergroundTempleTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(2, 4, DustType<Dusts.Stamina>(), SoundID.Shatter, false, new Color(204, 91, 50), false, false, "Stamina Jar");
			MinPick = int.MaxValue;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<Items.Hovers.WindsHover>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(1, 0.5f, 0.2f) * 0.3f);

			if (Main.rand.NextBool(4))
				Dust.NewDustPerfect(new Vector2(i + Main.rand.NextFloat(), j + Main.rand.NextFloat()) * 16, DustType<Dusts.Stamina>(), new Vector2(0, -Main.rand.NextFloat()));
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
			{
				Projectile dummy = Dummy(i, j);

				if (dummy is null)
					return;

				Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/UndergroundTemple/JarTallGlow").Value;
				Texture2D tex2 = Request<Texture2D>("StarlightRiver/Assets/Tiles/UndergroundTemple/JarTallGlow2").Value;

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);

				spriteBatch.Draw(tex, (Helper.TileAdj + new Vector2(i, j)) * 16 - Main.screenPosition, Color.White);
				spriteBatch.Draw(tex2, (Helper.TileAdj + new Vector2(i, j)) * 16 + new Vector2(-2, 0) - Main.screenPosition, Helper.IndicatorColorProximity(150, 300, dummy.Center));

			}
		}

		public override bool CanDrop(int i, int j)
		{
			return false;
		}
	}

	internal class JarDummy : Dummy, IDrawAdditive
	{
		public JarDummy() : base(TileType<JarTall>(), 32, 32) { }

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Projectile.Hitbox))
			{
				WorldGen.KillTile(ParentX, ParentY);
				NetMessage.SendTileSquare(Player.whoAmI, (int)(Projectile.position.X / 16f), (int)(Projectile.position.Y / 16f), 2, 4, TileChangeType.None);

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Keys/Glow").Value;
			spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition + Vector2.UnitY * 16, tex.Frame(), Color.OrangeRed * 0.7f, 0, tex.Size() / 2, 0.8f, 0, 0);
		}
	}

	[SLRDebug]
	public class JarTallItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public JarTallItem() : base("Stamina Jar Placer (Tall)", "Debug Item", "JarTall", -12) { }
	}
}