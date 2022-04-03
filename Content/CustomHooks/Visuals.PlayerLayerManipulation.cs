using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	class PlayerLayerManipulation : HookGroup
    {
        //Swaps out some variables on PlayerLayer drawData for certain effects like the pancake and spinning. This might look weird if other mods do strange PlayerLayer stuff but it shouldnt have consquences outside of visuals. The actual patch is a bit iffy rn tho, come fix this later.
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            IL.Terraria.Main.DrawPlayer_DrawAllLayers += ManipulateLayers; //PORTTODO: Figure out where all of this moved
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

        private delegate DrawData LayerManipDelegate(DrawData input, Player Player);

        private DrawData EmitLayerManipDelegate(DrawData input, Player Player)
        {
            /*if(!Main.gameMenu && Player.HeldItem.GetGlobalItem<Items.Vitric.GlassReplica>().isReplica && input.texture == Main.PopupTexture[Player.HeldItem.type])
            {
                input.shader = 2; //TODO: Move this. actually bind the correct armor shader. Stop being lazy.
            }*/

            float rotation = Player.GetModPlayer<StarlightPlayer>().rotation;

            if (rotation != 0) //paper mario-style rotation
            {
                float sin = (float)Math.Sin(rotation + 1.57f * Player.direction);
                int off = Math.Abs((int)(input.texture.Width * sin));

                SpriteEffects effect = sin > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                if (input.effect == SpriteEffects.FlipHorizontally) effect = effect == SpriteEffects.FlipHorizontally ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                DrawData newData = new DrawData(input.texture, new Rectangle((int)input.position.X, (int)input.position.Y, off, input.useDestinationRectangle ? input.destinationRectangle.Height : input.sourceRect?.Height ?? input.texture.Height),
                    input.sourceRect, input.color, input.rotation, input.origin, effect, 0);

                newData.shader = input.shader;

                return newData;
            }

            //the pancake debuff
            else if (Player.HasBuff(ModContent.BuffType<Buffs.Squash>()))
            {
                DrawData newData = new DrawData(input.texture, new Rectangle((int)Player.position.X - 20 - (int)Main.screenPosition.X, (int)Player.position.Y + 20 - (int)Main.screenPosition.Y + 1, Player.width + 40, Player.height - 20), input.sourceRect, input.color, input.rotation, default, input.effect, 0);
                newData.shader = input.shader;

                return newData;
            }

            else return input;
        }


    }
}