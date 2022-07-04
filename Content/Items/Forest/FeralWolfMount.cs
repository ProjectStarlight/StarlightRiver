using Microsoft.Xna.Framework;
using Terraria.Graphics.Effects;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Forest
{
	internal class FeralWolfMount : CombatMount
	{
		List<Projectile> buffedMinions = new List<Projectile>();

		public override string PrimaryIconTexture => "StarlightRiver/Assets/Items/Example/RocketAbility";
		public override string SecondaryIconTexture => "StarlightRiver/Assets/Items/Example/RingAbility";

		public override void SetDefaults()
		{
			primarySpeedCoefficient = 14;
			primaryCooldownCoefficient = 20;
			secondaryCooldownCoefficient = 600;
			secondarySpeedCoefficient = 120;
			damageCoefficient = 16;
			autoReuse = false;
		}

		public override void PostUpdate(Player player)
		{
			
		}

		public override void OnStartPrimaryAction(Player player)
		{
			int i = Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Vector2.Zero, ModContent.ProjectileType<FeralWolfMountBite>(), damageCoefficient, 0, player.whoAmI, MaxPrimaryTime);
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
			var animTime = (float)secondarySpeedCoefficient / 4f;
			var time = Math.Max(0, (timer - animTime * 3) / (animTime));
			Filters.Scene.Activate("Shockwave", player.Center).GetShader().UseProgress(2f).UseIntensity(100 - time * 100).UseDirection(new Vector2(0.1f - time * 0.1f, 0.02f - time * 0.02f));

			if (timer == 1)
			{
				foreach (Projectile proj in buffedMinions)
				{
					proj.extraUpdates--;
				}

				buffedMinions.Clear();
			}			
		}
	}

	internal class FeralWolfMountData : ModMount
	{
		public override string Texture => "StarlightRiver/Assets/Items/Example/ExampleCombatMount";

		public override void SetStaticDefaults()
		{
			MountData.jumpHeight = 5;
			MountData.acceleration = 0.14f;
			MountData.jumpSpeed = 8f; 
			MountData.blockExtraJumps = false; 
			MountData.heightBoost = 20; 
			MountData.runSpeed = 8f; 

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

		public override string Texture => AssetDirectory.Debug;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Juicy Steak");
			Tooltip.SetDefault("Summons a feral wolf combat mount\nInflicts bleeding with powerful bites\nRight click to howl and boost the speed of minions");
		}
	}
}
