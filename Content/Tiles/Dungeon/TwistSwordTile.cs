//TODO:
//Sparkles
//Glowmask

using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ObjectData;

namespace StarlightRiver.Content.Tiles.Dungeon
{
	public class TwistSwordTile : DummyTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override int DummyType => ModContent.ProjectileType<TwistSwordTileProj>();

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Iron, SoundID.Tink, Color.Blue, 16, false, false, "Twisted Greatsword", default, new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0));
		}

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<TwistSword>());
		}
	}

	public class TwistSwordTileProj : Dummy
	{

		public float swaySpeed = 0.01f;

		public float sway;

		public float springiness = 0.1f;

		public override string Texture => AssetDirectory.DungeonTile + "TwistSwordTile";

		public TwistSwordTileProj() : base(ModContent.TileType<TwistSwordTile>(), 32, 72) { }

		public override void SafeSetDefaults()
		{
			Projectile.knockBack = 6f;
			Projectile.width = 32;
			Projectile.height = 72;
			Projectile.penetrate = -1;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.rotation = 1.57f;
		}

		public override int ParentX => (int)(Projectile.position.X / 16);

		public override int ParentY => (int)(Projectile.position.Y / 16);

		public override bool PreDraw(ref Color lightColor)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(Texture + "_Glow").Value;
			Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, null, lightColor, Projectile.rotation - 1.57f, new Vector2(tex.Width / 2, 0), Projectile.scale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(glowTex, Projectile.position - Main.screenPosition, null, Color.White, Projectile.rotation - 1.57f, new Vector2(tex.Width / 2, 0), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

		public override void Update()
		{
			Tile tile = Parent;

			if (tile.HasTile && tile.TileType == ModContent.TileType<TwistSwordTile>())
				Projectile.timeLeft = 2;
			else
				Projectile.active = false;

			sway += swaySpeed;
			Vector2 endPoint = Projectile.position + Projectile.rotation.ToRotationVector2() * Projectile.height;

			for (int k = 0; k < 4; k++)
			{
				float lightLerper = k / 4f;
				Lighting.AddLight(Vector2.Lerp(Projectile.position, endPoint, lightLerper), Color.Blue.ToVector3() * 0.2f);
			}

			if (Main.rand.NextBool(40))
			{
				Vector2 dustPos = Projectile.position;
				dustPos.X += Main.rand.Next(Projectile.width);
				dustPos.Y += Main.rand.Next(Projectile.height);
				int dustType = Main.rand.NextBool() ? ModContent.DustType<CrystalSparkle>() : ModContent.DustType<CrystalSparkle2>();
				Dust.NewDustPerfect(dustPos, dustType, Vector2.Zero);
			}

			Player collider = Main.player.Where(n => n.active && !n.dead && Terraria.Collision.CheckAABBvLineCollision(n.position, n.Hitbox.Size(), Projectile.Center, endPoint)).FirstOrDefault();

			if (collider != default)
			{
				Projectile.rotation -= collider.velocity.X * (collider.wet ? 0.5f : 1) * 0.019f * (1.57f - Math.Abs(Projectile.rotation - 1.57f));
				Projectile.rotation = MathHelper.Clamp(Projectile.rotation, 0f, 3.14f);
			}

			Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 1.57f + 0.15f * (float)Math.Sin(sway), springiness);
			Projectile.velocity = Vector2.Zero;
		}
	}
}