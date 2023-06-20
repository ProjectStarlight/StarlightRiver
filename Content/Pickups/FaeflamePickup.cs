using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Core.Systems;
using System;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Pickups
{
	internal class FaeflamePickup : AbilityPickup
	{
		public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
		public override Color GlowColor => new(255, 255, 130);

		public override bool CanPickup(Player Player)
		{
			return !Player.GetHandler().Unlocked<Whip>();
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Faeflame");
		}

		public override void Visuals()
		{
			Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.visualTimer), (float)Math.Sin(StarlightWorld.visualTimer)) * (float)Math.Sin(StarlightWorld.visualTimer * 2 + 1) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);
			Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.visualTimer + 2) / 2, (float)Math.Sin(StarlightWorld.visualTimer + 2)) * (float)Math.Sin(StarlightWorld.visualTimer * 2 + 4) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);
			Dust.NewDustPerfect(NPC.Center + new Vector2((float)Math.Cos(StarlightWorld.visualTimer + 4), (float)Math.Sin(StarlightWorld.visualTimer + 4) / 2) * (float)Math.Sin(StarlightWorld.visualTimer * 2 + 2) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.Zero, 0, default, 0.65f);

			Dust.NewDustPerfect(NPC.Center + Vector2.One.RotateRandom(Math.PI) * (float)Math.Sin(StarlightWorld.visualTimer * 2 + 2) * 32, DustType<Content.Dusts.GoldWithMovement>(), Vector2.UnitY * -2, 0, default, 0.25f);
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

	public class FaeflamePickupTile : AbilityPickupTile
	{
		public override int PickupType => NPCType<FaeflamePickup>();
	}

	[SLRDebug]
	public class FaeflameTileItem : QuickTileItem
	{
		public FaeflameTileItem() : base("Faeflame", "Debug placer for ability pickup", "FaeflamePickupTile", -1) { }

		public override string Texture => "StarlightRiver/Assets/Abilities/Faeflame";
	}
}