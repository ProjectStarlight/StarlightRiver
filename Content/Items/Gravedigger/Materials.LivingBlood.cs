using Terraria.DataStructures;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Gravedigger
{
	public class LivingBlood : QuickMaterial
	{
		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += DropLivingBlood;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Living Blood");
			Tooltip.SetDefault("'Unnaturally pulses with the light of the Blood Moon'");
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(5, 5));
			ItemID.Sets.AnimatesAsSoul[Item.type] = true;

			ItemID.Sets.ItemIconPulse[Item.type] = true;
			ItemID.Sets.ItemNoGravity[Item.type] = true;

		}

		public LivingBlood() : base("Living Blood", "", 999, 50, 2, AssetDirectory.GravediggerItem) { }

		public static void DropLivingBlood(NPC npc, NPCLoot npcLoot)
		{
			if (npc.type == NPCID.BloodZombie || npc.type == NPCID.Drippler)
			{
				npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<LivingBlood>(), 8));
			}
		}
	}
}