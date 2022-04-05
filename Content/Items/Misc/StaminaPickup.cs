using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Misc
{
	internal class StaminaPickup : ModItem
    {
        public override string Texture => AssetDirectory.GUI + "Stamina";

        public override bool PreDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI) => false;
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale) => false;

        public override bool ItemSpace(Player Player) => true;

        public override bool OnPickup(Player Player)
        {
            AbilityHandler mp = Player.GetHandler();
            mp.Stamina++;

            for (int k = 0; k <= 20; k++)
                Dust.NewDust(Player.Center, 1, 1, DustType<Dusts.Stamina>(), 0, 0, 0, default, 1.2f);
            CombatText.NewText(Player.Hitbox, new Color(255, 170, 60), "+1");
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Item112, Player.Center);
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            spriteBatch.Draw(Terraria.GameContent.TextureAssets.Item[Item.type].Value, Item.Center - Vector2.One * 11 - Main.screenPosition, new Rectangle(0, 0, 22, 22), Color.White * (0.7f + (float)Math.Sin(StarlightWorld.rottime) * 0.1f),
                rotation, Vector2.One * 11, 0.9f + (float)Math.Sin(StarlightWorld.rottime) * 0.1f, 0, 0);
        }
    }

    internal class StaminaDrop : GlobalNPC
    {
        public bool DropStamina = false;

        public override bool InstancePerEntity => true;

		public override void OnKill(NPC npc)
		{
            if (DropStamina)
                Item.NewItem(npc.GetItemSource_Loot(), npc.Hitbox, ItemType<StaminaPickup>());
        }
    }
}
