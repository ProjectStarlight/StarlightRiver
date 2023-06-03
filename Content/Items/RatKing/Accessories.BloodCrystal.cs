using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Content.Items.Misc;
using System;
using System.Linq;
using Terraria.ID;

namespace StarlightRiver.Content.Items.RatKing
{
	class BloodCrystal : CursedAccessory
	{
		public override string Texture => AssetDirectory.RatKingItem + Name;

		public BloodCrystal() : base(ModContent.Request<Texture2D>(AssetDirectory.RatKingItem + "BloodCrystal").Value) { }

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood Crystal");
			Tooltip.SetDefault("Cursed : Debuffs you inflict are also inflicted onto you \nAll debuffs you inflict stack and last longer");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 3, 0, 0);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.GetModPlayer<BloodCrystalPlayer>().equipped = true;
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient<TarnishedRing>();
			recipe.AddIngredient<LivingBlood>(10);
			recipe.AddRecipeGroup("StarlightRiver:Gems", 10);
			recipe.AddTile(TileID.DemonAltar);
			recipe.Register();
		}
	}

	class BloodCrystalComparer : IOrderedLoadable
	{
		public float Priority => 1.4f;

		public void Load()
		{
			StatusTrackingNPC.buffCompareEffects += buffCompareEffects;
		}

		public void Unload()
		{
			StatusTrackingNPC.buffCompareEffects -= buffCompareEffects;
		}

		private void buffCompareEffects(Player player, NPC NPC, int[] storedBuffs, int[] buffType, int[] storedTimes, int[] buffTime)
		{
			if (player.GetModPlayer<BloodCrystalPlayer>().equipped)
			{
				for (int i = 0; i < 5; i++)
				{
					if (!storedBuffs.Contains(buffType[i]))
					{
						player.AddBuff(buffType[i], buffTime[i] / 4);
						buffTime[i] = (int)(buffTime[i] * 1.25f);
					}
					else
					{
						int storedIndex;
						for (storedIndex = 0; storedIndex < 5; storedIndex++)
						{
							if (storedBuffs[storedIndex] == buffType[i])
								break;
						}

						if (storedTimes[storedIndex] != buffTime[i] && storedTimes[storedIndex] - 1 != buffTime[i])
						{
							buffTime[i] = storedTimes[storedIndex] + (int)(buffTime[i] * 0.25f);
							player.AddBuff(buffType[i], buffTime[i] / 4);
						}
					}
				}
			}
		}
	}

	public class BloodCrystalPlayer : ModPlayer
	{
		public bool equipped = false;

		public override void ResetEffects()
		{
			equipped = false;
		}
	}
}