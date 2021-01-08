using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

using StarlightRiver.Core;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace StarlightRiver.Content.CustomHooks
{
    class PlayerLayerManipulation : HookGroup
    {
        //Swaps out some variables on PlayerLayer drawData for certain effects like the pancake and spinning. This might look weird if other mods do strange playerLayer stuff but it shouldnt have consquences outside of visuals. The actual patch is a bit iffy rn tho, come fix this later.
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            IL.Terraria.Main.DrawPlayer_DrawAllLayers += ManipulateLayers;
        }

        public override void Unload()
        {
            IL.Terraria.Main.DrawPlayer_DrawAllLayers -= ManipulateLayers;
        }

        private void ManipulateLayers(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchStloc(2)); //I need to match more instructions here probably. TODO: that

            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<LayerManipDelegate>(EmitLayerManipDelegate);
        }

        private delegate DrawData LayerManipDelegate(DrawData input, Player player);

        private DrawData EmitLayerManipDelegate(DrawData input, Player player)
        {
            /*if(!Main.gameMenu && player.HeldItem.GetGlobalItem<Items.Vitric.GlassReplica>().isReplica && input.texture == Main.itemTexture[player.HeldItem.type])
            {
                input.shader = 2; //TODO: Move this. actually bind the correct armor shader. Stop being lazy.
            }*/

            float rotation = player.GetModPlayer<StarlightPlayer>().rotation;

            if (rotation != 0) //paper mario-style rotation
            {
                float sin = (float)Math.Sin(rotation + 1.57f * player.direction);
                int off = Math.Abs((int)(input.texture.Width * sin));

                SpriteEffects effect = sin > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                if (input.effect == SpriteEffects.FlipHorizontally) effect = effect == SpriteEffects.FlipHorizontally ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                return new DrawData(input.texture, new Rectangle((int)input.position.X, (int)input.position.Y, off, input.useDestinationRectangle ? input.destinationRectangle.Height : input.sourceRect?.Height ?? input.texture.Height),
                    input.sourceRect, input.color, input.rotation, input.origin, effect, 0);
            }

            //the pancake debuff
            else if (player.HasBuff(ModContent.BuffType<Buffs.Squash>()))
                return new DrawData(input.texture, new Rectangle((int)player.position.X - 20 - (int)Main.screenPosition.X, (int)player.position.Y + 20 - (int)Main.screenPosition.Y + 1, player.width + 40, player.height - 20), input.sourceRect, input.color, input.rotation, default, input.effect, 0);

            else return input;
        }


    }
}