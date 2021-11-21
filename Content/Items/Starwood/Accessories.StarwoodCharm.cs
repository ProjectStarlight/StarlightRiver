using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NetEasy;
using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Starwood
{
	class StarwoodCharm : SmartAccessory
	{
        public override string Texture => AssetDirectory.StarwoodItem + Name;

        public StarwoodCharm() : base("Starwood Charm", "Critical strikes generate mana stars\n-3% critical strike chance\n+3% critical strike chance when empowered") { }

        public override void SafeSetDefaults() => item.rare = ItemRarityID.Blue;

        public override bool Autoload(ref string name)
		{
            StarlightPlayer.OnHitNPCEvent += SpawnManaOnCrit;
            StarlightProjectile.ModifyHitNPCEvent += SpawnManaOnProjCrit;
			return base.Autoload(ref name);
		}

		private void SpawnManaOnProjCrit(Projectile projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
            var player = Main.player[projectile.owner];

            if (Equipped(player) && crit)
                spawnStar(target.Center);
        }

		private void SpawnManaOnCrit(Player player, Item item, NPC target, int damage, float knockback, bool crit)
		{
            if (Equipped(player) && crit)
                spawnStar(target.Center);

        }

        private void spawnStar(Vector2 position)
        {
            int item = Item.NewItem(position, ItemID.Star, 1, true, 0, true);


            if (Main.netMode == NetmodeID.MultiplayerClient && item >= 0)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, item, 1f);
            }
        }

        public override void SafeUpdateEquip(Player player)
        {
            var mp = player.GetModPlayer<StarlightPlayer>();

            player.meleeCrit += mp.empowered ? 3 : -3;
            player.rangedCrit += mp.empowered ? 3 : -3;
            player.magicCrit += mp.empowered ? 3 : -3;
            player.thrownCrit += mp.empowered ? 3 : -3;
        }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
		{
            var mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
            frame.Height /= 2;
            origin.Y /= 2;
            scale = Main.inventoryScale;
            position += new Vector2(-4, 4);

            if (!mp.empowered)
            {
                spriteBatch.Draw(ModContent.GetTexture(Texture), position, frame, drawColor, 0, origin, scale, 0, 0);
                return false;
            }

            frame.Y += frame.Height;
            spriteBatch.Draw(ModContent.GetTexture(Texture), position, frame, drawColor, 0, origin, scale, 0, 0);
            return false;
		}
	}
}
