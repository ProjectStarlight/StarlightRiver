using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.Gravedigger;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems.InoculationSystem;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.RatKing
{
	class BloodCrystal : CursedAccessory
	{
		public override string Texture => AssetDirectory.RatKingItem + Name;

		public BloodCrystal() : base(ModContent.Request<Texture2D>(AssetDirectory.RatKingItem + "BloodCrystal").Value) { }

		public override void Load()
		{
			StarlightNPC.OnHitByItemEvent += InflictItem;
			StarlightNPC.OnHitByProjectileEvent += InflictProj;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Blood Crystal");
			Tooltip.SetDefault(
				"Inflict blood resonance for every debuff on an enemy when you hit them\n" +
				"Cursed : Inflict an equal amount of blood resonance on yourself\n" +
				"Blood resonance decreases inoculation by 3% per stack\n" +
				"You take 5 damage per second while you have blood resonance");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(0, 3, 0, 0);
		}

		private void InflictItem(NPC npc, Player player, Item item, NPC.HitInfo hit, int damageDone)
		{
			if (Equipped(player))
			{
				for (int k = 0; k < npc.buffTime.Length; k++)
				{
					if (npc.buffTime[k] > 0)
					{
						BuffInflictor.Inflict<BloodCrystalBuff>(npc, 420);
						BuffInflictor.Inflict<BloodCrystalBuff>(player, 420);
					}
				}
			}
		}

		private void InflictProj(NPC npc, Projectile projectile, NPC.HitInfo hit, int damageDone)
		{
			Player player = Main.player[projectile.owner];

			if (Equipped(player))
			{
				for (int k = 0; k < npc.buffTime.Length; k++)
				{
					if (npc.buffTime[k] > 0)
					{
						BuffInflictor.Inflict<BloodCrystalBuff>(npc, 420);
						BuffInflictor.Inflict<BloodCrystalBuff>(player, 420);
					}
				}
			}
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

	public class BloodCrystalBuff : StackableBuff
	{
		public override string Name => "BloodCrystalBuff";

		public override string DisplayName => "Blood Resonance";

		public override string Tooltip => "Inoculation is decreased";

		public override string Texture => AssetDirectory.Buffs + Name;

		public override bool Debuff => true;

		public override BuffStack GenerateDefaultStack(int duration)
		{
			var stack = new BuffStack
			{
				duration = duration
			};
			return stack;
		}

		public override void PerStackEffectsNPC(NPC npc, BuffStack stack)
		{
			npc.GetGlobalNPC<InoculationNPC>().DoTResist -= 0.03f;
		}

		public override void PerStackEffectsPlayer(Player player, BuffStack stack)
		{
			player.GetModPlayer<InoculationPlayer>().DoTResist -= 0.03f;
		}

		public override void AnyStacksUpdatePlayer(Player player)
		{
			player.lifeRegen -= 10;
		}

		public override void AnyStacksUpdateNPC(NPC npc)
		{
			if (Main.rand.NextBool(30))
			{
				var d = Dust.NewDustPerfect(npc.Center, ModContent.DustType<Dusts.Aurora>(), Vector2.Zero, 0, new Color(255, 100, 100) * 0.25f, 1);
				d.customData = stacks.Count * 0.1f;
			}
		}
	}
}