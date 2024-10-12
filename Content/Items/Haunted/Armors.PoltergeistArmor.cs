using Mono.Cecil;
using StarlightRiver.Content.Items.Gravedigger;
using System.Collections.Generic;
using System.Linq;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Haunted
{
	[AutoloadEquip(EquipType.Head)]
	public class PoltergeistHead : ModItem
	{
		public List<Projectile> minions = new();
		public int timer;
		public int sleepTimer;
		public (string singular, string plural)[] minionBoredomMessages =
		{
			("Your _ seems bored...", "Your _ seem bored..."),
			("Your _ lost interest...", "Your _ lost interest..."),
			("Your _ is hanging limply...", "Your _ are hanging limply..."),
			("Looks like your _ isn't feeling it...", "Looks like your _ aren't feeling it..."),
			("Your _ is clearly unimpressed...", "Your _ are clearly unimpressed..."),
			("It seems the magic is just gone for your _...", "It seems the magic is just gone for your _..."),
			("Looks like your _ is totally checked out...", "Looks like your _ are totally checked out..."),
			("Your _ is hovering aimlessly...", "Your _ are hovering aimlessly..."),
		};

		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void Load()
		{
			On_Player.KeyDoubleTap += HauntItem;
			StarlightItem.CanUseItemEvent += ControlItemUse;
		}

		public override void Unload()
		{
			On_Player.KeyDoubleTap -= HauntItem;
			StarlightItem.CanUseItemEvent -= ControlItemUse;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunting Hood");
			Tooltip.SetDefault("15% increased magic critical strike damage");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 5;
		}

		public override ModItem Clone(Item newEntity)
		{
			var clone = base.Clone(newEntity) as PoltergeistHead;
			clone.minions = new List<Projectile>(minions);

			return clone;
		}

		public override void UpdateEquip(Player player)
		{
			player.GetModPlayer<CritMultiPlayer>().MagicCritMult += 0.15f;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ItemType<PoltergeistChest>() && legs.type == ItemType<PoltergeistLegs>();
		}

		public override void UpdateArmorSet(Player player)
		{
			player.setBonus =
				"Double tap DOWN with a magic weapon to haunt or unhaunt it, or with an empty hand to unhaunt all\n" +
				"Haunted weapons float around you and attack automatically, but {{Reserve}} mana\n" +
				"Haunted weapons become disinterested in non-magic users and can't be used while haunted";

			minions.RemoveAll(n => !n.active || n.type != ProjectileType<PoltergeistMinion>());

			timer++;

			for (int k = 0; k < minions.Count; k++)
			{
				var proj = minions[k].ModProjectile as PoltergeistMinion;
				player.GetModPlayer<ResourceReservationPlayer>().ReserveMana((int)(proj.Item.mana * (60f / proj.Item.useTime) * 2 + 40));
			}

			if (player == Main.LocalPlayer && sleepTimer == 1 && minions.Count > 0) //warning message
			{
				AdvancedPopupRequest request = default;
				(string messageSingular, string messagePlural) = minionBoredomMessages[Main.rand.Next(minionBoredomMessages.Length)];
				request.Text = messagePlural.Replace("_", "haunted weapons");
				if (minions.Count == 1)
					request.Text = messageSingular.Replace("_", (minions.First().ModProjectile as PoltergeistMinion).Item.Name);
				request.DurationInFrames = 180;
				request.Color = new Color(200, 120, 255);
				request.Velocity = new Vector2(0f, -4f);
				PopupText.NewText(request, player.Top);
			}

			if (sleepTimer > 0) //decrement sleep timer
				sleepTimer--;

		}

		private void HauntItem(On_Player.orig_KeyDoubleTap orig, Player player, int keyDir)
		{
			if (keyDir == 0 && player.armor[0].type == ItemType<PoltergeistHead>())
			{
				Item item = player.HeldItem;
				var helm = player.armor[0].ModItem as PoltergeistHead;
				ResourceReservationPlayer mp = player.GetModPlayer<ResourceReservationPlayer>();

				if (item.IsAir) //clear from empty hand
				{
					helm.minions.Clear();
					orig(player, keyDir);
					return;
				}

				if (helm.minions.Any(n => (n.ModProjectile as PoltergeistMinion).Item.type == item.type)) //removal
				{
					helm.minions.RemoveAll(n => (n.ModProjectile as PoltergeistMinion).Item.type == item.type);
					orig(player, keyDir);
					return;
				}

				if (item.DamageType.Type == DamageClass.Magic.Type && item.mana > 0 && !item.channel && item.shoot != ProjectileID.None && mp.TryReserveMana((int)(item.mana * (60f / item.useTime) * 2))) //addition
				{
					int i = Projectile.NewProjectile(player.GetSource_ItemUse(Item), player.Center, Vector2.Zero, ProjectileType<PoltergeistMinion>(), 0, 0, player.whoAmI);
					Projectile proj = Main.projectile[i];
					(proj.ModProjectile as PoltergeistMinion).Item = item.Clone();

					helm.minions.Add(proj);
					helm.sleepTimer = 1200;
					SoundEngine.PlaySound(item.UseSound.Value with { Pitch = item.UseSound.Value.Pitch - 0.3f});
				}
			}

			orig(player, keyDir);
		}

		private bool ControlItemUse(Item item, Player player)
		{
			if (player.armor[0].type == ItemType<PoltergeistHead>())
			{
				var helm = player.armor[0].ModItem as PoltergeistHead;

				if (helm.minions.Any(n => (n.ModProjectile as PoltergeistMinion)?.Item?.type == item.type))
					return false;

				if (item.DamageType.Type == DamageClass.Magic.Type)
					helm.sleepTimer = 1200;
			}

			return true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Silk, 14);
			recipe.AddIngredient(ItemType<VengefulSpirit>(), 7);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Silk, 14);
			recipe.AddIngredient(ItemType<VengefulSpirit>(), 7);
			recipe.AddTile(TileID.Loom);
			recipe.Register();
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class PoltergeistChest : ModItem
	{
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunting Breastplate");
			Tooltip.SetDefault("5% increased magic damage\n+15% {{Inoculation}}");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 6;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.GetDamage(DamageClass.Magic) += 0.05f;
			Player.GetModPlayer<DoTResistancePlayer>().DoTResist += 0.15f;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Silk, 16);
			recipe.AddIngredient(ItemType<VengefulSpirit>(), 8);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Silk, 16);
			recipe.AddIngredient(ItemType<VengefulSpirit>(), 8);
			recipe.AddTile(TileID.Loom);
			recipe.Register();
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	public class PoltergeistLegs : ModItem
	{
		public override string Texture => AssetDirectory.HauntedItem + Name;

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Haunting Robes");
			Tooltip.SetDefault("+40 maximum mana");
		}

		public override void SetDefaults()
		{
			Item.width = 18;
			Item.height = 18;
			Item.value = 1;
			Item.rare = ItemRarityID.Green;
			Item.defense = 5;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.statManaMax2 += 40;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Silk, 12);
			recipe.AddIngredient(ItemType<VengefulSpirit>(), 6);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Silk, 12);
			recipe.AddIngredient(ItemType<VengefulSpirit>(), 6);
			recipe.AddTile(TileID.Loom);
			recipe.Register();
		}
	}
}