using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Food.Special
{
	internal class ShrimpSandwich : BonusIngredient
	{
		public ShrimpSandwich() : base("Ability to breathe and swim underwater.\nUnderwater enemies have a chance to drop random fish.\n\"It's just that shrimple\"") { }

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
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 50);
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
		public bool Active = false;

		//reusing this modplayer since its related and so im not making one just for 1 method
		public override void AnglerQuestReward(float rarityReduction, List<Item> rewardItems)
		{
			if (rarityReduction > 0.45f && Main.rand.NextBool(4, 5))//skip if the reward is too rare, or at random sometimes
			{
				var shrimp = new Item();
				shrimp.SetDefaults(ModContent.ItemType<JumboShrimp>());
				shrimp.stack = Main.rand.Next(2, 6);//copied fish potion chances
				rewardItems.Add(shrimp);
			}
		}

		public override void ResetEffects()
		{
			Active = false;
		}
	}

	public class ShrimpSandwhichDropsNPC : GlobalNPC
	{
		private static MethodInfo RollFishingDropInfo;
		public delegate void RollFishingDropDelegate(Projectile proj, ref FishingAttempt fisher);
		public static RollFishingDropDelegate RollFishingDrop;

		public override void Load()
		{
			RollFishingDropInfo = typeof(Terraria.Projectile).GetMethod("FishingCheck_RollItemDrop", BindingFlags.NonPublic | BindingFlags.Instance);
			RollFishingDrop = (RollFishingDropDelegate)Delegate.CreateDelegate(typeof(RollFishingDropDelegate), RollFishingDropInfo);
		}

		public override void Unload()
		{
			RollFishingDrop = null;
		}

		public override void ModifyGlobalLoot(GlobalLoot globalLoot)
		{
			var leadingConditionRule = new LeadingConditionRule(new ShrimpSandwhichDropsCondition());
			leadingConditionRule.OnSuccess(new FishDrop(8));
			globalLoot.Add(leadingConditionRule);
			base.ModifyGlobalLoot(globalLoot);
		}
	}

	public class FishDrop : IItemDropRule
	{
		public int chanceDenominator;
		public int chanceNumerator;

		public List<IItemDropRuleChainAttempt> ChainedRules { get; private set; }

		public FishDrop(int chanceDenominator, int chanceNumerator = 1)
		{
			this.chanceDenominator = chanceDenominator;
			this.chanceNumerator = chanceNumerator;
			ChainedRules = new();
		}

		public virtual bool CanDrop(DropAttemptInfo info)
		{
			return true;
		}

		//check copied from FishingCheck() in projectile.cs
		private int GetHeightTeir(int TilePositionY)
		{
			if (TilePositionY < Main.worldSurface * 0.5)
			{
				return 0;//space
			}
			else if (TilePositionY < Main.worldSurface)
			{
				return 1;//overworld
			}
			else if (TilePositionY < Main.rockLayer)
			{
				return 2;//dirt layer
			}
			else if (TilePositionY < Main.maxTilesY - 300)
			{
				return 3;//rock layer
			}
			else
			{
				return 4;//underworld
			}
		}

		//modifed from FishingCheck_RollDropLevels() in projectile.cs, removed crate logic
		private void RollDropLevels(int fishingLevel, out bool common, out bool uncommon, out bool rare, out bool veryrare, out bool legendary)
		{
			int num = 150 / fishingLevel;
			int num2 = 300 / fishingLevel;
			int num3 = 1050 / fishingLevel;
			int num4 = 2250 / fishingLevel;
			int num5 = 4500 / fishingLevel;

			if (num < 2)
				num = 2;

			if (num2 < 3)
				num2 = 3;

			if (num3 < 4)
				num3 = 4;

			if (num4 < 5)
				num4 = 5;

			if (num5 < 6)
				num5 = 6;

			common = false;
			uncommon = false;
			rare = false;
			veryrare = false;
			legendary = false;

			if (Main.rand.NextBool(num))
				common = true;

			if (Main.rand.NextBool(num2))
				uncommon = true;

			if (Main.rand.NextBool(num3))
				rare = true;

			if (Main.rand.NextBool(num4))
				veryrare = true;

			if (Main.rand.NextBool(num5))
				legendary = true;
		}

		//modifier from AI_061_FishingBobber_GiveItemToPlayer() in projectile.cs
		private int GetFishStack(int fishingLevel, int itemType)
		{
			int stack = 1;
			//bomb fish
			if (itemType == 3196)
			{
				int finalFishingLevel = fishingLevel;
				int minValue = (finalFishingLevel / 20 + 3) / 2;
				int num = (finalFishingLevel / 10 + 6) / 2;
				if (Main.rand.Next(50) < finalFishingLevel)
					num++;

				if (Main.rand.Next(100) < finalFishingLevel)
					num++;

				if (Main.rand.Next(150) < finalFishingLevel)
					num++;

				if (Main.rand.Next(200) < finalFishingLevel)
					num++;

				stack = Main.rand.Next(minValue, num + 1);
			}
			//frost daggerfish
			if (itemType == 3197)//frost daggerfish
			{
				int finalFishingLevel2 = fishingLevel;
				int minValue2 = (finalFishingLevel2 / 4 + 15) / 2;
				int num2 = (finalFishingLevel2 / 2 + 30) / 2;
				if (Main.rand.Next(50) < finalFishingLevel2)
					num2 += 4;

				if (Main.rand.Next(100) < finalFishingLevel2)
					num2 += 4;

				if (Main.rand.Next(150) < finalFishingLevel2)
					num2 += 4;

				if (Main.rand.Next(200) < finalFishingLevel2)
					num2 += 4;

				stack = Main.rand.Next(minValue2, num2 + 1);
			}

			return stack;
		}

		public virtual ItemDropAttemptResult TryDroppingItem(DropAttemptInfo info)
		{
			ItemDropAttemptResult result;
			if (info.player.RollLuck(chanceDenominator) < chanceNumerator)
			{
				const int BaseFishingLevel = 15;
				int FishingLevel = BaseFishingLevel + info.player.fishingSkill + Math.Min((int)(info.npc.value / (Main.expertMode ? 200f : 100f)), 1000);
				int playerPosY = (int)(info.player.position.Y / 16);
				RollDropLevels(FishingLevel, out bool common, out bool uncommon, out bool rare, out bool veryrare, out bool legendary);
				//Main.NewText("fishing level: " + FishingLevel, Color.Gold);
				var FishingAttemptData =
				new FishingAttempt()
				{
					X = (int)(info.npc.position.X / 16),
					Y = (int)(info.npc.position.Y / 16),
					CanFishInLava = Main.hardMode || info.player.accLavaFishing,
					inLava = info.npc.lavaWet,
					inHoney = info.npc.honeyWet,
					common = common,
					uncommon = uncommon,
					rare = rare,
					veryrare = veryrare,
					legendary = legendary,//30%~ chance for pet/accessory items
					crate = false,
					waterTilesCount = 5000,//ignores water body size, prob best for performance anyway
					waterNeededToFish = 300,
					fishingLevel = FishingLevel,//only used for junk item checks, does not give junk if water size is larger than above number, or this is above 50
					heightLevel = GetHeightTeir((int)(info.player.position.Y / 16))
				};

				//Main.NewText("Height teir: " + GetHeightTeir((int)(info.player.position.Y / 16)), Color.SkyBlue);
				//projecile just to pass into the roll fishing drop method, the proj is never used
				var dummyProjectile = new Projectile() { position = info.npc.position };
				ShrimpSandwhichDropsNPC.RollFishingDrop(dummyProjectile, ref FishingAttemptData);

				int itemdrop = FishingAttemptData.rolledItemDrop;
				int itemstack = GetFishStack(FishingLevel, itemdrop);//increase the stack size if this is bombfish or frost daggerfish

				//drop copied from CommonCode.DropItem, which vanilla IItemDropRules use
				Rectangle npcHitbox = info.npc.Hitbox;
				int itemindex = Item.NewItem(
					info.npc.GetSource_Loot(),
					new Vector2(npcHitbox.X + npcHitbox.Width / 2f, npcHitbox.Y + npcHitbox.Height / 2f),
					itemdrop,
					itemstack,// + info.rng.Next(amountDroppedMinimum, amountDroppedMaximum + 1),
					noBroadcast: false,
					-1);

				//allow mods to modify the caught fish
				PlayerLoader.ModifyCaughtFish(info.player, Main.item[itemindex]);
				ItemLoader.CaughtFishStack(Main.item[itemindex]);

				if (Main.netMode == NetmodeID.MultiplayerClient)
					NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemindex, 1f);

				//changes the item color if this is gel or sharkfin, likely not needed but just in case a mod adds either to the fishing loot pool, and this is run by CommonCode.DropItem anyway
				CommonCode.ModifyItemDropFromNPC(info.npc, itemindex);

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

	//if we have a reason to have any underwater npc drop stuff other than fish, this should be made to only check npc.wet, 
	//and the shrimp check moved to its own condition
	public class ShrimpSandwhichDropsCondition : IItemDropRuleCondition
	{
		public bool CanDrop(DropAttemptInfo info)
		{
			return !info.npc.SpawnedFromStatue && (info.npc.wet || info.npc.lavaWet || info.npc.honeyWet) && info.player.GetModPlayer<ShrimpSandwhichPlayer>().Active;
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
}