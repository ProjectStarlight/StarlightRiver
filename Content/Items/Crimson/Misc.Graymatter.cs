using rail;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;


namespace StarlightRiver.Content.Items.Crimson
{
	internal class Graymatter : ModItem
	{
		public override string Texture => AssetDirectory.CrimsonItem + "DendritePickup";
		public override LocalizedText Tooltip => LocalizedText.Empty;

		public override void SetStaticDefaults()
		{
			ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
			ItemID.Sets.IgnoresEncumberingStone[Type] = true;
			ItemID.Sets.IsAPickup[Type] = true;
			ItemID.Sets.ItemSpawnDecaySpeed[Type] = 4;
		}

		public override void SetDefaults()
		{
			Item.height = 18;
			Item.width = 18;
		}

		internal class PsychosisImbuementBuff : SmartBuff
		{
			public override string Texture => AssetDirectory.Buffs + Name;

			public PsychosisImbuementBuff() : base("Weapon Imbue: Psychosis", "All attacks inflict stacks of the Psychosis debuff on enemies", false) { }

			public override void Load()
			{
				StarlightNPC.ModifyHitByItemEvent += PsychosisBuff;
				StarlightNPC.ModifyHitByProjectileEvent += PsychosisBuffProjectile;
			}


			private void PsychosisBuffProjectile(NPC npc, Projectile projectile, ref NPC.HitModifiers modifiers)
			{
				Player player = Main.player[projectile.owner];
				if (player.HasBuff(ModContent.BuffType<PsychosisImbuementBuff>()))
				{
					BuffInflictor.Inflict<Psychosis>(npc, 60 * Main.rand.Next(5,9));
				}
			}

			private void PsychosisBuff(NPC npc, Player player, Item item, ref NPC.HitModifiers modifiers)
			{
				if (player.HasBuff(ModContent.BuffType<PsychosisImbuementBuff>()))
				{
					BuffInflictor.Inflict<Psychosis>(npc, 60 * Main.rand.Next(5, 9));
				}
			}
		}

		public override bool OnPickup(Player player)
		{
			player.AddBuff(ModContent.BuffType<PsychosisImbuementBuff>(), 3600, true, false);
			SoundEngine.PlaySound(SoundID.Item29, player.Center);
			return false;
		}
	}
}

