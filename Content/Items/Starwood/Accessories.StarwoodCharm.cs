using StarlightRiver.Content.Items.BaseTypes;
using Terraria.ID;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace StarlightRiver.Content.Items.Starwood
{
	class StarwoodCharm : SmartAccessory
	{
		public override string Texture => AssetDirectory.StarwoodItem + Name;

		public StarwoodCharm() : base("Starwood Charm", "Critical strikes generate mana stars\n-3% critical strike chance\n+3% critical strike chance when empowered by Starwood Armor") { }

		public override void SafeSetDefaults()
		{
			Item.rare = ItemRarityID.Blue;
			Item.Size = new Vector2(24, 28);
		}

		public override void Load()
		{
			StarlightPlayer.OnHitNPCEvent += SpawnManaOnCrit;
			StarlightProjectile.OnHitNPCEvent += SpawnManaOnProjCrit;
		}

		private void SpawnManaOnProjCrit(Projectile Projectile, NPC target, NPC.HitInfo info, int damageDone)
		{
			Player Player = Main.player[Projectile.owner];

			if (Equipped(Player) && info.Crit && Main.myPlayer == Player.whoAmI)
				SpawnStar(target.Center);
		}

		private void SpawnManaOnCrit(Player Player, Item Item, NPC target, NPC.HitInfo info, int damageDone)
		{
			if (Equipped(Player) && info.Crit && Main.myPlayer == Player.whoAmI)
				SpawnStar(target.Center);
		}

		private void SpawnStar(Vector2 position)
		{
			Player player = Main.player[Item.playerIndexTheItemIsReservedFor];
			int item = Item.NewItem(player.GetSource_Loot(), position, ItemID.Star, 1, true, 0, true);

			if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
				NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
		}

		public override void SafeUpdateEquip(Player Player)
		{
			StarlightPlayer mp = Player.GetModPlayer<StarlightPlayer>();

			Player.GetCritChance(DamageClass.Generic) += mp.empowered ? 3 : -3;
		}

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();

			frame.Height -= 2;
			frame.Height /= 2;

			origin.Y = frame.Height / 2;

			scale = Main.inventoryScale;

			if (!mp.empowered)
			{
				spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, position, frame, drawColor, 0, origin, scale, 0, 0);
				return false;
			}

			frame.Y += frame.Height + 2;
			spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, position, frame, drawColor, 0, origin, scale, 0, 0);
			return false;
		}

		public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
			StarlightPlayer mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();

			Rectangle frame = new Rectangle(0, 0, tex.Width, (tex.Height - 2) / 2);
			Vector2 offset = new Vector2(0, 4);//keeps item from hovering above floor

			if (!mp.empowered)
			{
				spriteBatch.Draw(tex, Item.position - Main.screenPosition + offset, frame, lightColor * alphaColor.A, rotation, default, scale, 0, 0);
				return false;
			}

			frame.Y = frame.Height + 2;
			spriteBatch.Draw(tex, Item.position - Main.screenPosition + offset, frame, lightColor * alphaColor.A, rotation, default, scale, 0, 0);
			return false;
		}
	}
}