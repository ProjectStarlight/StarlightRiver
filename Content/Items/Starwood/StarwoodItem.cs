using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Items.Starwood
{
    public abstract class StarwoodItem : ModItem
    {
        protected Texture2D EmpoweredTexture;
        protected bool isEmpowered;

        protected StarwoodItem(Texture2D AltTexture) => EmpoweredTexture = AltTexture;
        public override void UpdateInventory(Player player) => isEmpowered = player.GetModPlayer<StarlightPlayer>().Empowered;
        public override bool PreDrawInInventory(SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color itemColor, Vector2 origin, float scale)
        {
            if (isEmpowered)
                spriteBatch.Draw(EmpoweredTexture, position, frame, drawColor, default, origin, scale, default, default);
            return !isEmpowered;
        }
    }
}