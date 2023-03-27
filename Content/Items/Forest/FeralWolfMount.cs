using StarlightRiver.Core.Systems.CombatMountSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;
using Terraria.Graphics.Effects;

namespace StarlightRiver.Content.Items.Forest
{
	internal class FeralWolfMount : CombatMount
	{
		readonly List<Projectile> buffedMinions = new();

		public override string PrimaryIconTexture => AssetDirectory.ForestItem + "FeralWolfMountPrimary";
		public override string SecondaryIconTexture => AssetDirectory.ForestItem + "FeralWolfMountSecondary";

		public override void SetDefaults()
		{
			primarySpeedCoefficient = 14;
			primaryCooldownCoefficient = 12;
			secondaryCooldownCoefficient = 600;
			secondarySpeedCoefficient = 120;
			damageCoefficient = 16;
			autoReuse = false;
		}

		public override void PostUpdate(Player player)
		{
			CombatMountPlayer mp = player.GetModPlayer<CombatMountPlayer>();
			float progress = 1 - Math.Max(0, (mp.mountingTime - 15) / 15f);

			if (progress < 1)
			{
				for (int k = 0; k < 2; k++)
				{
					Vector2 pos = player.Center + new Vector2(0, 40 - (int)(progress * 40));
					Dust.NewDustPerfect(pos + Vector2.UnitX * Main.rand.NextFloat(-20, 20), ModContent.DustType<Dusts.Cinder>(), Main.rand.NextVector2Circular(1, 1), 0, new Color(255, 255, 200), 0.5f);
				}
			}
		}

		public override void OnStartPrimaryAction(Player player)
		{
			int i = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<FeralWolfMountBite>(), damageCoefficient, 10, player.whoAmI, MaxPrimaryTime);
			Main.projectile[i].rotation = player.direction == 1 ? 0 : 3.14f;
		}

		public override void PrimaryAction(int timer, Player player)
		{

		}

		public override void OnStartSecondaryAction(Player player)
		{
			foreach (Projectile proj in Main.projectile.Where(n => n.active && n.owner == player.whoAmI && n.minion))
			{
				proj.extraUpdates++;
				buffedMinions.Add(proj);
			}
		}

		public override void SecondaryAction(int timer, Player player)
		{
			float animTime = secondarySpeedCoefficient / 4f;
			float time = Math.Max(0, (timer - animTime * 3) / animTime);

			if (time > 0)
				Filters.Scene.Activate("Shockwave", player.Center).GetShader().UseProgress(2f).UseIntensity(100 - time * 100).UseDirection(new Vector2(0.1f - time * 0.1f, 0.02f - time * 0.02f));
			else
				Filters.Scene.Deactivate("Shockwave");

			if (timer == 1)
			{
				foreach (Projectile proj in buffedMinions)
				{
					proj.extraUpdates--;

					if (proj.extraUpdates < 0)
						proj.extraUpdates = 0;
				}

				buffedMinions.Clear();
			}
		}
	}

	internal class FeralWolfMountData : ModMount
	{
		public override string Texture => AssetDirectory.ForestItem + "FeralWolfMount";

		public override void SetMount(Player player, ref bool skipDust)
		{
			skipDust = true;
		}

		public override void Dismount(Player player, ref bool skipDust)
		{
			skipDust = true;
		}

		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			Texture2D tex2 = ModContent.Request<Texture2D>(Texture + "Shape").Value;
			CombatMountPlayer mp = drawPlayer.GetModPlayer<CombatMountPlayer>();
			float progress = 1 - Math.Max(0, (mp.mountingTime - 15) / 15f);

			Vector2 pos = drawPlayer.Center - Main.screenPosition + new Vector2(0, 52 - (int)(progress * 40));
			var source = new Rectangle(0, 40 - (int)(progress * 40), 60, (int)(progress * 40));
			var source2 = new Rectangle(0, 40 - (int)(progress * 40), 60, 2);

			if (mp.mountingTime <= 0)
				pos.Y += drawPlayer.gfxOffY;

			playerDrawData.Add(new DrawData(tex, pos, source, drawColor, drawPlayer.fullRotation, new Vector2(31, 22), 1, spriteEffects, 0));

			if (progress < 1)
				playerDrawData.Add(new DrawData(tex2, pos, source2, Color.White, drawPlayer.fullRotation, new Vector2(31, 22), 1, spriteEffects, 0));

			return false;
		}

		public override void SetStaticDefaults()
		{
			MountData.jumpHeight = 6;
			MountData.acceleration = 0.1f;
			MountData.jumpSpeed = 10f;
			MountData.blockExtraJumps = false;
			MountData.heightBoost = 14;
			MountData.runSpeed = 6f;

			// Frame data and player offsets
			MountData.totalFrames = 1;
			MountData.playerYOffsets = Enumerable.Repeat(20, MountData.totalFrames).ToArray();
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

	internal class FeralWolfMountItem : CombatMountItem
	{
		public override int MountType => ModContent.MountType<FeralWolfMountData>();

		public override Type CombatMountType => typeof(FeralWolfMount);

		public override string Texture => AssetDirectory.ForestItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Juicy Steak");
			Tooltip.SetDefault("Summons a feral wolf combat mount\nInflicts bleeding with powerful bites\nRight click to howl and boost the speed of minions");
		}
	}
}