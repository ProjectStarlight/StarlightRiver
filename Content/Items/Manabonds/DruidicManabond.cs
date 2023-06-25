using Terraria.ID;

namespace StarlightRiver.Content.Items.Manabonds
{
	internal class DruidicManabond : Manabond
	{
		public override string Texture => AssetDirectory.ManabondItem + Name;

		public DruidicManabond() : base("Druidic Manabond", "Your minions can store 40 mana\nYour minions siphon 6 mana per second from you untill full\nYour minions spend 15 mana to attack with a burst of poision thorns") { }

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 15);
			Item.rare = ItemRarityID.Orange;
		}

		public override void MinionAI(Projectile minion, ManabondProjectile mp)
		{
			if (mp.timer % 60 == 0 && mp.mana >= 6 && mp.target != null)
			{
				mp.mana -= 6;
				int i = Projectile.NewProjectile(minion.GetSource_FromThis(), minion.Center, Vector2.Normalize(mp.target.Center - minion.Center).RotatedByRandom(0.5f) * 15, ModContent.ProjectileType<MagicBolt>(), 12, 0.25f, minion.owner);

				var bolt = Main.projectile[i].ModProjectile as MagicBolt;
				bolt.target = mp.target;
			}
		}
	}
}
