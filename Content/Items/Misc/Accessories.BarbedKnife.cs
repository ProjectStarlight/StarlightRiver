using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Systems.InstancedBuffSystem;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Misc
{
	public class BarbedKnife : SmartAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public BarbedKnife() : base("Barbed Knife", "Critical strikes apply a bleeding debuff that stacks up to five times") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(0, 0, 20, 0);
		}

		public override void Load()
		{
			StarlightPlayer.OnHitNPCWithProjEvent += OnHitNPCWithProjAccessory;
			StarlightPlayer.OnHitNPCEvent += OnHitNPC;
		}

		public override void Unload()
		{
			StarlightPlayer.OnHitNPCWithProjEvent -= OnHitNPCWithProjAccessory;
			StarlightPlayer.OnHitNPCEvent -= OnHitNPC;
		}

		private void OnHitNPCWithProjAccessory(Player Player, Projectile proj, NPC target, NPC.HitInfo info, int damageDone)
		{
			OnHit(Player, target, info.Crit);
		}

		private void OnHitNPC(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			OnHit(Player, target, info.Crit);
		}

		private void OnHit(Player Player, NPC target, bool crit)
		{
			if (Equipped(Player) && crit)
			{
				BarbedKnifeBleed bleed = InstancedBuffNPC.GetInstance<BarbedKnifeBleed>(target);

				if (bleed is null || bleed.stacks.Count < 5)
					BuffInflictor.Inflict<BarbedKnifeBleed>(target, 300);

				if (Main.netMode == NetmodeID.MultiplayerClient)
					Player.GetModPlayer<StarlightPlayer>().shouldSendHitPacket = true;
			}
		}

		public override void AddRecipes()
		{
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.ShadowScale, 5);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();

			recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.TissueSample, 5);
			recipe.AddRecipeGroup(RecipeGroupID.IronBar, 10);
			recipe.AddTile(TileID.Anvils);
			recipe.Register();
		}
	}

	internal class BarbedKnifeBleed : StackableBuff
	{
		public override string Name => "BarbedKnifeBleed";

		public override string DisplayName => "Deep gash";

		public override string Texture => AssetDirectory.MiscItem + Name;

		public override bool Debuff => true;

		public override string Tooltip => "You're bleeding out!";

		public override BuffStack GenerateDefaultStack(int duration)
		{
			return new BuffStack()
			{
				duration = duration
			};
		}

		public override void PerStackEffectsNPC(NPC npc, BuffStack stack)
		{
			npc.lifeRegen -= 15;
		}

		public override void AnyStacksUpdateNPC(NPC npc)
		{
			Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood);
		}
	}
}