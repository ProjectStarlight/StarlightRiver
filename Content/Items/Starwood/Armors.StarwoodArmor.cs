﻿using StarlightRiver.Content.Items;
using StarlightRiver.Content.Items.Vanity;
using StarlightRiver.Content.Packets;
using System;
using System.Linq;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Starwood
{
	[AutoloadEquip(EquipType.Head)]
	public class StarwoodHat : StarwoodItem, IArmorLayerDrawable
	{
		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public StarwoodHat() : base(Assets.Items.Starwood.StarwoodHat_Alt.Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starwood Hat");
			Tooltip.SetDefault("5% increased magic damage");

			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<AncientStarwoodHat>();
		}

		public override void SetDefaults()
		{
			Item.width = 42;
			Item.height = 26;
			Item.value = Item.sellPrice(0, 0, 10, 0);
			Item.defense = 2;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.GetDamage(DamageClass.Magic) += 0.05f;
			isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;
		}

		public override void UpdateVanity(Player Player)
		{
			isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;
		}

		public void DrawArmorLayer(PlayerDrawSet info, IArmorLayerDrawable.SubLayer subLayer)
		{
			if (info.drawPlayer.GetModPlayer<StarlightPlayer>().empowered)
				ArmorHelper.QuickDrawHeadFramed(info, AssetDirectory.StarwoodItem + "StarwoodHat_Head_Alt", 1, new Vector2(0, -3));
		}
	}

	[AutoloadEquip(EquipType.Body)]
	public class StarwoodChest : StarwoodItem, IArmorLayerDrawable
	{
		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public StarwoodChest() : base(Assets.Items.Starwood.StarwoodChest_Alt.Value) { }

		public override void Load()//adds method to Starlight Player event
		{
			StarlightPlayer.OnHitNPCEvent += ModifyHitNPCStarwood;
			StarlightPlayer.OnHitNPCWithProjEvent += ModifyHitNPCWithProjStarwood;
			StarlightPlayer.ResetEffectsEvent += ResetEmpowerment;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starwood Robes");
			Tooltip.SetDefault("Increases max mana by 20");

			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<AncientStarwoodChest>();
		}

		public override void SetDefaults()
		{
			Item.width = 38;
			Item.height = 30;
			Item.value = Item.sellPrice(0, 0, 12, 0);
			Item.defense = 3;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.statManaMax2 += 20;
			isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;
		}

		public override void UpdateVanity(Player Player)
		{
			isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;
		}
		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return head.type == ItemType<StarwoodHat>() &&
				legs.type == ItemType<StarwoodBoots>();
		}

		public override void UpdateArmorSet(Player Player)
		{
			Player.setBonus = "Picking up mana stars empowers Starwood items, temporarily granting them new effects\nAll enemies have a chance of dropping mana stars on death";
			StarlightPlayer mp = Player.GetModPlayer<StarlightPlayer>();

			if (mp.empowermentTimer > 0 && ArmorHelper.IsSetEquipped(this, Player)) //checks if complete to disable empowerment if set is removed
			{
				for (int k = 0; k < 1; k++)//temp gfx 
					Dust.NewDustPerfect(Player.position + new Vector2(Main.rand.Next(Player.width), Main.rand.Next(Player.height)), DustType<Dusts.BlueStamina>(), -Vector2.UnitY.RotatedByRandom(0.8f) * Main.rand.NextFloat(1.0f, 1.4f), 0, default, 1.2f);

				mp.empowermentTimer--;
			}
			else
			{
				mp.empowermentTimer = 0;
				mp.empowered = false;
			}
		}

		private void ModifyHitNPCStarwood(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)//sets bool on hit NPCs
		{
			if (ArmorHelper.IsSetEquipped(this, Player))
			{
				target.GetGlobalNPC<ManastarDrops>().DropStar = true;
				Player.GetModPlayer<StarlightPlayer>().SetHitPacketStatus(shouldRunProjMethods: false);
			}
		}

		private void ModifyHitNPCWithProjStarwood(Player Player, Projectile proj, NPC target, NPC.HitInfo hit, int damageDone)
		{
			if (ArmorHelper.IsSetEquipped(this, Player))
			{
				target.GetGlobalNPC<ManastarDrops>().DropStar = true;
				Player.GetModPlayer<StarlightPlayer>().SetHitPacketStatus(shouldRunProjMethods: false);
			}
		}

		private void ResetEmpowerment(StarlightPlayer modPlayer)
		{
			Player player = modPlayer.Player;

			if (!(player.armor[1].ModItem is Starwood.StarwoodChest) || !ArmorHelper.IsSetEquipped(player.armor[1].ModItem, player)) // Checks armor set is off since it does not seem to be called in armor when the set is not complete
			{
				modPlayer.empowered = false;
				modPlayer.empowermentTimer = 0;
			}
		}

		public void DrawArmorLayer(PlayerDrawSet info, IArmorLayerDrawable.SubLayer subLayer)
		{
			if (subLayer == IArmorLayerDrawable.SubLayer.InFront)
			{
				if (info.drawPlayer.GetModPlayer<StarlightPlayer>().empowered)
					ArmorHelper.QuickDrawFrontArmsFramed(info, AssetDirectory.StarwoodItem + "StarwoodChest_Body_Alt", 1, new Vector2(0, -5));
			}
			else
			{
				if (info.drawPlayer.GetModPlayer<StarlightPlayer>().empowered)
					ArmorHelper.QuickDrawBodyFramed(info, AssetDirectory.StarwoodItem + "StarwoodChest_Body_Alt", 1, new Vector2(0, -5));
			}
		}
	}

	[AutoloadEquip(EquipType.Legs)]
	public class StarwoodBoots : StarwoodItem, IArmorLayerDrawable
	{
		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public StarwoodBoots() : base(Assets.Items.Starwood.StarwoodBoots_Alt.Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Starwood Leggings");
			Tooltip.SetDefault("5% increased magic critial strike chance");

			ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<AncientStarwoodBoots>();
		}

		public override void SetDefaults()
		{
			Item.width = 30;
			Item.height = 18;
			Item.value = Item.sellPrice(0, 0, 8, 0);
			Item.defense = 2;
		}

		public override void UpdateEquip(Player Player)
		{
			Player.GetCritChance(DamageClass.Magic) += 5;
			isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;
		}

		public override void UpdateVanity(Player Player)
		{
			isEmpowered = Player.GetModPlayer<StarlightPlayer>().empowered;
		}

		public void DrawArmorLayer(PlayerDrawSet info, IArmorLayerDrawable.SubLayer subLayer)
		{
			if (info.drawPlayer.GetModPlayer<StarlightPlayer>().empowered)
				ArmorHelper.QuickDrawLegsFramed(info, AssetDirectory.StarwoodItem + "StarwoodBoots_Legs_Alt", 1, new Vector2(10, 18));
		}
	}

	//makes star Items start starwood empowerment, starting it just doesn't do anything is the Player does not have the set equiped
	internal class ManastarPickup : GlobalItem
	{
		public override bool OnPickup(Item Item, Player Player)
		{
			//clientside
			if (new int[] { ItemID.Star, ItemID.SoulCake, ItemID.SugarPlum }.Contains(Item.type))
			{
				StarlightPlayer mp = Player.GetModPlayer<StarlightPlayer>();
				mp.StartStarwoodEmpowerment();
			}

			return base.OnPickup(Item, Player);
		}
	}

	//makes NPCs drop stars if the bool the armor-set sets is true
	internal class ManastarDrops : GlobalNPC
	{
		public bool DropStar = false;

		public override bool InstancePerEntity => true;

		public override void OnKill(NPC npc)
		{
			if (DropStar && Main.rand.NextBool(2))
				Item.NewItem(npc.GetSource_Loot(), npc.Center, ItemID.Star);
		}
	}
}

//modPlayer to handle empowerment

namespace StarlightRiver.Core
{
	public partial class StarlightPlayer : ModPlayer
	{
		public bool empowered = false;
		public int empowermentTimer = 0;

		public void StartStarwoodEmpowerment()
		{
			if (Player.armor[1].ModItem is Content.Items.Starwood.StarwoodChest && ArmorHelper.IsSetEquipped(Player.armor[1].ModItem, Player))//checks if complete, not completely needed but is there so empowered isnt true for a brief moment
			{
				StarwoodEmpowermentPacket packet = new StarwoodEmpowermentPacket(Player.whoAmI);
				packet.Send();
			}
		}
	}
}