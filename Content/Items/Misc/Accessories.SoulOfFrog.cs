using StarlightRiver.Content.Items.BaseTypes;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class SoulOfFrog : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public SoulOfFrog() : base("Soul Of Frog",
			"Frogs are resurrected on death, growing stronger with each return from the brink.\n" +
			"Killing other critters has a chance to reveal them as actually being a frog the whole time\n" +
			"They will then be resurrected as if they were a frog the whole time, because they were\n" +
			"I'm telling you, don't fuck with the frogs, please, they are stronger than you know")
		{ }

		public override void SafeSetDefaults()
		{
			Item.value = 1;
			Item.rare = ItemRarityID.LightRed;
		}

		public override void Load()
		{
			StarlightNPC.ModifyHitByItemEvent += GrowFrogItem;
			StarlightNPC.ModifyHitByProjectileEvent += GrowFrogProjectile;

			StarlightNPC.ModifyNPCLootEvent += Drop;
		}

		private void GrowFrogItem(NPC NPC, Player Player, Item Item, ref NPC.HitModifiers hit)
		{
			if (Equipped(Player))
				TrySummonFrog(NPC);
		}

		private void GrowFrogProjectile(NPC NPC, Projectile Projectile, ref NPC.HitModifiers hit)
		{
			if (Equipped(Main.player[Projectile.owner]))
				TrySummonFrog(NPC);
		}

		private void Drop(NPC npc, NPCLoot npcloot)
		{
			if (npc.type == NPCID.Frog)
				npcloot.Add(ItemDropRule.Common(ModContent.ItemType<SoulOfFrog>(), 1000));
		}

		private static void TrySummonFrog(NPC NPC)
		{
			if (NPC.type == NPCID.Frog || Main.rand.NextBool(5) && NPC.catchItem > 0)
			{
				NPC frog = Main.npc[NPC.NewNPC(NPC.GetSource_FromThis(), (int)NPC.Center.X, (int)NPC.Center.Y + 5, NPCID.Frog)];
				if (NPC.type == NPCID.Frog)
				{
					frog.scale = NPC.scale + 0.01f;

					int oldWidth = frog.width;
					int oldHeight = frog.height;

					frog.width = (int)(frog.scale * frog.width);
					frog.height = (int)(frog.scale * frog.height);

					frog.Center = NPC.Center - new Vector2(0, frog.height - NPC.height);
				}
			}
		}
	}
}