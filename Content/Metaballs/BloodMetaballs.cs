using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Content.Buffs;
using StarlightRiver.Content.Items.Vitric;
using StarlightRiver.Helpers;
using Terraria;
using Terraria.ID;
using Terraria.Enums;
using Terraria.ModLoader;
using System;
using System.Linq;
using System.Collections.Generic;
using Terraria.Graphics.Effects;
using Terraria.DataStructures;
using Terraria.GameContent;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using StarlightRiver.Core.Systems.MetaballSystem;
using StarlightRiver.Content.Dusts;

namespace StarlightRiver.Content.Items.Misc
{
    internal class BloodMetaballs : MetaballActor
    {
        public override bool Active => Main.dust.Any(x => x.active && x.type == DustType);

        public override Color outlineColor => new Color(173, 19, 19);

        public virtual Color inColor => new Color(96, 6, 6);

        public virtual int DustType => ModContent.DustType<BloodMetaballDust>();

        public override void DrawShapes(SpriteBatch spriteBatch)
        {
            Effect borderNoise = Filters.Scene["BorderNoise"].GetShader().Shader;

            var tex = ModContent.Request<Texture2D>(AssetDirectory.Dust + "BloodLine").Value;

            if (borderNoise is null)
                return;

            borderNoise.Parameters["offset"].SetValue((float)Main.time / 100f);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            borderNoise.CurrentTechnique.Passes[0].Apply();

            foreach (Dust dust in Main.dust)
            {
                if (dust.active && dust.type == DustType && dust.customData != null)
                {
                    borderNoise.Parameters["offset"].SetValue((float)dust.rotation);
                    spriteBatch.Draw(tex, (dust.position - Main.screenPosition) / 2, null, Color.White, dust.rotation, tex.Size() / 2, dust.scale * new Vector2(1f, (float)dust.customData + (0.25f * dust.velocity.Length())), SpriteEffects.None, 0);
                }
            }

            spriteBatch.End();
            spriteBatch.Begin();
        }

        public override bool PostDraw(SpriteBatch spriteBatch, Texture2D target)
        {
            Rectangle sourceRect = new Rectangle(0, 0, target.Width, target.Height);
            LightingBufferRenderer.DrawWithLighting(sourceRect, target, sourceRect, inColor, new Vector2(2, 2));
            return false;
        }
    }

    internal class BloodMetaballsLight : BloodMetaballs
    {
        public override int DustType => ModContent.DustType<BloodMetaballDustLight>();

        public override Color outlineColor => new Color(129, 0, 0);

        public override Color inColor => new Color(192, 27, 27);
    }
}
