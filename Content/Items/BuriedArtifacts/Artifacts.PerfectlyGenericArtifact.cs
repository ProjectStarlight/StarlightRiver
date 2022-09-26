using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace StarlightRiver.Content.Items.BuriedArtifacts
{
    public class PerfectlyGenericArtifactItem : ModItem
    {
        public override string Texture => AssetDirectory.Archaeology + "PerfectlyGenericArtifact";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Perfectly Generic Object");
			Tooltip.SetDefault("Summons a Perfectly Generic Pet");
		}

		public override void SetDefaults()
		{
			Item.damage = 0;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.width = 16;
			Item.height = 30;
			Item.UseSound = SoundID.Item2;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.noMelee = true;
			Item.shoot = ModContent.ProjectileType<PerfectlyGenericPet>();
			Item.buffType = ModContent.BuffType<PerfectlyGenericPetBuff>();
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Orange;
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame)
		{
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
				player.AddBuff(Item.buffType, 3600);
		}
	}

	public class PerfectlyGenericPetBuff : ModBuff
	{
		public override string Texture => AssetDirectory.ArtifactItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Perfectly Generic Pet");
			Description.SetDefault("Made of grist");

			Main.buffNoTimeDisplay[Type] = true;
			Main.vanityPet[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex)
		{ 
			player.buffTime[buffIndex] = 18000;

			int projType = ModContent.ProjectileType<PerfectlyGenericPet>();

			// If the player is local, and there hasn't been a pet projectile spawned yet - spawn it.
			if (player.whoAmI == Main.myPlayer && player.ownedProjectileCounts[projType] <= 0)
				Projectile.NewProjectile(player.GetSource_Buff(buffIndex), player.Center, Vector2.Zero, projType, 0, 0f, player.whoAmI);
		}
	}

	public class PerfectlyGenericPet : ModProjectile
	{
		public override string Texture => AssetDirectory.Archaeology + "PerfectlyGenericArtifact";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Perfectly Generic Object");
			Main.projPet[Projectile.type] = true;
		}

		public override void SetDefaults()
        {
            Projectile.CloneDefaults(ProjectileID.ZephyrFish); // Copy the stats of the Zephyr Fish
			AIType = ProjectileID.ZephyrFish;
        }

        public override bool PreAI()
		{
			Player player = Main.player[Projectile.owner];
			player.zephyrfish = false; // Relic from aiType
			return true;
		}

		public override void AI()
		{
			Player player = Main.player[Projectile.owner];

			// Keep the projectile from disappearing as long as the player isn't dead and has the pet buff.
			if (!player.dead && player.HasBuff(ModContent.BuffType<PerfectlyGenericPetBuff>()))
				Projectile.timeLeft = 2;
			else
				Projectile.active = false;

			Projectile.spriteDirection = 1;
			Projectile.rotation = 0f;
			Projectile.frame = 0;
		}
	}
}