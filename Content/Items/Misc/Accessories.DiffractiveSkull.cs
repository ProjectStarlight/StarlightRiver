using StarlightRiver.Content.Items.BaseTypes;

namespace StarlightRiver.Content.Items.Misc
{
	class DiffractiveSkull : CursedAccessory
	{
		public override string Texture => AssetDirectory.MiscItem + Name;

		public DiffractiveSkull() : base(ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "DiffractiveSkull").Value) { }

		public override void Load()
		{
			StarlightItem.CanUseItemEvent += StarlightItem_CanUseItemEvent;
		}

		private bool StarlightItem_CanUseItemEvent(Item Item, Player Player)
		{
			if (Equipped(Player))
			{
				var dummyProj = new Projectile();

				dummyProj.SetDefaults(Item.shoot);

				if (Item.CountsAsClass(DamageClass.Summon) && dummyProj.minion)
					return Player.ownedProjectileCounts[Item.shoot] <= 0;

			}

			return true;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Diffractive Skull");
			Tooltip.SetDefault("Increases your max number of minions by 4\nYou cannot have the same minion summoned more than once\n'Behold, I shall beat you to death.. with everything!'");
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 1);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			Player.maxMinions += 4;
		}

		public override void OnEquip(Player player, Item item)
		{
			for (int i = 0; i < Main.maxProjectiles; i++)
			{
				Projectile proj = Main.projectile[i];

				if (proj.minion && proj.minionPos > 0 && proj.owner == player.whoAmI)
					proj.Kill();
			}
		}
	}
}
