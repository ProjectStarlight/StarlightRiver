using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Interactive
{
	internal class StaminaGem : DummyTile
	{
		public override int DummyType => ProjectileType<StaminaGemDummy>();

		public override string Texture => AssetDirectory.InteractiveTile + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.Stamina>(), SoundID.Shatter, false, new Color(255, 186, 66));
		}

		public override void KillMultiTile(int i, int j, int frameX, int frameY)
		{
			Item.NewItem(new EntitySource_TileBreak(i, j), new Vector2(i, j) * 16, ItemType<StaminaGemItem>());
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.236f * 1.1f;
			g = 0.144f * 1.1f;
			b = 0.071f * 1.1f;
		}
	}

	internal class StaminaGemItem : QuickTileItem
	{
		public StaminaGemItem() : base("Starlight Gem", "Restores starlight when hit with an ability", "StaminaGem", 8, AssetDirectory.InteractiveTile) { }
	}

	internal class StaminaGemDummy : Dummy
	{
		public StaminaGemDummy() : base(TileType<StaminaGem>(), 16, 16) { }

		public override void Update()
		{
			if (Projectile.ai[0] > 0)
				Projectile.ai[0]--;
			else if (Main.rand.NextBool(3))
				Dust.NewDust(Projectile.position, 16, 16, DustType<Dusts.Stamina>());

			Lighting.AddLight(Projectile.Center, new Vector3(1, 0.4f, 0.1f) * 0.35f);
		}

		public override void Collision(Player Player)
		{
			AbilityHandler mp = Player.GetHandler();

			if (Projectile.ai[0] == 0 && Projectile.Hitbox.Intersects(Player.Hitbox) && mp.Stamina < mp.StaminaMax && mp.ActiveAbility != null)
			{
				mp.Stamina++;
				Projectile.ai[0] = 300;

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Projectile.Center);
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Item112, Projectile.Center);
				CombatText.NewText(Player.Hitbox, new Color(255, 170, 60), "+1");

				for (float k = 0; k <= 6.28; k += 0.1f)
					Dust.NewDustPerfect(Projectile.Center, DustType<Dusts.Stamina>(), new Vector2((float)Math.Cos(k), (float)Math.Sin(k)) * (Main.rand.Next(50) * 0.1f), 0, default, 3f);
			}
		}

		public override void PostDraw(Color lightColor)
		{
			if (Projectile.ai[0] == 0)
			{
				Color color = Color.White * (float)Math.Sin(StarlightWorld.visualTimer * 3f);
				Main.spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/StaminaGemGlow").Value, Projectile.position - Main.screenPosition, color);
				Main.spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/StaminaGemOn").Value, Projectile.position - Main.screenPosition, Color.White);
			}
		}
	}
}