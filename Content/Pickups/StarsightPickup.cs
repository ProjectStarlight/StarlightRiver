using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using StarlightRiver.Content.Abilities.Hint;
using StarlightRiver.Content.GUI;
using StarlightRiver.Content.NPCs.Starlight;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;

namespace StarlightRiver.Content.Pickups
{
	internal class StarsightPickup : AbilityPickup
	{
		public override Asset<Texture2D> Texture => Assets.Abilities.Hint;

		public override Color GlowColor => new(65, 160, 255);

		public StarsightPickup() : base(ModContent.TileType<StarsightPickupTile>()) { }

		public override bool CanPickup(Player Player)
		{
			return AlicanSafetySystem.IntendedAlicanPhase > 1 && !Player.GetHandler().Unlocked<HintAbility>();
		}

		public override void Visuals()
		{
			if (Main.GameUpdateCount % 2 == 0)
			{
				Dust.NewDustPerfect(Center + new Vector2((float)Math.Cos(StarlightWorld.visualTimer) * 32, (float)Math.Sin(StarlightWorld.visualTimer * 2) * 12), ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, new Color(70, 210, 255, 0), 0.25f);
				Dust.NewDustPerfect(Center + new Vector2((float)Math.Cos(StarlightWorld.visualTimer + 3.14f) * 32, (float)Math.Sin(-StarlightWorld.visualTimer * 2) * 12), ModContent.DustType<Dusts.PixelatedGlow>(), Vector2.Zero, 0, new Color(90, 140, 255, 0), 0.25f);
			}
		}

		public override void PickupVisuals(int timer)
		{
			if (timer == 1)
			{
				if (StarlightRiver.Instance.AbilityKeys.Get<HintAbility>().GetAssignedKeys().Count <= 0)
					KeybindHelper.OpenKeybindsWithHelp();

				TutorialManager.ActivateTutorial("Starsight");

				SoundEngine.PlaySound(new SoundStyle($"{nameof(StarlightRiver)}/Sounds/Pickups/get")); //start the SFX
				Filters.Scene.Deactivate("Shockwave");
			}
		}

		public override void PickupEffects(Player player)
		{
			AbilityHandler mp = player.GetHandler();

			mp.Unlock<HintAbility>();

			if (player == Main.LocalPlayer)
			{
				StarlightHUD.gainAnimationTimer = 240;
			}

			player.GetModPlayer<StarlightPlayer>().maxPickupTimer = 240;
			player.AddBuff(BuffID.Featherfall, 250);
		}
	}

	public class StarsightPickupTile : DummyTile
	{
		public override int DummyType => DummySystem.DummyType<StarsightPickup>();

		public override string Texture => AssetDirectory.Invisible;
	}

	[SLRDebug]
	public class StarsightTileItem : QuickTileItem
	{
		public StarsightTileItem() : base("Starsight", "{{Debug}} placer for ability pickup", "StarsightPickupTile", -1) { }

		public override string Texture => "StarlightRiver/Assets/Abilities/Hint";
	}
}