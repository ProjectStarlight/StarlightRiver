using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	internal class FaeflamePickup : AbilityPickup
	{
		public float timer;
		public float timerMax = 6.28f * 2;

		public override Asset<Texture2D> Texture => Assets.Abilities.Whip;

		public override Color GlowColor => new(255, 200, 70);

		public FaeflamePickup() : base(TileType<FaeflamePickupTile>()) { }

		public override bool CanPickup(Player Player)
		{
			return !Player.GetHandler().Unlocked<Whip>();
		}

		public Color trailColor(float prog)
		{
			Color noodleColor = new Color(255, 255, 150, 0);
			Color startColor = new Color(255, 50, 75, 0);
			Color endColor = new Color(0, 150, 255, 0);

			if (prog < 0.5)
				return Color.Lerp(startColor, noodleColor, prog / 0.5f);
			if (prog > 0.5)
				return Color.Lerp(noodleColor, endColor, (prog - 0.5f) / 0.5f);

			return noodleColor;
		}

		public override void Visuals()
		{
			timerMax = 6.28f * 2;

			timer += timerMax / 240f;

			if (timer > timerMax)
				timer = 0;

			if (Main.rand.NextBool(10))
			{
				var yOffset = (float)Math.Sin(StarlightWorld.visualTimer) * 5;

				Dust.NewDustPerfect(Center + new Vector2(5, -14 + yOffset), DustType<Dusts.PixelatedEmber>(), Vector2.UnitY.RotatedBy(-1f) * Main.rand.NextFloat(-1, 0), 0, new Color(255, 50, 75, 0), 0.15f);
				Dust.NewDustPerfect(Center + new Vector2(5, 14 + yOffset), DustType<Dusts.PixelatedEmber>(), Vector2.UnitY.RotatedBy(1.77f) * Main.rand.NextFloat(-1, 0), 0, new Color(0, 150, 255, 0), 0.15f);
			}

			float t = timer * 2.5f;
			float y = (timer / timerMax) * 80;
			float w = (float)Math.Sin(y / 80f * 3.14f);
			Dust.NewDustPerfect(Center + new Vector2((float)Math.Cos(t) * (32 * w), -40 + (float)Math.Sin(t) * (8 * w) + y), DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, trailColor(timer / timerMax), 0.15f);

			var timer2 = (timer + 3.14f) % timerMax;
			t = timer2 * 2.5f;
			y = (timer2 / timerMax) * 80;
			w = (float)Math.Sin(y / 80f * 3.14f);
			Dust.NewDustPerfect(Center + new Vector2((float)Math.Cos(t) * (32 * w), -40 + (float)Math.Sin(t) * (8 * w) + y), DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, trailColor(timer2 / timerMax), 0.15f);

		}

		public override void PickupVisuals(int timer)
		{
			if (timer == 1)
			{
				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Pickups/get")); //start the SFX
				Filters.Scene.Deactivate("Shockwave");
			}
		}

		public override void PickupEffects(Player player)
		{
			AbilityHandler mp = player.GetHandler();
			mp.Unlock<Whip>();

			player.GetModPlayer<StarlightPlayer>().maxPickupTimer = 570;
			player.AddBuff(BuffID.Featherfall, 580);
		}
	}

	public class FaeflamePickupTile : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<FaeflamePickup>();

		public override string Texture => AssetDirectory.Invisible;
	}

	[SLRDebug]
	public class FaeflameTileItem : BaseTileItem
	{
		public FaeflameTileItem() : base("Faeflame", "{{Debug}} placer for ability pickup", "FaeflamePickupTile", -1) { }

		public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
	}
}