using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Items
{
    internal class StaminaPickup : ModItem
    {
        public override string Texture => "StarlightRiver/Invisible";

        public override bool ItemSpace(Player player)
        {
            return true;
        }

        public override bool OnPickup(Player player)
        {
            AbilityHandler mp = player.GetModPlayer<AbilityHandler>();
            mp.StatStamina++;
            if (mp.wisp.Active) { mp.wisp.Timer = 60 * mp.StatStamina - 1; }

            for (int k = 0; k <= 20; k++)
            {
                Dust.NewDust(player.Center, 1, 1, DustType<Dusts.Stamina>(), 0, 0, 0, default, 1.2f);
            }
            CombatText.NewText(player.Hitbox, new Color(255, 170, 60), "+1");
            Main.PlaySound(SoundID.Item112, player.Center);
            return false;
        }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            spriteBatch.Draw(GetTexture("StarlightRiver/GUI/Assets/Stamina"), item.Center - Vector2.One * 11 - Main.screenPosition, new Rectangle(0, 0, 22, 22), Color.White * (0.7f + (float)Math.Sin(StarlightWorld.rottime) * 0.1f),
                rotation, Vector2.One * 11, 0.9f + (float)Math.Sin(StarlightWorld.rottime) * 0.1f, 0, 0);
        }
    }

    internal class StaminaDrop : GlobalNPC
    {
        public bool DropStamina = false;
        public override bool InstancePerEntity => true;
        public override void NPCLoot(NPC npc)
        {
            if (DropStamina)
            {
                Item.NewItem(npc.Center, ItemType<StaminaPickup>());
            }
        }
    }
}
