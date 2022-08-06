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
			Tooltip.SetDefault("Summons a ridable Cogwheel mount");
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 30;
			Item.useTime = 20;
			Item.useAnimation = 20;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.value = Item.sellPrice(0, 1, 0, 0);
			Item.rare = ItemRarityID.Green;
			Item.UseSound = SoundID.Item79; 
			Item.noMelee = true; 
			Item.mountType = ModContent.MountType<CogwheelMount>();
		}
		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.DemoniteBar, 10);
			recipe.AddIngredient(ModContent.ItemType<AncientGear>(), 8);
			recipe.AddTile(TileID.Anvils);

			Recipe recipe2 = CreateRecipe();
			recipe2.AddIngredient(ItemID.CrimtaneBar, 10);
			recipe2.AddIngredient(ModContent.ItemType<AncientGear>(), 8);
			recipe2.AddTile(TileID.Anvils);
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
			MountData.playerHeadOffset = 52;
		}

        public override void Dismount(Player player, ref bool skipDust)
        {
			for (int j = 0; j < 17; j++)
			{
				Vector2 direction = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Dust.NewDustPerfect((player.Center + (direction * 6) + new Vector2(0, 40)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(2, 10), 0, new Color(255, 255, 60) * 0.8f, 1.6f);
			}
			skipDust = true;
        }
        public override void UpdateEffects(Player player)
		{
			CogwheelPlayer modPlayer = player.GetModPlayer<CogwheelPlayer>();
			MountData.playerYOffsets = Enumerable.Repeat(38 - (int)(2 * Math.Sin(((CogWheelSpecificData)player.mount._mountSpecificData).rotation * 2)), 1).ToArray(); // Fills an array with values for less repeating code
			((CogWheelSpecificData)player.mount._mountSpecificData).rotation += modPlayer.climbing ? (player.velocity.Y * Math.Sign(modPlayer.oldSpeed)) / -40f : player.velocity.X / 40f;

			modPlayer.armLerper = EaseFunction.EaseQuadIn.Ease(0.5f + (0.5f * (float)Math.Sin(((CogWheelSpecificData)player.mount._mountSpecificData).rotation * 1.1f)));
		}

		public override void SetMount(Player player, ref bool skipDust)
		{
			for (int j = 0; j < 17; j++)
			{
				Vector2 direction = Main.rand.NextFloat(6.28f).ToRotationVector2();
				Dust.NewDustPerfect((player.Center + (direction * 6) + new Vector2(0, 40)) + new Vector2(0, 35), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(2, 10), 0, new Color(255, 255, 60) * 0.8f, 1.6f); 
			}
			skipDust = true;
			player.mount._mountSpecificData = new CogWheelSpecificData();

		}

		public override bool Draw(List<DrawData> playerDrawData, int drawType, Player drawPlayer, ref Texture2D texture, ref Texture2D glowTexture, ref Vector2 drawPosition, ref Rectangle frame, ref Color drawColor, ref Color glowColor, ref float rotation, ref SpriteEffects spriteEffects, ref Vector2 drawOrigin, ref float drawScale, float shadow)
		{
			// Draw is called for each mount texture we provide, so we check drawType to avoid duplicate draws.
			if (drawType == 0)
			{
				Texture2D platformTex = ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "CogwheelMount").Value;
				Texture2D wheelTex = ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "CogwheelMount_Wheel").Value;
				Texture2D baseTex = ModContent.Request<Texture2D>(AssetDirectory.SteampunkItem + "CogwheelMount_Base").Value;
				var drawPos = drawPosition;
				playerDrawData.Add(new DrawData(baseTex, drawPos + new Vector2(0, 17 + (int)(2 * Math.Sin(((CogWheelSpecificData)drawPlayer.mount._mountSpecificData).rotation * 2))), new Rectangle(0, 0, platformTex.Width, platformTex.Height), drawColor, drawPlayer.fullRotation, ((baseTex.Size() / 2) / new Vector2(1, 3)) + new Vector2(0, 17), drawScale, SpriteEffects.None, 0));
				playerDrawData.Add(new DrawData(wheelTex, drawPos + new Vector2(0, 17 + (int)(1 * Math.Sin(((CogWheelSpecificData)drawPlayer.mount._mountSpecificData).rotation * 2))), new Rectangle(0, 0, wheelTex.Width, wheelTex.Height), drawColor, ((CogWheelSpecificData)drawPlayer.mount._mountSpecificData).rotation, wheelTex.Size() / 2, drawScale, SpriteEffects.None, 0));
				playerDrawData.Add(new DrawData(platformTex, drawPos + new Vector2(0, 17 + (int)(2 * Math.Sin(((CogWheelSpecificData)drawPlayer.mount._mountSpecificData).rotation * 2))), new Rectangle(0, 0, platformTex.Width, platformTex.Height), drawColor, drawPlayer.fullRotation, ((platformTex.Size() / 2) / new Vector2(1, 3)) + new Vector2(0, 17), drawScale, SpriteEffects.None, 0));
			}

			// by returning true, the regular drawing will still happen.
			return false;
		}
	}
	class CogwheelBuff : SmartBuff
	{
		public override string Texture => AssetDirectory.SteampunkItem + Name;

		public CogwheelBuff() : base("Cogwheel", "They see me rollin'", false, true) { }

        public override void SafeSetDetafults()
        {
			Main.buffNoTimeDisplay[Type] = true;
        }
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

		public float armLerper;

		private float armRotFront = 0f;

		private float armRotBack = 0f;

		public static float Acceleration = 0.17f;
		public static int RunSpeed = 11;

		private bool rotationReset = true;

        public override void ResetEffects()
        {
			mounted = false;
        }

        public override bool PreHurt(bool pvp, bool quiet, ref int damage, ref int hitDirection, ref bool crit, ref bool customDamage, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource, ref int cooldownCounter)
        {
			if (damageSource.SourceOtherIndex == 3 && mounted)
				return false;
			return base.PreHurt(pvp, quiet, ref damage, ref hitDirection, ref crit, ref customDamage, ref playSound, ref genGore, ref damageSource, ref cooldownCounter);
        }

        public override void PostUpdate()
        {
			if (!mounted)
			{
				if (!rotationReset)
                {
					Player.fullRotationOrigin = new Vector2(Player.Size.X / 2, Player.Size.Y / 2);
					Player.fullRotation = 0;
					rotationReset = true;
				}
				return;
			}
			rotationReset = false;

			Player.legFrame = new Rectangle(0, 56 * 3, 40, 56);

			if (Player.ownedProjectileCounts[ModContent.ProjectileType<CogwheelHitbox>()] == 0 && !Player.dead)
				Projectile.NewProjectile(Player.GetSource_None(), Player.Center, Vector2.Zero, ModContent.ProjectileType<CogwheelHitbox>(), 15, 2, Player.whoAmI);

			Vector2 dustPos = Player.Center + new Vector2(Player.velocity.X * 2, 64);
			Vector2 direction = new Vector2(Player.direction, 0);
			if (climbing)
			{
				direction = new Vector2(0, -1);
				dustPos = Player.Center + new Vector2(Player.direction * 15, 45 + (Player.velocity.Y * 2));
			}
			if ((Player.velocity.X == 0 && climbing) || Player.velocity.Y == 0)
			{
				for (int j = 0; j < Player.velocity.Length() * 0.05f; j++)
				{
					direction = Vector2.Normalize(-direction).RotatedByRandom(0.6f).RotatedBy(1.57f + (0.6f * Player.direction));
					Dust.NewDustPerfect(dustPos + new Vector2(0, 15), ModContent.DustType<Dusts.BuzzSpark>(), direction.RotatedBy(Main.rand.NextFloat(-0.2f, 0.2f) - 1.57f) * Main.rand.Next(2, 5), 0, new Color(255, 255, 60) * 0.8f, 1.3f);
				}
			}

			if (oldSpeed == 0)
            {
				if (Player.controlLeft)
					oldSpeed = -0.01f;
				if (Player.controlRight)
					oldSpeed = 0.01f;
			}
			armRotFront = MathHelper.Lerp(armRotFront, (0.105f * MathHelper.Min(Player.velocity.Length(), 11)), 0.2f);
			armRotBack = MathHelper.Lerp(armRotBack, (0.105f * MathHelper.Min(Player.velocity.Length(), 11)), 0.2f);

			if (Player.itemAnimation == 0)
			{

				Player.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(6.11f, 6.28f, armLerper) - armRotFront * Player.direction);
				Player.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, MathHelper.Lerp(0.17f, 0f, armLerper) + armRotBack * Player.direction);
			}

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
					Player.velocity.Y *= 0.75f;
					/*if (Player.controlLeft)
						Player.velocity.X = climbSpeed;
					else if (Player.controlRight)
						Player.velocity.X = -climbSpeed;*/
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

	public class CogwheelCollisionGNPC : GlobalNPC
    {
        public override bool CanHitPlayer(NPC npc, Player target, ref int cooldownSlot)
        {
			CogwheelPlayer modPlayer = target.GetModPlayer<CogwheelPlayer>();
			if (modPlayer.mounted && !(npc.Hitbox.Intersects(new Rectangle((int)target.position.X, (int)target.position.Y, target.width, target.height - 34))))
				return false;
            return base.CanHitPlayer(npc, target, ref cooldownSlot);
        }
    }
	public class CogwheelHitbox : ModProjectile
	{
		public override string Texture => AssetDirectory.Assets + "Invisible";

		private Player Player => Main.player[Projectile.owner];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Cogwheel");
		}

		public override void SetDefaults()
		{
			Projectile.width = 34;
			Projectile.height = 34;
			Projectile.friendly = true;
			Projectile.DamageType = DamageClass.Melee;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 700;
			Projectile.tileCollide = false;
			Projectile.hide = true;
		}

        public override void AI()
        {
			CogwheelPlayer modPlayer = Player.GetModPlayer<CogwheelPlayer>();

			if (modPlayer.mounted && !Player.dead)
			{
				Projectile.Center = Player.Bottom - new Vector2(0, 17);
			}
			else
				Projectile.active = false;
        }

        public override bool? CanHitNPC(NPC target)
        {
			if (Player.velocity.Length() < 1)
				return false;
            return base.CanHitNPC(target);
        }

        public override void ModifyHitNPC(NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			hitDirection = Math.Sign(Player.direction);
			damage = (int)(damage * MathHelper.Lerp(0.4f, 1.6f, MathHelper.Min(11, Player.velocity.Length()) / 11f));
            base.ModifyHitNPC(target, ref damage, ref knockback, ref crit, ref hitDirection);
        }
    }
}
