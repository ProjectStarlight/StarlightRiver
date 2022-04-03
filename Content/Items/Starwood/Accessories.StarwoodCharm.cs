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

        public override void SafeSetDefaults() => Item.rare = ItemRarityID.Blue;

        public override void Load()
		{
            StarlightPlayer.OnHitNPCEvent += SpawnManaOnCrit;
            StarlightProjectile.ModifyHitNPCEvent += SpawnManaOnProjCrit;
			return base.Autoload(ref name);
		}

		private void SpawnManaOnProjCrit(Projectile Projectile, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
            var Player = Main.player[Projectile.owner];

            if (Equipped(Player) && crit && Main.myPlayer == Player.whoAmI)
                spawnStar(target.Center);
        }

		private void SpawnManaOnCrit(Player Player, Item Item, NPC target, int damage, float knockback, bool crit)
		{
            if (Equipped(Player) && crit && Main.myPlayer == Player.whoAmI)
                spawnStar(target.Center);

        }

        private void spawnStar(Vector2 position)
        {
            int Item = Item.NewItem(position, ItemID.Star, 1, true, 0, true);


            if (Main.netMode == NetmodeID.MultiplayerClient && Item >= 0)
            {
                NetMessage.SendData(MessageID.SyncItem, -1, -1, null, Item, 1f);
            }
        }

        public override void SafeUpdateEquip(Player Player)
        {
            var mp = Player.GetModPlayer<StarlightPlayer>();

            Player.meleeCrit += mp.empowered ? 3 : -3;
            Player.rangedCrit += mp.empowered ? 3 : -3;
            Player.magicCrit += mp.empowered ? 3 : -3;
            Player.thrownCrit += mp.empowered ? 3 : -3;
        }

		public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
            var mp = Main.LocalPlayer.GetModPlayer<StarlightPlayer>();
            frame.Height /= 2;
            origin.Y /= 2;
            scale = Main.inventoryScale;
            position += new Vector2(-4, 4);

            if (!mp.empowered)
            {
                spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, position, frame, drawColor, 0, origin, scale, 0, 0);
                return false;
            }

            frame.Y += frame.Height;
            spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, position, frame, drawColor, 0, origin, scale, 0, 0);
            return false;
		}
	}
}
