using Microsoft.Xna.Framework;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Content.Tiles.Forest;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

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
			Item.width = 32;
			Item.height = 32;
			Item.consumable = true;
			Item.maxStack = 30;
			Item.UseSound = SoundID.Item3;
			Item.useStyle = ItemUseStyleID.EatFood;
			Item.useTime = 15;
			Item.useAnimation = 15;
		}

		public override bool CanUseItem(Player Player) => !Player.HasBuff(ModContent.BuffType<NoShieldPot>()) && !Player.HasBuff(BuffID.PotionSickness);

		public override bool? UseItem(Player Player)
		{
			Player.GetModPlayer<ShieldPlayer>().Shield += amount;
			Player.AddBuff(ModContent.BuffType<ShieldDegenReduction>(), duration);
			Player.AddBuff(ModContent.BuffType<NoShieldPot>(), 3600);
			Player.AddBuff(BuffID.PotionSickness, 1200);

			CombatText.NewText(Player.Hitbox, new Color(150, 255, 255), amount);

			return true;
		}
	}

	public class LesserBarrierPotion  : BarrierPotion
	{
		public LesserBarrierPotion() : base(40, 180, "Lesser") { }

		public override void AddRecipes()
		{
			var recipe = new ModRecipe(Mod);
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
			var recipe = new ModRecipe(Mod);
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
			var recipe = new ModRecipe(Mod);
			recipe.AddIngredient(ModContent.ItemType<RegularBarrierPotion>(), 5);
			recipe.AddIngredient(ItemID.SoulofLight);
			recipe.AddIngredient(ItemID.SoulofNight);
			recipe.SetResult(this, 5);
			recipe.AddTile(TileID.Bottles);
			recipe.AddRecipe();

			recipe = new ModRecipe(Mod);
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

		public override string Texture => AssetDirectory.PotionsItem + Name;
	}

	public class ShieldDegenReduction : SmartBuff
	{
		public ShieldDegenReduction() : base("Barrier Affinity", "Barrier sticks to you better", false) { }

		public override string Texture => AssetDirectory.PotionsItem + Name;

		public override void Update(Player Player, ref int buffIndex)
		{
			Player.GetModPlayer<ShieldPlayer>().OvershieldDrainRate -= 50;
		}
	}
}
