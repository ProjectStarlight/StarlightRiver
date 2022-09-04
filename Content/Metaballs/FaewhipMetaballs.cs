using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.Enums;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core.Systems.MetaballSystem;
using StarlightRiver.Content.Dusts;
using Terraria.Graphics;
using Terraria.Graphics.CameraModifiers;
using Terraria.Graphics.Capture;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Light;
using Terraria.Graphics.Renderers;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.Items.Misc
{
    internal class FaewhipMetaballs : MetaballActor
    {
        public override bool Active => Main.dust.Any(x => x.active && x.type == DustType);

        public int DustType => ModContent.DustType<Dusts.FaewhipMetaballDust>();

        public override Color outlineColor => Color.Black;

        public Color inColor => Color.Gold;

        public override void DrawShapes(SpriteBatch spriteBatch)
        {
            Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;

            var tex = ModContent.Request<Texture2D>(AssetDirectory.MiscItem + "MagmaGunProj").Value;

            if (borderNoise is null)
                return;

            borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            borderNoise.CurrentTechnique.Passes[0].Apply();

            foreach (Dust dust in Main.dust)
            {
                if (dust.active && dust.type == DustType)
                {
                    borderNoise.Parameters["offset"].SetValue((float)dust.rotation);
                    spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, Color.White, dust.rotation, tex.Size() / 2, 0.005f, SpriteEffects.None, 0);
                }
            }

            spriteBatch.End();
            spriteBatch.Begin();
        }

        public override void SafeLoad()
        {
            On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayers += DrawOverPlayers;
        }

        public override void SafeUnload()
        {
            On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.DrawPlayers -= DrawOverPlayers;
        }

        private void DrawOverPlayers(On.Terraria.Graphics.Renderers.LegacyPlayerRenderer.orig_DrawPlayers orig, Terraria.Graphics.Renderers.LegacyPlayerRenderer self, Camera camera, IEnumerable<Player> players)
        {
            orig(self, camera, players);

            if (!Active)
                return;
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, null, Main.GameViewMatrix.ZoomMatrix);

            Main.spriteBatch.Draw(Target, Vector2.Zero, null, inColor * 0.3f, 0, Vector2.Zero, 2, 0, 0);
            
            Main.spriteBatch.End();
        }
    }
}
