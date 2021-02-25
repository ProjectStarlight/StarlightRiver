using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;

namespace StarlightRiver.Content.ArmorEnchantment
{
    class DebugEnchant : ArmorEnchantment
    {
        public DebugEnchant() : base() { }

        public DebugEnchant(Guid guid) : base(guid) { }

        public override string Texture => AssetDirectory.ArmorEnchant + "DebugEnchant";

        public override bool IsAvailable(Item head, Item chest, Item legs)
        {
            return true;
        }

        public override void UpdateSet(Player player)
        {
            player.setBonus = "Spams the chat with debug text";
            Main.NewText(player.name + " Is wearing armor enchanted with the debug enchant!");
        }

        public override void DrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {

        }

        public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

            var tex = Main.itemTexture[item.type];

            for (int k = 0; k < 3; k++)
            {
                spriteBatch.Draw(tex, position + tex.Size() * 0.5f * scale, frame, new Color(0.5f, 0.8f, 1f) * (0.55f), 0, frame.Size() * 0.5f, scale * 1.3f + 0.1f * (float)Math.Sin(StarlightWorld.rottime + k), SpriteEffects.None, 0);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

            return true;
        }
    }
}
