using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core;
using Microsoft.Xna.Framework;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Content.Items.Vitric;

namespace StarlightRiver.Content.Items.Potions
{
	public abstract class BarrierPotion : ModItem
	{
		int amount;
		int duration;
		readonly string prefix;

		public override string Texture => AssetDirectory.PotionsItem + Name;

		public BarrierPotion(int amount, int duration, string prefix)
		{
			this.amount = amount;
			this.duration = duration;
			this.prefix = prefix;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault(prefix + " Barrier Potion");
			Tooltip.SetDefault($"Grants {amount} barrier\nGreatly reduces overcharge barrier loss for {duration / 60} seconds");
		}

		public override void SetDefaults()
		{
			item.width = 32;
			item.height = 32;
			item.consumable = true;
			item.maxStack = 30;
			item.UseSound = SoundID.Item3;
			item.useStyle = ItemUseStyleID.EatingUsing;
			item.useTime = 15;
			item.useAnimation = 15;
		}

		public override bool CanUseItem(Player player) => !player.HasBuff(ModContent.BuffType<NoShieldPot>()) && !player.HasBuff(BuffID.PotionSickness);

		public override bool UseItem(Player player)
		{
			player.GetModPlayer<ShieldPlayer>().Shield += amount;
			player.AddBuff(ModContent.BuffType<ShieldDegenReduction>(), duration);
			player.AddBuff(ModContent.BuffType<NoShieldPot>(), 3600);
			player.AddBuff(BuffID.PotionSickness, 1200);

			CombatText.NewText(player.Hitbox, new Color(150, 255, 255), amount);

			return true;
		}
	}

	public class LesserBarrierPotion  : BarrierPotion
	{
		public LesserBarrierPotion() : base(40, 180, "Lesser") { }

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Slimeberry>(), 2);
			recipe.AddIngredient(ItemID.Glass, 5);
			recipe.AddIngredient(ItemID.BottledWater);
			recipe.AddTile(TileID.Bottles);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}

	public class RegularBarrierPotion : BarrierPotion
	{
		public RegularBarrierPotion() : base(80, 240, "") { }

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<LesserBarrierPotion>(), 5);
			recipe.AddIngredient(ModContent.ItemType<VitricOre>(), 2);
			recipe.AddIngredient(ItemID.GlowingMushroom, 2);
			recipe.AddTile(TileID.Bottles);
			recipe.SetResult(this, 5);
			recipe.AddRecipe();
		}
	}

	public class GreaterBarrierPotion : BarrierPotion
	{
		public GreaterBarrierPotion() : base(120, 300, "Greater") { }

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<RegularBarrierPotion>(), 5);
			recipe.AddIngredient(ItemID.SoulofLight);
			recipe.AddIngredient(ItemID.SoulofNight);
			recipe.SetResult(this, 5);
			recipe.AddTile(TileID.Bottles);
			recipe.AddRecipe();

			recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.BottledWater, 5);
			recipe.AddIngredient(ModContent.ItemType<Slimeberry>(), 10);
			recipe.AddIngredient(ItemID.SoulofLight);
			recipe.AddIngredient(ItemID.SoulofNight);
			recipe.SetResult(this, 5);
			recipe.AddTile(TileID.Bottles);
			recipe.AddRecipe();
		}
	}

	public class NoShieldPot : SmartBuff
	{
		public NoShieldPot() : base("Barrier Sickness", "Cannot consume more barrier potions", true ) { }

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.PotionsItem + name;
			return true;
		}
	}

	public class ShieldDegenReduction : SmartBuff
	{
		public ShieldDegenReduction() : base("Barrier Affinity", "Barrier sticks to you better", false) { }

		public override bool Autoload(ref string name, ref string texture)
		{
			texture = AssetDirectory.PotionsItem + name;
			return true;
		}

		public override void Update(Player player, ref int buffIndex)
		{
			player.GetModPlayer<ShieldPlayer>().OvershieldDrainRate -= 50;
		}
	}
}
