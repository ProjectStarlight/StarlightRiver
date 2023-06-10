using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Abilities.Faewhip;
using Terraria.DataStructures;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items
{
	public static class ArmorHelper
	{
		public static bool IsSetEquipped(this ModItem Item, Player Player)
		{
			return Item.IsArmorSet(Player.armor[0], Player.armor[1], Player.armor[2]);
		}

		public static void QuickDrawArmor(PlayerDrawSet info, string texture, Color color, float scale, Vector2 offset)
		{
			if (info.drawPlayer.ActiveAbility<Whip>())
				return;

			Texture2D tex = Request<Texture2D>(texture).Value;

			info.DrawDataCache.Add(new DrawData(tex, (info.drawPlayer.position - Main.screenPosition + offset).ToPoint16().ToVector2(), null, color * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f), info.drawPlayer.headRotation, tex.Size() * 0.5f, scale, info.playerEffect, 0));
		}

		/// <summary>
		/// uses the Player frame as the frame in the passed sheet, frame position is normalized to the passed sheet height.  
		/// is color is null immune fade is applied by default
		/// </summary>
		/// <param name="info"></param>
		/// <param name="texture"></param>
		/// <param name="scale"></param>
		/// <param name="offset"></param>
		/// <param name="color"></param>
		/// <param name="immuneFade"></param>
		public static void QuickDrawHeadFramed(PlayerDrawSet info, string texture, float scale, Vector2 offset, Color? color = null, bool immuneFade = false)//TODO fix framing (uses leg frame since head is always zero)
		{
			if (info.drawPlayer.ActiveAbility<Whip>())
				return;

			//TODO: somewhere this is hardcoded to the sheet size, as gravity flip is broken for old starwood hat, for now it has a temp fix

			Texture2D newTex = Request<Texture2D>(texture).Value;

			//uses body frame for the 1.3 sheets because headframe is always zero for some reason
			int frame = (int)(info.drawPlayer.bodyFrame.Y / info.drawPlayer.bodyFrame.Height);//(int)((info.drawPlayer.bodyFrame.Y / 1120f) * 20);
			int height = (int)(newTex.Height / 20);

			Vector2 pos = (info.drawPlayer.MountedCenter - Main.screenPosition +
				offset + //flips offsets when gravity is
				new Vector2(0, info.drawPlayer.gfxOffY)).ToPoint16().ToVector2() + //stepping up blocks
				info.drawPlayer.headPosition;//player gore position
											 //head does not use bopping while walking since it is built into the sheet (despite vanilla using for the head for some reason?)

			Color drawColor = color ?? info.colorArmorHead;

			info.DrawDataCache.Add(new DrawData(newTex, pos, new Rectangle(0, frame * height, newTex.Width, height),
				immuneFade ? drawColor * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f) : drawColor,
				info.drawPlayer.headRotation,
				new Vector2(newTex.Width * 0.5f, newTex.Height * 0.025f),
				scale, info.playerEffect, 0));
		}

		/// <summary>
		/// uses the Player frame as the frame in the passed sheet, frame position is normalized to the passed sheet height.  
		/// is color is null immune fade is applied by default
		/// </summary>
		/// <param name="info"></param>
		/// <param name="texture"></param>
		/// <param name="scale"></param>
		/// <param name="offset"></param>
		/// <param name="color"></param>
		/// <param name="immuneFade"></param>
		public static void QuickDrawBodyFramed(PlayerDrawSet info, string texture, float scale, Vector2 offset, Color? color = null, bool immuneFade = false)
		{
			if (info.drawPlayer.ActiveAbility<Whip>())
				return;

			Texture2D newTex = Request<Texture2D>(texture).Value;

			const int frameCountX = 9;
			const int frameCountY = 4;

			//frame size based on new sheet
			int frameWidth = (int)(newTex.Width / frameCountX);
			int frameHeight = (int)(newTex.Height / frameCountY);

			Vector2 weirdGravityOffset = new Vector2(0, info.drawPlayer.gravDir == -1 ? 4 : 0);

			Vector2 pos = (info.drawPlayer.MountedCenter - Main.screenPosition +
				offset +
				new Vector2(0, info.drawPlayer.gfxOffY)).ToPoint16().ToVector2() +
				info.drawPlayer.bodyPosition + //player gore position
				Main.OffsetsPlayerHeadgear[info.drawPlayer.bodyFrame.Y / info.drawPlayer.bodyFrame.Height] * info.drawPlayer.gravDir;//bobbing while walking

			pos += weirdGravityOffset;

			Color drawColor = color ?? info.colorArmorBody;
			Color outColor = immuneFade ? drawColor * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f) : drawColor;

			//back shoulder may be broken or out of order
			int backShoulderFrameX = (int)(info.compBackShoulderFrame.X / info.compBackShoulderFrame.Width);
			int backShoulderFrameY = (int)(info.compBackShoulderFrame.Y / info.compBackShoulderFrame.Height);

			info.DrawDataCache.Add(new DrawData(newTex, pos, new Rectangle(backShoulderFrameX * frameWidth, backShoulderFrameY * frameHeight, frameWidth, frameHeight),
				outColor,
				info.drawPlayer.bodyRotation,
				new Vector2(frameWidth / 2, frameHeight / 2),
				scale, info.playerEffect, 0));

			int backArmFrameX = (int)(info.compBackArmFrame.X / info.compBackArmFrame.Width);
			int backArmFrameY = (int)(info.compBackArmFrame.Y / info.compBackArmFrame.Height);

			info.DrawDataCache.Add(new DrawData(newTex, pos, new Rectangle(backArmFrameX * frameWidth, backArmFrameY * frameHeight, frameWidth, frameHeight),
				outColor,
				info.compositeBackArmRotation,
				new Vector2(frameWidth / 2, frameHeight / 2),
				scale, info.playerEffect, 0));

			//gets values based on frame instead of location on vanilla sized sheet
			int torsoFrameX = (int)(info.compTorsoFrame.X / info.compTorsoFrame.Width);
			int torsoFrameY = (int)(info.compTorsoFrame.Y / info.compTorsoFrame.Height);

			info.DrawDataCache.Add(new DrawData(newTex, pos, new Rectangle(torsoFrameX * frameWidth, torsoFrameY * frameHeight, frameWidth, frameHeight),
				outColor,
				info.drawPlayer.bodyRotation,
				new Vector2(frameWidth / 2, frameHeight / 2),
				scale, info.playerEffect, 0));
		}

		public static void QuickDrawFrontArmsFramed(PlayerDrawSet info, string texture, float scale, Vector2 offset, Color? color = null, bool immuneFade = false)
		{
			if (info.drawPlayer.ActiveAbility<Whip>())
				return;

			Texture2D newTex = Request<Texture2D>(texture).Value;

			//gets values based on frame instead of location on vanilla sized sheet
			//int torsoFrameX = (int)(info.compTorsoFrame.X / info.compTorsoFrame.Width);
			//int torsoFrameY = (int)(info.compTorsoFrame.Y / info.compTorsoFrame.Height);

			const int frameCountX = 9;
			const int frameCountY = 4;

			//frame size based on new sheet
			int frameWidth = (int)(newTex.Width / frameCountX);
			int frameHeight = (int)(newTex.Height / frameCountY);

			Vector2 weirdGravityOffset = new Vector2(0, info.drawPlayer.gravDir == -1 ? 4 : 0);

			//Vector2 fixedOffset = new Vector2(offset.X, offset.Y);// info.drawPlayer.gravDir == -1 ? offset.Y : offset.Y);

			Color drawColor = color ?? info.colorArmorBody;
			Color outColor = immuneFade ? drawColor * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f) : drawColor;

			Vector2 pos = (info.drawPlayer.MountedCenter - Main.screenPosition +
				offset +
				new Vector2(0, info.drawPlayer.gfxOffY)).ToPoint16().ToVector2() +
				info.drawPlayer.bodyPosition + //player gore position (uses same for torso, arms, and shoulders
				Main.OffsetsPlayerHeadgear[info.drawPlayer.bodyFrame.Y / info.drawPlayer.bodyFrame.Height] * info.drawPlayer.gravDir;//bobbing while walking

			//values that are needed to match vanilla
			Vector2 someothervalue5 = new Vector2((float)(-5 * info.drawPlayer.direction), -1f);

			pos += someothervalue5;
			if (info.compFrontArmFrame.X / info.compFrontArmFrame.Width >= 7)
				pos += new Vector2(info.drawPlayer.direction, info.drawPlayer.gravDir);

			pos += weirdGravityOffset;

			int frontArmFrameX = (int)(info.compFrontArmFrame.X / info.compFrontArmFrame.Width);
			int frontArmFrameY = (int)(info.compFrontArmFrame.Y / info.compFrontArmFrame.Height);

			//front arm
			info.DrawDataCache.Add(new DrawData(newTex, pos + new Vector2(0f, 1f), new Rectangle(frontArmFrameX * frameWidth, frontArmFrameY * frameHeight, frameWidth, frameHeight),
				outColor,
				info.drawPlayer.bodyRotation + info.compositeFrontArmRotation,//may just need arm rotation
				new Vector2(frameWidth / 2, frameHeight / 2) + someothervalue5 + new Vector2(0f, 1f),
				scale, info.playerEffect, 0));

			if (info.compShoulderOverFrontArm)
			{
				Vector2 posTemp2 = (info.drawPlayer.MountedCenter - Main.screenPosition +
					offset +
					new Vector2(0, info.drawPlayer.gfxOffY)).ToPoint16().ToVector2() +
					info.drawPlayer.bodyPosition + // + //player gore position
					Main.OffsetsPlayerHeadgear[info.drawPlayer.bodyFrame.Y / info.drawPlayer.bodyFrame.Height] * info.drawPlayer.gravDir;//bobbing while walking

				posTemp2 += weirdGravityOffset;

				int frontShoulderFrameX = (int)(info.compFrontShoulderFrame.X / info.compFrontShoulderFrame.Width);
				int frontShoulderFrameY = (int)(info.compFrontShoulderFrame.Y / info.compFrontShoulderFrame.Height);

				info.DrawDataCache.Add(new DrawData(newTex, posTemp2, new Rectangle(frontShoulderFrameX * frameWidth, frontShoulderFrameY * frameHeight, frameWidth, frameHeight),
					outColor,
					info.drawPlayer.bodyRotation,
					new Vector2(frameWidth / 2, frameHeight / 2),
					scale, info.playerEffect, 0));
			}
		}

		/// <summary>
		/// uses the Player frame as the frame in the passed sheet, frame position is normalized to the passed sheet height.  
		/// is color is null immune fade is applied by default
		/// </summary>
		/// <param name="info"></param>
		/// <param name="texture"></param>
		/// <param name="scale"></param>
		/// <param name="offset"></param>
		/// <param name="color"></param>
		/// <param name="immuneFade"></param>
		public static void QuickDrawLegsFramed(PlayerDrawSet info, string texture, float scale, Vector2 offset, Color? color = null, bool immuneFade = false)
		{
			if (info.drawPlayer.ActiveAbility<Whip>())
				return;

			Texture2D newTex = Request<Texture2D>(texture).Value;
			int frame = (int)(info.drawPlayer.legFrame.Y * 0.01785714286f);//(int)((frame / 1120f) * 20);
			var pos = (info.drawPlayer.position + info.drawPlayer.legPosition - Main.screenPosition + offset).ToPoint16().ToVector2();
			int height = (int)(newTex.Height * 0.05f);//tex.Height / 20
			Color drawColor = color ?? info.colorArmorLegs;
			info.DrawDataCache.Add(new DrawData(newTex, pos, new Rectangle(0, frame * height, newTex.Width, height), immuneFade ? drawColor * ((255 - info.drawPlayer.immuneAlpha) * 0.003921568627f) : drawColor, info.drawPlayer.legRotation, new Vector2(newTex.Width * 0.5f, newTex.Height * 0.025f), scale, info.playerEffect, 0));
		}
	}
}