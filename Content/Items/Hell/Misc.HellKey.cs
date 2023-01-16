using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using Terraria;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Items.Hell
{
	class HellKey : QuickMaterial
    {
        public override string Texture => "StarlightRiver/Assets/Items/Hell/HellKey";

        public HellKey() : base("Hellscorch Key", "Opens a hellscorch chest", 10, 10000, ItemRarityID.Quest) { }

        public override void PostDrawInWorld(SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
        {
            Texture2D tex = Request<Texture2D>("StarlightRiver/Assets/Items/Hell/HellKeyGlow").Value;
            spriteBatch.Draw(tex, Item.position + new Vector2(0, -8) - Main.screenPosition, Color.White);
        }
    }
}
