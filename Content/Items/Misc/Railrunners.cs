using Microsoft.Xna.Framework;
using StarlightRiver;
using Terraria.ID;
using Terraria.ModLoader;
using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Content.Tiles.Misc;
using StarlightRiver.Core.Systems.BlockerTileSystem;

namespace StarlightRiver.Content.Items.Misc
{
	public class RailrunnersItem : ModItem
	{
		public override string Texture => AssetDirectory.Debug;
		public override void Load()
		{
			On_Main.Update += UpdateCollision;
		}

		public override void Unload()
		{
			On_Main.Update -= UpdateCollision;
		}

		private void UpdateCollision(On_Main.orig_Update orig, Main self, GameTime gameTime)
		{
			orig(self, gameTime);

			if (Main.gameMenu && Main.netMode != NetmodeID.Server)
				return;

			// Hardcoded for testing. may need to come up with another solution since the intersections act strange with this and player hovers above the rails
			TileID.Sets.Platforms[TileID.MinecartTrack] = true;
			Main.tileSolidTop[TileID.MinecartTrack] = true;
			Main.tileSolid[TileID.MinecartTrack] = true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Railrunners");
			Tooltip.SetDefault("Treat minecart tracks like platforms. Maintain horizontal speed when mounting.");
		}

		public override void SetDefaults()
		{
			Item.width = 20;
			Item.height = 30;
			Item.value = 30000;
			Item.rare = ItemRarityID.Green;
			Item.mountType = ModContent.MountType<RailrunnersMount>();
		}
	}

	public class RailrunnersMount : ModMount
	{
		public override string Texture => AssetDirectory.Debug;

		public Vector2 storedVelocity = Vector2.Zero;

		public override void Load()
		{
			On_Player.LaunchMinecartHook += SnapshotVelocity;
		}

		public override void Unload()
		{
			On_Player.LaunchMinecartHook -= SnapshotVelocity;
		}

		public override void SetStaticDefaults()
		{
			// Mostly just ExampleMinecart assignments here

			// What separates mounts and minecarts are these 3 lines
			MountData.Minecart = true;
			// This makes the minecarts item autoequip in the minecart slot
			MountID.Sets.Cart[ModContent.MountType<RailrunnersMount>()] = true;
			// The specified method takes care of spawning dust when stopping or jumping. Use DelegateMethods.Minecart.Sparks for normal sparks.
			MountData.spawnDust = DustID.SparksMech;

			MountData.spawnDust = 16;
			MountData.buff = ModContent.BuffType<RailrunnerBuff>();

			// Movement fields:
			MountData.flightTimeMax = 0; // always set flight time to 0 for minecarts
			MountData.fallDamage = 1f; // how much fall damage will the player take in the minecart
			MountData.runSpeed = 10f; // how fast can the minecart go
			MountData.acceleration = 0.04f; // how fast does the minecart accelerate
			MountData.jumpHeight = 15; // how far does the minecart jump
			MountData.jumpSpeed = 5.15f; // how fast does the minecart jump
			MountData.blockExtraJumps = true; // Can the player not use a could in a bottle when in the minecart?
			MountData.heightBoost = 12;

			// Drawing fields:
			MountData.playerYOffsets = new int[] { 9, 9, 9 }; // where is the players Y position on the mount for each frame of animation
			MountData.xOffset = 2; // the X offset of the minecarts sprite
			MountData.yOffset = 9; // the Y offset of the minecarts sprite
			MountData.bodyFrame = 3; // which body frame is being used from the player when the player is boarded on the minecart
			MountData.playerHeadOffset = 14; // Affects where the player head is drawn on the map

			// Animation fields: The following is the mount animation values shared by vanilla minecarts. It can be edited if you know what you are doing.
			MountData.totalFrames = 3;
			MountData.standingFrameCount = 1;
			MountData.standingFrameDelay = 12;
			MountData.standingFrameStart = 0;
			MountData.runningFrameCount = 3;
			MountData.runningFrameDelay = 12;
			MountData.runningFrameStart = 0;
			MountData.flyingFrameCount = 0;
			MountData.flyingFrameDelay = 0;
			MountData.flyingFrameStart = 0;
			MountData.inAirFrameCount = 0;
			MountData.inAirFrameDelay = 0;
			MountData.inAirFrameStart = 0;
			MountData.idleFrameCount = 1;
			MountData.idleFrameDelay = 10;
			MountData.idleFrameStart = 0;
			MountData.idleFrameLoop = false;
		}

		/// <summary>
		/// Right Before firing the minecart hook that locks player velocity to zero we'll capture the players velocity to re-assign after they mount
		/// </summary>
		/// <param name=""></param>
		public void SnapshotVelocity(On_Player.orig_LaunchMinecartHook orig, Player self, int myX, int myY)
		{
			storedVelocity = self.velocity;
			Main.NewText(self.velocity);
			orig.Invoke(self, myX, myY);
		}

		public override void SetMount(Player player, ref bool skipDust)
		{
			player.velocity = storedVelocity;
		}

		public override void Dismount(Player player, ref bool skipDust)
		{
			player.velocity += new Vector2(0, -3f); // Give player a little upwards velocity on dismount to not fall off the rails immediately
		}
	}
	public class RailrunnerBuff : Buffs.SmartBuff
    {
		public override string Texture => AssetDirectory.Debug;

		public RailrunnerBuff() : base("Railrunners", "Riding the rails", false, true) { }

		public override void SafeSetDefaults()
		{
			Main.buffNoTimeDisplay[Type] = true;
			Main.buffNoSave[Type] = true;
		}

		public override void Update(Player Player, ref int buffIndex)
		{
			Player.mount.SetMount(ModContent.MountType<RailrunnersMount>(), Player);
			Player.buffTime[buffIndex] = 10;
		}

	}
}
