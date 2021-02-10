using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Keys;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;

namespace StarlightRiver.Content.GUI
{
    public class KeyInventory : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public static List<KeyIcon> keys = new List<KeyIcon>();

        protected override void DrawSelf(SpriteBatch spriteBatch)
        {
            foreach (KeyIcon key in keys)
                key.DrawKey(spriteBatch, new Vector2(Main.screenWidth - (Main.LocalPlayer.GetModPlayer<Abilities.AbilityHandler>().StaminaMax > 7 ? 344 : 324), 110 + keys.IndexOf(key) * 40));
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            Recalculate();
        }
    }

    public class KeyIcon : UIElement
    {
        public int timer;
        public Key parent;

        public KeyIcon(Key key, bool animate)
        {
            parent = key;
            timer = animate ? 60 : 0;
        }

        public void DrawKey(SpriteBatch spriteBatch, Vector2 pos)
        {
            Vector2 center = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2);
            Texture2D tex = GetTexture(parent.Texture);
            float x = (timer - 30) / 30f;
            float scale = 1 + (0.7f - 0.7f * (x * x));
            Color color = parent.ShowCondition ? Color.White : Color.White * 0.2f;
            spriteBatch.Draw(tex, Vector2.SmoothStep(pos, center, timer / 60f), tex.Frame(), color, 0, tex.Frame().Size() / 2, scale, 0, 0);

            if (timer > 0) timer--;
        }
    }
}