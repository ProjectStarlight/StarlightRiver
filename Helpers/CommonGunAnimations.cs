using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Helpers
{
	public static class CommonGunAnimations
	{
		// for custom hold style animations, see Skullbuster / Sling etc
		public static void CleanHoldStyle(Player player, float desiredRotation, Vector2 desiredPosition, Vector2 spriteSize, Vector2? rotationOriginFromCenter = null, bool noSandstorm = false, bool flipAngle = false, bool stepDisplace = true)
		{
			if (noSandstorm)
				player.sandStorm = false;

			if (rotationOriginFromCenter == null)
				rotationOriginFromCenter = new Vector2?(Vector2.Zero);

			Vector2 origin = rotationOriginFromCenter.Value;
			origin.X *= player.direction;
			origin.Y *= player.gravDir;
			player.itemRotation = desiredRotation;

			if (flipAngle)
				player.itemRotation *= player.direction;
			else if (player.direction < 0)
				player.itemRotation += 3.1415927f;

			Vector2 consistentAnchor = player.itemRotation.ToRotationVector2() * (spriteSize.X / -2f - 10f) * player.direction - origin.RotatedBy(player.itemRotation, default);
			Vector2 offsetAgain = spriteSize * -0.5f;
			Vector2 finalPosition = desiredPosition + offsetAgain + consistentAnchor;
			if (stepDisplace)
			{
				int frame = player.bodyFrame.Y / player.bodyFrame.Height;
				if (frame > 6 && frame < 10 || frame > 13 && frame < 17)
					finalPosition -= Vector2.UnitY * 2f;
			}

			player.itemLocation = finalPosition + new Vector2(spriteSize.X * 0.5f, 0f);
		}

		/// <summary>
		/// Helper method for quick custom gun style recoil item animations, edits the item position
		/// </summary>
		/// <param name="player">Player using item</param>
		/// <param name="item">Item being used</param>
		/// <param name="shootDirection">Shot direction, either 1 or -1</param>
		/// <param name="recoil">For the animation; how many pixels it recoils back</param>
		/// <param name="itemSize">Exact size of item sprite</param>
		/// <param name="itemOrigin">Offset of item for drawing correctly</param>
		/// <param name="animationRatio">ratio of the animation, example by default, 5% of animation spend recoiling back, other 95% spent returning to original position</param>
		/// <returns></returns>
		public static Vector2 SetGunUseStyle(Player player, Item item, int shootDirection, float recoil, Vector2 itemSize, Vector2 itemOrigin, Vector2? animationRatio = null)
		{
			if (animationRatio == null)
				animationRatio = new Vector2(0.05f, 0.95f);

			if (item.noUseGraphic) // the item draws wrong for the first frame it is drawn when you switch directions for some odd reason, this plus setting it to true in shoot makes it not draw for the first frame.
				item.noUseGraphic = false;

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;

			if (Main.myPlayer == player.whoAmI)
				player.direction = shootDirection;

			float itemRotation = player.compositeFrontArm.rotation + 1.5707964f * player.gravDir;
			Vector2 itemPosition = player.MountedCenter;

			if (animProgress < animationRatio.Value.X)
			{
				float lerper = animProgress / animationRatio.Value.X;
				itemPosition += itemRotation.ToRotationVector2() * MathHelper.Lerp(0f, recoil, Eases.EaseCircularOut(lerper));
			}
			else
			{
				float lerper = (animProgress - animationRatio.Value.X) / animationRatio.Value.Y;
				itemPosition += itemRotation.ToRotationVector2() * MathHelper.Lerp(recoil, 0f, Eases.EaseBackInOut(lerper));
			}

			CleanHoldStyle(player, itemRotation, itemPosition, itemSize, new Vector2?(itemOrigin), false, false, true);

			return itemPosition;
		}

		/// <summary>
		/// Helper method for quick custom gun style recoil item animations, edits the item rotation
		/// </summary>
		/// <param name="player">Player using item</param>
		/// <param name="item">Item being used</param>
		/// <param name="shootDirection">Shot direction, either 1 or -1</param>
		/// <param name="recoil">For the animation; how many pixels it recoils "up" (akin to gun kickback)</param>
		/// <param name="animationRatio">ratio of the animation, example by default, 5% of animation spend recoiling up, other 95% spent returning to original rotation</param>
		public static void SetGunUseItemFrame(Player player, int shootDirection, float shootRotation, float recoil, bool setBackArm = false, Vector2? animationRatio = null)
		{
			if (animationRatio == null)
				animationRatio = new Vector2(0.05f, 0.95f);

			if (Main.myPlayer == player.whoAmI)
				player.direction = shootDirection;

			float animProgress = 1f - player.itemTime / (float)player.itemTimeMax;
			float rotation = shootRotation * player.gravDir + 1.5707964f;

			if (animProgress < animationRatio.Value.X)
			{
				float lerper = animProgress / animationRatio.Value.X;
				rotation += MathHelper.Lerp(0f, recoil, Eases.EaseCircularOut(lerper)) * player.direction;
			}
			else
			{
				float lerper = (animProgress - animationRatio.Value.X) / animationRatio.Value.Y;
				rotation += MathHelper.Lerp(recoil, 0, Eases.EaseBackInOut(lerper)) * player.direction;
			}

			Player.CompositeArmStretchAmount stretch = Player.CompositeArmStretchAmount.Full;
			if (animProgress < 0.5f)
				stretch = Player.CompositeArmStretchAmount.None;
			else if (animProgress < 0.75f)
				stretch = Player.CompositeArmStretchAmount.ThreeQuarters;

			player.SetCompositeArmFront(true, stretch, rotation);
			if (setBackArm)
				player.SetCompositeArmBack(true, stretch, rotation + MathHelper.ToRadians(25f) * player.direction);
		}
	}
}