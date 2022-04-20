using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System.Collections.Generic;
using Terraria;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.GUI
{
    public class BossBarOverlay : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        public override bool Visible => visible;

        public static bool visible;

        public static NPC tracked;
        public static string Text;
        public static Texture2D Texture = Request<Texture2D>(AssetDirectory.GUI + "BossBarFrame", ReLogic.Content.AssetRequestMode.ImmediateLoad).Value;
        public static Color glowColor = Color.Transparent;

        private BarOverlay bar = new BarOverlay();

        public float Priority => 1f;

        public override void OnInitialize()
		{
            bar.Left.Set(-258, 0.5f);
            bar.Top.Set(-76, 1);
            bar.Width.Set(516, 0);
            bar.Height.Set(46, 0);
            Append(bar);
		}

        public override void Update(GameTime gameTime)
		{
            Recalculate();

            if (tracked is null)
                visible = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

		public static void SetTracked(NPC NPC, string text = "", Texture2D tex = default)
        {
            tracked = NPC;
            Text = text;
            visible = true;

            if (tex != default)
                Texture = tex;
        }
    }

    public class BarOverlay : UIElement
	{
		public override void Draw(SpriteBatch spriteBatch)
		{
            var NPC = BossBarOverlay.tracked;
            var pos = GetDimensions().ToRectangle().TopLeft() + new Vector2(0, 1);
            var off = new Vector2(30, 12);

            if (NPC is null)
                return;

            var texGlow = Request<Texture2D>(AssetDirectory.GUI + "BossbarGlow").Value;

            int progress = (int)(BossBarOverlay.tracked?.life / (float)BossBarOverlay.tracked?.lifeMax * 456);

            if (NPC.dontTakeDamage || NPC.immortal)
            {
                var texFill = Request<Texture2D>(AssetDirectory.GUI + "BossbarFillImmune").Value;
                var texEdge = Request<Texture2D>(AssetDirectory.GUI + "BossbarEdgeImmune").Value;

                spriteBatch.Draw(texFill, new Rectangle((int)(pos.X + off.X), (int)(pos.Y + off.Y) + 2, progress, texFill.Height -4), Color.White);
                spriteBatch.Draw(texEdge, pos + off + Vector2.UnitX * progress, Color.White);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.UIScaleMatrix);

            spriteBatch.Draw(texGlow, pos + off, BossBarOverlay.glowColor * 0.5f);
            spriteBatch.Draw(texGlow, new Rectangle((int)(pos.X + off.X), (int)(pos.Y + off.Y), progress, 22), new Rectangle(0, 0, progress, 22), BossBarOverlay.glowColor);

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);

            Utils.DrawBorderString(spriteBatch, NPC.FullName + BossBarOverlay.Text + ": " + NPC.life + "/" + NPC.lifeMax, pos + new Vector2(516 / 2, -20), Color.White, 1, 0.5f, 0);

            //spriteBatch.Draw(BossBarOverlay.Texture, pos, Color.White);           

            if (NPC.dontTakeDamage || NPC.immortal)
                spriteBatch.Draw(Request<Texture2D>(AssetDirectory.GUI + "BossbarChains").Value, pos, Color.White);
        }
	}
}