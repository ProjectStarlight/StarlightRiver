﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Interactive
{
	internal class Bouncer : DummyTile, IHintable
	{
		public override int DummyType => DummySystem.DummyType<BouncerDummy>();

		public override string Texture => AssetDirectory.InteractiveTile + Name;

		public override void SetStaticDefaults()
		{
			QuickBlock.QuickSetFurniture(this, 1, 1, DustType<Dusts.GlassNoGravity>(), SoundID.Shatter, false, new Color(115, 182, 158));
		}

		public string GetHint()
		{
			return "A reactive crystal. It... kinetically interacts with Starlight.";
		}
	}

	internal class BouncerItem : QuickTileItem
	{
		public BouncerItem() : base("Vitric Bouncer", "Dash into this to go flying!\nResets jump accessories", "Bouncer", 8, AssetDirectory.InteractiveTile) { }

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemType<VitricOre>(), 2);
			recipe.AddIngredient(ItemType<StaminaGel>(), 1);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}

	internal class BouncerDummy : Dummy
	{
		public BouncerDummy() : base(TileType<Bouncer>(), 16, 16) { }

		public override void Collision(Player Player)
		{
			AbilityHandler mp = Player.GetHandler();

			if (AbilityHelper.CheckDash(Player, Hitbox))
			{
				mp.ActiveAbility?.Deactivate();

				if (Player.velocity.Length() != 0)
				{
					Player.velocity = Vector2.Normalize(Player.velocity) * -18f;
					Player.wingTime = Player.wingTimeMax;
					Player.rocketTime = Player.rocketTimeMax;
					// TODO: Jump reset once Tmod adds that!
				}

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);

				for (int k = 0; k <= 30; k++)
				{
					int dus = Dust.NewDust(position, 48, 32, DustType<Dusts.GlassAttracted>(), Main.rand.Next(-16, 15), Main.rand.Next(-16, 15), 0, default, 1.3f);
					Main.dust[dus].customData = Center;
				}
			}
		}

		public override void PostDraw(Color lightColor)
		{
			Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Tiles/Interactive/BouncerGlow").Value;
			Color color = Helper.IndicatorColorProximity(150, 300, Center);
			Main.spriteBatch.Draw(tex, position - Vector2.One - Main.screenPosition, color);
		}
	}
}