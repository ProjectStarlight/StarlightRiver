using Terraria.Localization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core;
using StarlightRiver.NPCs;
using StarlightRiver.NPCs.Boss.SquidBoss;
using StarlightRiver.NPCs.TownUpgrade;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System;
using Terraria.Graphics.Effects;

namespace StarlightRiver
{
    public partial class StarlightRiver : Mod
    {
        private void HookIL()
        {
            //Town NPC name swaps
            IL.Terraria.WorldGen.SpawnTownNPC += SwapTitle;
            IL.Terraria.NPC.checkDead += SwapTitleDeath;
            IL.Terraria.Main.DrawInventory += SwapTitleMenu;

            //Pancake debuff
            IL.Terraria.Main.DrawPlayer_DrawAllLayers += DrawPancake;
        }

        private void UnhookIL()
        {

            IL.Terraria.WorldGen.SpawnTownNPC -= SwapTitle;
            IL.Terraria.NPC.checkDead -= SwapTitleDeath;
            IL.Terraria.Main.DrawInventory -= SwapTitleMenu;

            //Pancake debuff
            IL.Terraria.Main.DrawPlayer_DrawAllLayers -= DrawPancake;
        }


        #region IL edits

        //Squash haha funni
        private void DrawPancake(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchStloc(2));

            c.Emit(OpCodes.Ldarg_1);
            c.EmitDelegate<DrawPancakeDelegate>(EmitDrawPancakeDelegate);
        }

        private delegate DrawData DrawPancakeDelegate(DrawData input, Player player);

        private DrawData EmitDrawPancakeDelegate(DrawData input, Player player)
        {
            float rotation = player.GetModPlayer<StarlightPlayer>().rotation;

            if (rotation != 0)
            {
                float sin = (float)Math.Sin(rotation + 1.57f * player.direction);
                int off = Math.Abs((int)(input.texture.Width * sin));

                SpriteEffects effect = sin > 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
                if (input.effect == SpriteEffects.FlipHorizontally) effect = effect == SpriteEffects.FlipHorizontally ? SpriteEffects.None : SpriteEffects.FlipHorizontally;

                return new DrawData(input.texture, new Rectangle((int)input.position.X, (int)input.position.Y, off, input.useDestinationRectangle ? input.destinationRectangle.Height : input.sourceRect?.Height ?? input.texture.Height),
                    input.sourceRect, input.color, input.rotation, input.origin, effect, 0);
            }

            else if (player.HasBuff(ModContent.BuffType<Buffs.Squash>()))
                return new DrawData(input.texture, new Rectangle((int)player.position.X - 20 - (int)Main.screenPosition.X, (int)player.position.Y + 20 - (int)Main.screenPosition.Y + 1, player.width + 40, player.height - 20), input.sourceRect, input.color, input.rotation, default, input.effect, 0);

            else return input;
        }

        //custom name and icon for upgraded town NPCs
        private void SwapTitleMenu(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchLdsfld<Main>("spriteBatch"), i => i.MatchLdsfld<Main>("npcHeadTexture"), i => i.MatchLdloc(78));
            c.Index += 4; //not the safest thing ever ech

            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
            c.Emit(OpCodes.Ldloc, 71);
            c.Emit(OpCodes.Ldelem_Ref);

            c.EmitDelegate<SwapTitleMenuDelegate>(EmitSwapTitleMenuDelegate);

            c.Emit(OpCodes.Ldloc, 66);
            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
            c.Emit(OpCodes.Ldloc, 71);
            c.Emit(OpCodes.Ldelem_Ref);

            c.Emit(OpCodes.Ldloc, 73); //X and Y coords to check mouse collision. Fuck you vanilla.
            c.Emit(OpCodes.Ldloc, 74);

            c.EmitDelegate<SwapTextMenuDelegate>(EmitSwapTextMenuDelegate);

