using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.Audio;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Content.Tiles.Vitric.Temple.GearPuzzle;
using System.Linq;
using StarlightRiver.Content.Items.Misc;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace StarlightRiver.Content.Tiles.Dungeon
{
    public class TwistSwordTile : DummyTile
	{
		public override string Texture => AssetDirectory.Invisible;

		public override int DummyType => ModContent.ProjectileType<TwistSwordTileProj>();
		public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            TileObjectData.newTile.Height = 1;
            TileObjectData.newTile.Width = 1;
            TileObjectData.newTile.Origin = new Point16(0, 0); // Todo: make less annoying.
            TileObjectData.newTile.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16};
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.addTile(Type);

			HitSound = SoundID.Tink;

			ModTranslation name = CreateMapEntryName();
            name.SetDefault("Twisted Greatsword");
            AddMapEntry(Color.Blue, name);
        }

		public override void KillTile(int i, int j, ref bool fail, ref bool effectOnly, ref bool noItem)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 16, 16, ModContent.ItemType<TwistSword>());
		}
	}

    public class TwistSwordTileProj : ModProjectile
    {
		public override string Texture => AssetDirectory.DungeonTile + "TwistSwordTile";

		public float swaySpeed = 0.01f;

		public float sway;

		public float springiness = 0.02f;

		public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Twisted Sword");
        }

        public override void SetDefaults()
        {
            Projectile.knockBack = 6f;
            Projectile.width = 32;
            Projectile.height = 72;
            Projectile.penetrate = -1;
            Projectile.ignoreWater = true;
			Projectile.tileCollide = false;
			Projectile.rotation = 1.57f;
		}

		public override bool PreDraw(ref Color lightColor)
        {
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Main.spriteBatch.Draw(tex, Projectile.position - Main.screenPosition, null, lightColor, Projectile.rotation - 1.57f, new Vector2(tex.Width / 2, 0), Projectile.scale, SpriteEffects.None, 0f);
			return false;
		}

        public override void AI()
        {
			int i = (int)(Projectile.position.X / 16);
			int j = (int)(Projectile.position.Y / 16);
			Tile tile = Main.tile[i, j];

			if (tile.HasTile && tile.TileType == ModContent.TileType<TwistSwordTile>())
				Projectile.timeLeft = 2;
			else
				Projectile.active = false;

			sway += swaySpeed;
			Vector2 endPoint = Projectile.position + (Projectile.rotation.ToRotationVector2() * Projectile.height);
			var collider = Main.player.Where(n => n.active && !n.dead && Collision.CheckAABBvLineCollision(n.position, n.Hitbox.Size(), Projectile.Center, endPoint)).FirstOrDefault();

			if (collider != default)
			{
				Projectile.rotation -= collider.velocity.X * (collider.wet ? 0.5f : 1) * 0.019f * (1.57f - Math.Abs(Projectile.rotation - 1.57f));
				Projectile.rotation = MathHelper.Clamp(Projectile.rotation, 0f, 3.14f);
			}

			Projectile.rotation = MathHelper.Lerp(Projectile.rotation, 1.57f + (0.15f * (float)Math.Sin(sway)), springiness);
			Projectile.velocity = Vector2.Zero;
        }
    }
}
