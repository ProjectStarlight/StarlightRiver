using StarlightRiver.Content.Items.BaseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Items.Hell
{
	internal class FuryInABottle : SmartAccessory
	{
		public int charge;

		public override string Texture => AssetDirectory.HellItem + Name;

		public FuryInABottle() : base("Fury in a Bottle", "Allows a short double jump\nRecharges for every 100 damage dealt") { }

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += GainCharge;
			StarlightPlayer.OnHitNPCWithProjEvent += GainChargeProj;
		}

		private void GainCharge(Player player, Item Item, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player) && !player.GetJumpState<FuryJump>().Available)
			{
				var instance = GetEquippedInstance(player) as FuryInABottle;
				instance.charge += damageDone;
			}
		}

		private void GainChargeProj(Player player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player) && !player.GetJumpState<FuryJump>().Available)
			{
				var instance = GetEquippedInstance(player) as FuryInABottle;
				instance.charge += damageDone;
			}
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			player.GetJumpState<FuryJump>().Enable();

			if (charge >= 100)
			{
				player.GetJumpState<FuryJump>().Available = true;
				charge = 0;

				Helpers.Helper.PlayPitched("Effects/Bleep", 1, 0, player.Center);
				for(int k = 0; k < 10; k++)
				{
					Dust.NewDust(player.position, player.width, player.height, ModContent.DustType<Dusts.Cinder>(), 0, -2, 0, Color.Red);
				}
			}
		}
	}

	internal class FuryJump : ExtraJump
	{
		public override Position GetDefaultPosition()
		{
			return AfterBottleJumps;
		}

		public override float GetDurationMultiplier(Player player)
		{
			return 0.6f;
		}
	}
}
