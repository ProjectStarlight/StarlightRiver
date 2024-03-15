using System.Collections.Generic;
using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.DataStructures;
using System.Reflection;

namespace StarlightRiver.Content.Items.Food.Special
{
	internal class ShrimpSandwich : BonusIngredient
	{
		public ShrimpSandwich() : base("Ability to breathe and swim underwater.\nUnderwater enemies have a chance to drop random fish.\n\"It's just that shrimple\"\nWip") { }

		public override FoodRecipie Recipie()
		{
			return new(
			Type,
			ModContent.ItemType<Toast>(),
			ModContent.ItemType<JumboShrimp>(),
			ModContent.ItemType<Lettuce>(),
			ModContent.ItemType<Dressing>()
			);
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 5);
		}

		public override void BuffEffects(Player Player, float multiplier)
		{
			Player.accFlipper = true;
			Player.gills = true;
			Player.GetModPlayer<ShrimpSandwhichPlayer>().Active = true;
		}
	}

	public class ShrimpSandwhichPlayer : ModPlayer
	{
		public static MethodInfo RollFishingDropInfo;

		public override void Load()
		{
			RollFishingDropInfo = typeof(Terraria.Projectile).GetMethod("FishingCheck_RollItemDrop", BindingFlags.NonPublic | BindingFlags.Instance);
		}

		public override void Unload()
		{
			RollFishingDropInfo = null;
		}

		public bool Active = false;

		public override void ResetEffects()
		{
			Active = false;
		}
	}

	public class ShrimpSandwhichDropsNPC : GlobalNPC
	{
		public override void ModifyGlobalLoot(GlobalLoot globalLoot)
		{
			LeadingConditionRule leadingConditionRule = new LeadingConditionRule(new ShrimpSandwhichFishDrops());
			leadingConditionRule.OnSuccess(new FishDrop(1));
			globalLoot.Add(leadingConditionRule);
			base.ModifyGlobalLoot(globalLoot);
		}
	}

	//condition for if the npc should drop fish
	//if we have a reason to have any underwater npc drop stuff other than fish, this should be made to only check npc.wet, 
	//and the shrimp check moved to its own condition
	public class ShrimpSandwhichFishDrops : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			return (info.npc.wet || info.npc.lavaWet || info.npc.honeyWet) && info.player.GetModPlayer<ShrimpSandwhichPlayer>().Active;
		}

		public bool CanShowItemDropInUI()
		{
			return false;
		}

		public string GetConditionDescription()
		{
			return null;//this shouldn't show up in the bestiary, so no text is needed
		}
	}

	public class FishDrop : IItemDropRule
	{
		public int chanceDenominator;
		public int amountDroppedMinimum;
		public int amountDroppedMaximum;
		public int chanceNumerator;

		public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }

		public FishDrop(int chanceDenominator, int amountDroppedMinimum = 1, int amountDroppedMaximum = 1, int chanceNumerator = 1)
		{
			if (amountDroppedMinimum > amountDroppedMaximum)
			{
				throw new ArgumentOutOfRangeException("amountDroppedMinimum", "amountDroppedMinimum must be lesser or equal to amountDroppedMaximum.");
			}

			this.chanceDenominator = chanceDenominator;
			this.amountDroppedMinimum = amountDroppedMinimum;
			this.amountDroppedMaximum = amountDroppedMaximum;
			this.chanceNumerator = chanceNumerator;
			ChainedRules = new List<IItemDropRuleChainAttempt>();
		}

		public virtual bool CanDrop(DropAttemptInfo info)
		{
			return true;
		}

		public virtual ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
		{
			ItemDropAttemptResult result;
			if (info.player.RollLuck(chanceDenominator) < chanceNumerator)
			{
				object[] args = new object[] {
				new FishingAttempt()
				{
					X = (int)(info.npc.position.X / 16),
					Y = (int)(info.npc.position.Y / 16),
					CanFishInLava = true,
					inLava = info.npc.lavaWet,
					inHoney = info.npc.honeyWet,
					common = true,
					uncommon = true,
					rare = true,
					veryrare = false,
					legendary = false,
					crate = false,
					waterTilesCount = 5000,
					waterNeededToFish = 200,
					fishingLevel = 100
					//waterQuality = 1
				}
			};
				ShrimpSandwhichPlayer.RollFishingDropInfo.Invoke(new Projectile(), args);


				CommonCode.DropItem(info, ((FishingAttempt)args[0]).rolledItemDrop, info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1));
				result = default;
				result.State = ItemDropAttemptResultState.Success;
				return result;
			}

			result = default;
			result.State = ItemDropAttemptResultState.FailedRandomRoll;
			return result;
		}

		public virtual void ReportDroprates(List<DropRateInfo> drops, DropRateInfoChainFeed ratesInfo)
		{

		}
	}
}