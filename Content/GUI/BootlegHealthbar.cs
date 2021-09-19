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
    public class BootlegHealthbar : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
        public override bool Visible => tracked != null && tracked.active;

        public static NPC tracked;
        public static string Text;
        public static Texture2D Texture = GetTexture(AssetDirectory.GUI + "blank2");
        public int Timer;

        private Bar bar = new Bar();

		public override void OnInitialize()
		{
            bar.Left.Set(-258, 0.5f);
            bar.Top.Set(-80, 1);
            bar.Width.Set(516, 0);
            bar.Height.Set(46, 0);
            Append(bar);
		}

		public override void Update(GameTime gameTime)
		{
            Recalculate();
		}

		public static void SetTracked(NPC npc, string text = "", Texture2D tex = default)
        {
            tracked = npc;
            Text = text;

            if (tex != default)
                Texture = tex;
        }
	}

    public class Bar : UIElement
	{
		public override void Draw(SpriteBatch spriteBatch)
		{
            var npc = BootlegHealthbar.tracked;
            var pos = GetDimensions().ToRectangle().TopLeft();
            var off = new Vector2(30, 14);

            var texBack = GetTexture(AssetDirectory.GUI + "BossbarBack");
            var texFill = GetTexture(AssetDirectory.GUI + "BossbarFill");
            var texEdge = GetTexture(AssetDirectory.GUI + "BossbarEdge");

            if (npc.dontTakeDamage || npc.immortal)
			{
                texFill = GetTexture(AssetDirectory.GUI + "BossbarFillImmune");
                texEdge = GetTexture(AssetDirectory.GUI + "BossbarEdgeImmune");
            }

            int progress = (int)(BootlegHealthbar.tracked?.life / (float)BootlegHealthbar.tracked?.lifeMax * texBack.Width);

            spriteBatch.Draw(texBack, pos + off, Color.White);
            spriteBatch.Draw(texFill, new Rectangle((int)(pos.X + off.X), (int)(pos.Y + off.Y), progress, texFill.Height), Color.White);
            spriteBatch.Draw(texEdge, pos + off + Vector2.UnitX * progress, Color.White);

            spriteBatch.Draw(BootlegHealthbar.Texture, pos, Color.White);

            if (npc.GetBossHeadTextureIndex() > 0)
            {
                var tex = Main.npcHeadBossTexture[npc.GetBossHeadTextureIndex()];
                spriteBatch.Draw(tex, pos + new Vector2(0, 10), Color.White);
            }

            if (npc.dontTakeDamage || npc.immortal)
                spriteBatch.Draw(GetTexture(AssetDirectory.GUI + "BossbarChains"), pos, Color.White);
        }
	}
}