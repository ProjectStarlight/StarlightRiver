using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Items.Dungeon;
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

		public override int DummyType => DummySystem.DummyType<TwistSwordDummy>();

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustID.Iron, SoundID.Tink, Color.Blue, 16, false, false, "Twisted Greatsword", default, new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0));
			RegisterItemDrop(ModContent.ItemType<TwistSword>());
		}
	}

	public class TwistSwordDummy : Dummy
	{

		public float swaySpeed = 0.01f;

		public float sway;

		public float springiness = 0.1f;

		public float rotation;

		public TwistSwordDummy() : base(ModContent.TileType<TwistSwordTile>(), 32, 72) { }

		public override void SafeSetDefaults()
		{
			width = 32;
			height = 72;
			rotation = 1.57f;
		}

		public override int ParentX => (int)(position.X / 16);

		public override int ParentY => (int)(position.Y / 16);

		public override void PostDraw(Color lightColor)
		{
			string texturePath = AssetDirectory.DungeonTile + "TwistSwordTile";

			Texture2D tex = ModContent.Request<Texture2D>(texturePath).Value;
			Texture2D glowTex = ModContent.Request<Texture2D>(texturePath + "_Glow").Value;
			Main.spriteBatch.Draw(tex, position - Main.screenPosition, null, lightColor, rotation - 1.57f, new Vector2(tex.Width / 2, 0), 1, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(glowTex, position - Main.screenPosition, null, Color.White, rotation - 1.57f, new Vector2(tex.Width / 2, 0), 1, SpriteEffects.None, 0f);
		}

		public override void Update()
		{
			Tile tile = Parent;

			sway += swaySpeed;
			Vector2 endPoint = position + rotation.ToRotationVector2() * height;

			for (int k = 0; k < 4; k++)
			{
				float lightLerper = k / 4f;
				Lighting.AddLight(Vector2.Lerp(position, endPoint, lightLerper), Color.Blue.ToVector3() * 0.2f);
			}

			if (Main.rand.NextBool(40))
			{
				Vector2 dustPos = position;
				dustPos.X += Main.rand.Next(width);
				dustPos.Y += Main.rand.Next(height);
				int dustType = Main.rand.NextBool() ? ModContent.DustType<CrystalSparkle>() : ModContent.DustType<CrystalSparkle2>();
				Dust.NewDustPerfect(dustPos, dustType, Vector2.Zero);
			}

			Player collider = Main.player.Where(n => n.active && !n.dead && Terraria.Collision.CheckAABBvLineCollision(n.position, n.Hitbox.Size(), Center, endPoint)).FirstOrDefault();

			if (collider != default)
			{
				rotation -= collider.velocity.X * (collider.wet ? 0.5f : 1) * 0.019f * (1.57f - Math.Abs(rotation - 1.57f));
				rotation = MathHelper.Clamp(rotation, 0f, 3.14f);
			}

			rotation = MathHelper.Lerp(rotation, 1.57f + 0.15f * (float)Math.Sin(sway), springiness);
			velocity = Vector2.Zero;
		}
	}
}