            c.Emit(OpCodes.Stloc, 66);
        }

        private delegate string SwapTextMenuDelegate(string input, NPC npc, int x, int y);

        private string EmitSwapTextMenuDelegate(string input, NPC npc, int x, int y)
        {
            bool hovering = Main.mouseX >= x && Main.mouseX <= x + Main.inventoryBackTexture.Width * Main.inventoryScale && Main.mouseY >= y && Main.mouseY <= y + Main.inventoryBackTexture.Height * Main.inventoryScale;

            if (hovering && string.IsNullOrEmpty(input) && Main.mouseItem.type == ItemID.None && StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool unlocked) && unlocked)
                return npc.GivenName + " the " + TownUpgrade.FromString(npc.TypeName)._title;
            return input;
        }

        private delegate Texture2D SwapTitleMenuDelegate(Texture2D input, NPC npc);

        private Texture2D EmitSwapTitleMenuDelegate(Texture2D input, NPC npc)
        {
            if (StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool unlocked) && unlocked)
                return TownUpgrade.FromString(npc.TypeName).icon;
            return input;
        }

        //custom "departure" message for upgraded NPCs
        private void SwapTitleDeath(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchStloc(3));
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<SwapTitleDeathDelegate>(EmitSwapTitleDeathDelegate);
        }

        private delegate NetworkText SwapTitleDeathDelegate(NetworkText input, NPC npc);

        private NetworkText EmitSwapTitleDeathDelegate(NetworkText input, NPC npc)
        {
            if (StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool unlocked) && unlocked)
                return NetworkText.FromLiteral(npc.GivenName + " the " + TownUpgrade.FromString(npc.TypeName)._title + " was slain...");
            return input;
        }

        //Custom arrival message for upgraded NPCs
        private void SwapTitle(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchStloc(8));
            c.Index++;

            c.Emit(OpCodes.Ldsfld, typeof(Main).GetField("npc", BindingFlags.Static | BindingFlags.Public));
            c.Emit(OpCodes.Ldloc, 7);
            c.Emit(OpCodes.Ldelem_Ref);

            c.Emit(OpCodes.Ldloc, 8);

            c.EmitDelegate<SwapTitleDelegate>(EmitSwapTitleDelegate);
            c.Emit(OpCodes.Stloc, 8);
        }

        private delegate string SwapTitleDelegate(NPC npc, string input);

        private string EmitSwapTitleDelegate(NPC npc, string input)
        {
            if (StarlightWorld.TownUpgrades.TryGetValue(npc.TypeName, out bool unlocked) && unlocked) return npc.GivenName + " the " + TownUpgrade.FromString(npc.TypeName)._title;
            return input;
        }

        //IL edits to allow grappling hooks to interact with moving platforms
        private void GrapplePlatforms(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            c.TryGotoNext(i => i.MatchLdfld<Projectile>("aiStyle"), i => i.MatchLdcI4(7));
            c.TryGotoNext(i => i.MatchLdfld<Projectile>("ai"), i => i.MatchLdcI4(0), i => i.MatchLdelemR4(), i => i.MatchLdcR4(2));
            c.TryGotoNext(i => i.MatchLdloc(143)); //flag2 in source code
            c.Index++;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<GrapplePlatformDelegate>(EmitGrapplePlatformDelegate);
            c.TryGotoNext(i => i.MatchStfld<Player>("grapCount"));
            c.Index++;
            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate<UngrapplePlatformDelegate>(EmitUngrapplePlatformDelegate);
        }

        private delegate bool GrapplePlatformDelegate(bool fail, Projectile proj);
        private bool EmitGrapplePlatformDelegate(bool fail, Projectile proj)
        {
            if (proj.timeLeft < 36000 - 3)
                for (int k = 0; k < Main.maxNPCs; k++)
                {
                    NPC n = Main.npc[k];
                    if (n.active && n.modNPC is NPCs.MovingPlatform && n.Hitbox.Intersects(proj.Hitbox))
                    {
                        proj.position += n.velocity;
                        return false;
                    }
                }
            return fail;
        }

        private delegate void UngrapplePlatformDelegate(Projectile proj);
        private void EmitUngrapplePlatformDelegate(Projectile proj)
        {
            Player player = Main.player[proj.owner];
            int numHooks = 3;
            //time to replicate retarded vanilla hardcoding, wheee
            if (proj.type == 165) numHooks = 8;
            if (proj.type == 256) numHooks = 2;
            if (proj.type == 372) numHooks = 2;
            if (proj.type == 652) numHooks = 1;
            if (proj.type >= 646 && proj.type <= 649) numHooks = 4;
            //end vanilla zoink

            ProjectileLoader.NumGrappleHooks(proj, player, ref numHooks);
            if (player.grapCount > numHooks) Main.projectile[player.grappling.OrderBy(n => (Main.projectile[n].active ? 0 : 999999) + Main.projectile[n].timeLeft).ToArray()[0]].Kill();
        }

        //IL edit for the conversion and spread of specialized jungle grasses
        

       
        #endregion
    }
}
