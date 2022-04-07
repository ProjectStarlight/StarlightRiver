using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Content.Biomes;
using StarlightRiver.Core;
using System;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
    class MapHack : HookGroup
    {
        public override SafetyLevel Safety => SafetyLevel.Fragile;

		public override void Load()
        {
            if (Main.dedServ)
                return;

            IL.Terraria.Main.DrawMap += MapShader;
        }

		private void MapShader(ILContext il)
		{
            var c = new ILCursor(il);
            c.TryGotoNext(n => n.MatchStloc(56), n => n.MatchLdloc(61), n => n.MatchLdloc(59));
            c.Index += 2;
            c.EmitDelegate<Action>(RestartSB);

            c.TryGotoNext(n => n.MatchCallvirt<SpriteBatch>("Draw"));
            c.Index++;
            c.EmitDelegate<Action>(ResetSB);
		}

		private void ResetSB()
		{
            if (FirstContactEvent.spaceEventFade > 0)
            {
                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);
            }
        }

		private void RestartSB()
		{
            if (FirstContactEvent.spaceEventFade > 0)
            {
                Terraria.Graphics.Effects.Filters.Scene["SpaceMap"].GetShader().Shader.Parameters["uTime"].SetValue(Main.GameUpdateCount / 30f);
                Terraria.Graphics.Effects.Filters.Scene["SpaceMap"].GetShader().Shader.Parameters["sampleTexture2"].SetValue(Terraria.ModLoader.ModContent.Request<Texture2D>("StarlightRiver/Assets/Misc/SkyMap").Value);
                Terraria.Graphics.Effects.Filters.Scene["SpaceMap"].GetShader().Shader.Parameters["uOpacity"].SetValue(FirstContactEvent.spaceEventFade);

                Main.spriteBatch.End();
                Main.spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, Terraria.Graphics.Effects.Filters.Scene["SpaceMap"].GetShader().Shader);
            }
        }
	}
}