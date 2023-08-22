﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.Interactive
{
	internal class StaminaOrb : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<StaminaOrbDummy>();

		public override string Texture => AssetDirectory.InteractiveTile + Name;

		public override void SetStaticDefaults()
		{
			Main.tileLavaDeath[Type] = false;
			Main.tileFrameImportant[Type] = true;
			Main.tileBlockLight[Type] = false;
			Main.tileLighted[Type] = true;

			RegisterItemDrop(ItemType<StaminaOrbItem>());
			DustType = DustType<Dusts.Stamina>();
			AddMapEntry(new Color(255, 186, 66));
		}

		public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
		{
			r = 0.236f / 1.1f;
			g = 0.144f / 1.1f;
			b = 0.071f / 1.1f;
		}
	}

	internal class StaminaOrbDummy : Dummy
	{
		public float timer;

		public StaminaOrbDummy() : base(TileType<StaminaOrb>(), 16, 16) { }

		public override void Update()
		{
			if (timer > 0)
			{
				timer--;
			}
			else
			{
				float rot = Main.rand.NextFloat(0, 6.28f);
				Dust.NewDustPerfect(Center, DustType<Dusts.Stamina>(), new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) * 0.4f, 0, default, 2f);
			}
		}

		public override void Collision(Player Player)
		{
			AbilityHandler mp = Player.GetHandler();

			mp.Stamina++;
			timer = 300;
			Terraria.Audio.SoundEngine.PlaySound(SoundID.Item112, Center);
			CombatText.NewText(Player.Hitbox, new Color(255, 170, 60), "+1");

			for (float k = 0; k <= 6.28; k += 0.1f)
				Dust.NewDustPerfect(Center, DustType<Dusts.Stamina>(), new Vector2((float)Math.Cos(k), (float)Math.Sin(k)) * (Main.rand.Next(25) * 0.1f), 0, default, 3f);
		}
	}

	public class StaminaOrbItem : QuickTileItem
	{
		public StaminaOrbItem() : base("Starlight Orb", "Pass through this to gain starlight!\n5 second cooldown", "StaminaOrb", 8, AssetDirectory.InteractiveTile) { }
	}
}