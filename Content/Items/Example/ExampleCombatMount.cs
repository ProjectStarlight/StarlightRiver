using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Example
{
	internal class ExampleCombatMount : CombatMount
	{
		public override string PrimaryIconTexture => "StarlightRiver/Assets/Items/Example/RocketAbility";
		public override string SecondaryIconTexture => "StarlightRiver/Assets/Items/Example/RingAbility";

		public override void SetDefaults()
		{
			primarySpeedCoefficient = 20;
			primaryCooldownCoefficient = 40;
			secondaryCooldownCoefficient = 240;
			secondarySpeedCoefficient = 30;
			damageCoefficient = 100;
			autoReuse = true;
		}

		public override void PostUpdate(Player player)
		{
			if (Math.Abs(player.velocity.X) < 11 * moveSpeedMultiplier)
				player.velocity.X *= moveSpeedMultiplier;

			if (!player.controlLeft && !player.controlRight)
				player.velocity *= (1 - Math.Max(1, moveSpeedMultiplier) * 0.1f);
		}

		public override void OnStartPrimaryAction(Player player)
		{
			Helpers.Helper.PlayPitched("Guns/FlareBoom", 1, 1, player.Center);
		}

		public override void PrimaryAction(int timer, Player player)
		{
			for(int k = 0; k < 6; k++)
			{
				int check = (int)(k / 6f * MaxPrimaryTime);

				if (timer == check)
				{
					var vel = Vector2.Normalize(Main.MouseWorld - player.Center) * 20;
					Projectile.NewProjectile(player.GetSource_Misc("Test"), player.Center, vel, ModContent.ProjectileType<Items.SteampunkSet.JetwelderJumperMissle>(), damageCoefficient, 0, player.whoAmI);
					Helpers.Helper.PlayPitched("Guns/FlareFire", 1, 1, player.Center);
				}
			}

			player.statDefense += 200;
		}

		public override void OnStartSecondaryAction(Player player)
		{
			Projectile.NewProjectile(player.GetSource_Misc("Test"), player.Center, Vector2.Zero, ModContent.ProjectileType<Items.Vitric.FireRing>(), damageCoefficient * 4, 0, player.whoAmI);
			Helpers.Helper.PlayPitched("Yeehaw", 1, 1, player.Center);
		}
	}

	internal class ExampleCombatMountData : ModMount
	{
		public override string Texture => "StarlightRiver/Assets/Items/Example/ExampleCombatMount";

		public override void SetStaticDefaults()
		{
			//beautiful EM copy/paste

			MountData.jumpHeight = 5; // How high the mount can jump.
			MountData.acceleration = 0.19f; // The rate at which the mount speeds up.
			MountData.jumpSpeed = 4f; // The rate at which the player and mount ascend towards (negative y velocity) the jump height when the jump button is presssed.
			MountData.blockExtraJumps = false; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
			MountData.constantJump = true; // Allows you to hold the jump button down.
			MountData.heightBoost = 20; // Height between the mount and the ground
			MountData.fallDamage = 0.5f; // Fall damage multiplier.
			MountData.runSpeed = 11f; // The speed of the mount
			MountData.dashSpeed = 8f; // The speed the mount moves when in the state of dashing.
			MountData.flightTimeMax = 0; // The amount of time in frames a mount can be in the state of flying.

			// Frame data and player offsets
			MountData.totalFrames = 1; // Amount of animation frames for the mount
			MountData.playerYOffsets = Enumerable.Repeat(20, MountData.totalFrames).ToArray(); // Fills an array with values for less repeating code
			MountData.xOffset = 13;
			MountData.yOffset = -12;
			MountData.playerHeadOffset = 22;
			MountData.bodyFrame = 3;
			// Standing
			MountData.standingFrameCount = 1;
			MountData.standingFrameDelay = 12;
			MountData.standingFrameStart = 0;
			// Running
			MountData.runningFrameCount = 1;
			MountData.runningFrameDelay = 12;
			MountData.runningFrameStart = 0;
			// Flying
			MountData.flyingFrameCount = 0;
			MountData.flyingFrameDelay = 0;
			MountData.flyingFrameStart = 0;
			// In-air
			MountData.inAirFrameCount = 1;
			MountData.inAirFrameDelay = 12;
			MountData.inAirFrameStart = 0;
			// Idle
			MountData.idleFrameCount = 1;
			MountData.idleFrameDelay = 12;
			MountData.idleFrameStart = 0;
			MountData.idleFrameLoop = true;
			// Swim
			MountData.swimFrameCount = MountData.inAirFrameCount;
			MountData.swimFrameDelay = MountData.inAirFrameDelay;
			MountData.swimFrameStart = MountData.inAirFrameStart;

			if (!Main.dedServ)
			{
				MountData.textureWidth = MountData.backTexture.Width() + 20;
				MountData.textureHeight = MountData.backTexture.Height();
			}
		}
	}

	internal class ExampleCombatMountItem : CombatMountItem
	{
		public override int MountType => ModContent.MountType<ExampleCombatMountData>();

		public override Type CombatMountType => typeof(ExampleCombatMount);

		public override string Texture => AssetDirectory.Debug;
	}
}
