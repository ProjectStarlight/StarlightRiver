using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using StarlightRiver.Core.Systems.CombatMountSystem;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
    public class CombatMountInfo : SmartUIState
    {
        CombatMountSystem ModPlayer => Main.LocalPlayer.GetModPlayer<CombatMountSystem>();

        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => !Main.playerInventory && ModPlayer.activeMount != null;

		public override void Draw(SpriteBatch spriteBatch)
		{
            Vector2 pos = new Vector2(480, 24);
            var tex = Request<Texture2D>(AssetDirectory.GUI + "MountAbilitySlot").Value;

            var icon1 = Request<Texture2D>(ModPlayer.activeMount.PrimaryIconTexture).Value;
            var icon2 = Request<Texture2D>(ModPlayer.activeMount.SecondaryIconTexture).Value;

            spriteBatch.Draw(tex, pos, Color.White);
            spriteBatch.Draw(icon1, pos + Vector2.One * 22, null, Color.White, 0, icon1.Size() / 2, 1, 0, 0);

            var target = new Rectangle((int)pos.X, (int)pos.Y, tex.Width, (int)(tex.Height * (ModPlayer.activeMount.useCooldown / (float)ModPlayer.activeMount.MaxActionCD)));
            spriteBatch.Draw(tex, target, Color.Black * 0.5f);

            pos.X += 50;

            spriteBatch.Draw(tex, pos, Color.White);
            spriteBatch.Draw(icon2, pos + Vector2.One * 22, null, Color.White, 0, icon2.Size() / 2, 1, 0, 0);

            target = new Rectangle((int)pos.X, (int)pos.Y, tex.Width, (int)(tex.Height * (ModPlayer.activeMount.secondaryCooldown / (float)ModPlayer.activeMount.MaxSecondaryCD)));
            spriteBatch.Draw(tex, target, Color.Black * 0.5f);
        }

		public override void Update(GameTime gameTime)
        {

        }
    }
}