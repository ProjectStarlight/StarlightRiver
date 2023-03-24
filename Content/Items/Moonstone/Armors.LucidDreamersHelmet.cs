using Terraria.ID;
using Terraria;
using System.Linq;
using System;

namespace StarlightRiver.Content.Items.Moonstone
{
	[AutoloadEquip(EquipType.Head)]
	public class LucidDreamersHelmet : ModItem
	{
		public override string Texture => AssetDirectory.MoonstoneItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Lucid Dreamer's Helmet");
			Tooltip.SetDefault("Blue spirit arms will fight for you\n'You have become a superior being'");
		}

		public override void SetDefaults()
		{
			Item.width = 28;
			Item.height = 28;

			Item.value = Item.sellPrice(silver: 3);

			Item.defense = 4;
			Item.rare = ItemRarityID.Blue;
		}

		public override void UpdateEquip(Player player)
		{
			if (player.ownedProjectileCounts[ModContent.ProjectileType<LucidDreamerSpiritArms>()] <= 0)
			{
				Projectile.NewProjectile(player.GetSource_FromThis(), player.Center, Main.rand.NextVector2Circular(1f, 1f), ModContent.ProjectileType<LucidDreamerSpiritArms>(), 20, 1f, player.whoAmI);

				Projectile proj = Projectile.NewProjectileDirect(player.GetSource_FromThis(), player.Center, Main.rand.NextVector2Circular(1f, 1f), ModContent.ProjectileType<LucidDreamerSpiritArms>(), 20, 1f, player.whoAmI);
				(proj.ModProjectile as LucidDreamerSpiritArms).isSecondArm = true;
			}

			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];

				if (proj.active && proj.type == ModContent.ProjectileType<LucidDreamerSpiritArms>() && proj.owner == player.whoAmI)
					proj.timeLeft = 2;
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<MoonstoneBarItem>(), 5);
			recipe.AddIngredient(ItemID.DivingHelmet);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	class LucidDreamerSpiritArms : ModProjectile
	{
		public bool isSecondArm;

		public Vector2? OwnerMouse
		{
			get 
			{ 
				if (Main.myPlayer == Projectile.owner) 
					return Main.MouseWorld; 
				
				return null; 
			}
		}

		public NPC Target => Main.npc.Where(n => n.CanBeChasedBy() && n.Distance(Owner.Center) < 150f).OrderBy(n => n.Distance(OwnerMouse.Value)).FirstOrDefault();

		public Player Owner => Main.player[Projectile.owner];

		public override string Texture => AssetDirectory.Invisible;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Spirit Arm");
		}

		public override void SetDefaults()
		{
			Projectile.Size = new(10);

			Projectile.tileCollide = false;
			Projectile.friendly = true;
			Projectile.hostile = false;

			Projectile.DamageType = DamageClass.Generic;

			Projectile.penetrate = -1;
		}

		public override void AI()
		{
			//Projectile.velocity += new Vector2(0f, (float)Math.Sin(Main.GlobalTimeWrappedHourly) * (isSecondArm ? -0.1f : 0.1f));


			Vector2 idlePos = Owner.Center + Owner.DirectionTo(OwnerMouse.Value) * 50f;
;

			Vector2 toIdlePos = idlePos - Projectile.Center;
			if (toIdlePos.Length() < 0.0001f)
			{
				toIdlePos = Vector2.Zero;
			}
			else
			{
				float speed = Vector2.Distance(idlePos, Projectile.Center) * 0.2f;
				speed = Utils.Clamp(speed, 5f, 25f);
				toIdlePos.Normalize();
				toIdlePos *= speed;
			}

			Projectile.velocity = (Projectile.velocity * (45f - 1) + toIdlePos) / 45f;

			if (Vector2.Distance(Projectile.Center, idlePos) > 1000f)
			{
				Projectile.Center = idlePos;
				Projectile.velocity = Vector2.Zero;
				Projectile.netUpdate = true;
			}

			Dust.NewDustPerfect(Projectile.Center, DustID.Torch).noGravity = true;
		}
	}
}
