using Terraria;
using Terraria.ID;
using Terraria.DataStructures;
using Terraria.ModLoader;
using StarlightRiver.Core;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Helpers;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using System;
using System.Linq;
using System.Collections.Generic;

namespace StarlightRiver.Content.Items.SteampunkSet
{
	public class Cogwheel : ModItem
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Cogwheel");
			Tooltip.SetDefault("Egshels update this lol");
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing; 
			Item.value = Item.sellPrice(gold: 3);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item79; 
			Item.noMelee = true; 
			Item.mountType = ModContent.MountType<CogwheelMount>();
		}
	}
	public class CogwheelMount : ModMount
	{
		// Since only a single instance of ModMountData ever exists, we can use player.mount._mountSpecificData to store additional data related to a specific mount.
		// Using something like this for gameplay effects would require ModPlayer syncing, but this example is purely visual.
		protected class CogWheelSpecificData
		{
			public float rotation;

			public CogWheelSpecificData()
			{
				rotation = 0;
			}
		}

		public override void SetStaticDefaults()
		{
			// Movement
			MountData.jumpHeight = 8; // How high the mount can jump.
			MountData.acceleration = CogwheelPlayer.Acceleration; // The rate at which the mount speeds up.
			MountData.jumpSpeed = 4f; // The rate at which the player and mount ascend towards (negative y velocity) the jump height when the jump button is presssed.
			MountData.blockExtraJumps = true; // Determines whether or not you can use a double jump (like cloud in a bottle) while in the mount.
			MountData.constantJump = false; // Allows you to hold the jump button down.
			MountData.fallDamage = 1f; // Fall damage multiplier.
			MountData.runSpeed = CogwheelPlayer.RunSpeed; // The speed of the mount
			MountData.dashSpeed = 8f; // The speed the mount moves when in the state of dashing.
			MountData.flightTimeMax = 0; // The amount of time in frames a mount can be in the state of flying.

			// Misc
			MountData.fatigueMax = 0;
			MountData.buff = ModContent.BuffType<CogwheelBuff>(); // The ID number of the buff assigned to the mount.

			// Effects
			MountData.spawnDust = 6; // The ID of the dust spawned when mounted or dismounted.

			// Frame data and player offsets
			MountData.totalFrames = 1; // Amount of animation frames for the mount
			MountData.heightBoost = 52; // Height between the mount and the ground
			MountData.playerYOffsets = Enumerable.Repeat(38, 1).ToArray(); // Fills an array with values for less repeating code
			MountData.xOffset = 0;
			MountData.yOffset = 14;
			MountData.bodyFrame = 0;
			MountData.playerHeadOffset = 0;
		}

		public override void UpdateEffects(Player player)
		{
			MountData.heightBoost = 52;
			MountData.yOffset = 14;
			MountData.playerYOffsets = Enumerable.Repeat(38 - (int)(4 * Math.Sin(((CogWheelSpecificData)player.mount._mountSpecificData).rotation * 2)), 1).ToArray(); // Fills an array with values for less repeating code
			((CogWheelSpecificData)player.mount._mountSpecificData).rotation += player.GetModPlayer<CogwheelPlayer>().climbing ? (player.velocity.Y * Math.Sign(player.GetModPlayer<CogwheelPlayer>().oldSpeed)) / -40f : player.velocity.X / 40f;
		}

		public override void SetMount(Player player, ref bool skipDust)
		{
			player.mount._mountSpecificData = new CogWheelSpecificData();
		}

		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
		{
			// Draw is called for each mount texture we provide, so we check drawType to avoid duplicate draws.
			if (drawType == 0)
			{
				Texture2D platformTex = ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "CogwheelMount").Value;
				Texture2D wheelTex = ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "CogwheelMount_Wheel").Value;
				var drawPos = drawPosition;
				playerDrawData.Add(new DrawData(wheelTex, drawPos + new Vector2(0, 17 + (int)(2 * Math.Sin(((CogWheelSpecificData)drawPlayer.mount._mountSpecificData).rotation * 2))), new Rectangle(0, 0, wheelTex.Width, wheelTex.Height), drawColor, ((CogWheelSpecificData)drawPlayer.mount._mountSpecificData).rotation, wheelTex.Size() / 2, drawScale, SpriteEffects.None, 0));
				playerDrawData.Add(new DrawData(platformTex, drawPos + new Vector2(0, 17 + (int)(4 * Math.Sin(((CogWheelSpecificData)drawPlayer.mount._mountSpecificData).rotation * 2))), new Rectangle(0, 0, platformTex.Width, platformTex.Height), drawColor, drawPlayer.fullRotation, (platformTex.Size() / 2) + new Vector2(0, 17), drawScale, SpriteEffects.None, 0));
			}

			// by returning true, the regular drawing will still happen.
			return false;
		}
	}
	class CogwheelBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public CogwheelBuff() : base("Cogwheel", "Egshels update this lol", false) { }

		public override void Update(Player player, ref int buffIndex)
		{
			player.mount.SetMount(ModContent.MountType<CogwheelMount>(), player);
			player.buffTime[buffIndex] = 10; // reset buff time
			player.GetModPlayer<CogwheelPlayer>().mounted = true;
		}
	}

	class CogwheelPlayer : ModPlayer
    {
		public bool mounted = false;

		public bool climbing = false;

		public float climbSpeed;

		public float oldSpeed;

		public static float Acceleration = 0.17f;
		public static int RunSpeed = 11;

        public override void ResetEffects()
        {
			mounted = false;
        }

        public override void PostUpdate()
        {
			if (!mounted)
				return;

			Player.fullRotationOrigin = new Vector2(Player.Size.X / 2, Player.Size.Y + 17);
			Player.fullRotation = (float)Math.Sqrt(Math.Abs(climbing ? Player.velocity.Y : Player.velocity.X)) * 0.025f * Math.Sign(oldSpeed);
			if (Player.velocity.X == 0 && (Player.controlLeft || Player.controlRight))
			{
				if (!climbing)
					climbSpeed = MathHelper.Max(-(Math.Abs(oldSpeed) + Acceleration), -RunSpeed);
				else
					climbSpeed = MathHelper.Max((climbSpeed - Acceleration), -RunSpeed);

				Player.velocity.Y = climbSpeed;
				climbing = true;
			}
			else
			{
				if (climbing)
                {
					Player.velocity.Y = 0;
					if (Player.controlLeft)
						Player.velocity.X = climbSpeed;
					else if (Player.controlRight)
						Player.velocity.X = -climbSpeed;
				}
				else
                {
					oldSpeed = Player.velocity.X;
                }
				climbSpeed = 0;
				climbing = false;
			}
			base.PostUpdate();
        }
    }
}
