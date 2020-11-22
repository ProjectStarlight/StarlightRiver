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
            //Auroracle layer
            IL.Terraria.Main.DoDraw += DrawWater;

            //soulbound items
            IL.Terraria.UI.ChestUI.DepositAll += PreventSoulboundStack;

            //dynamic map icons
            IL.Terraria.Main.DrawMap += DynamicBossIcon;

            //jungle grass
            IL.Terraria.WorldGen.Convert += JungleGrassConvert;
            IL.Terraria.WorldGen.hardUpdateWorld += JungleGrassSpread;

            //grappling hooks on moving platforms
            IL.Terraria.Projectile.VanillaAI += GrapplePlatforms;

            //Town NPC name swaps
            IL.Terraria.WorldGen.SpawnTownNPC += SwapTitle;
            IL.Terraria.NPC.checkDead += SwapTitleDeath;
            IL.Terraria.Main.DrawInventory += SwapTitleMenu;

            //Pancake debuff
            IL.Terraria.Main.DrawPlayer_DrawAllLayers += DrawPancake;
        }

        private void UnhookIL()
        {
            // Vitric lighting
            IL.Terraria.Lighting.PreRenderPhase -= VitricLighting;

            //moonlord draw layer
            IL.Terraria.Main.DoDraw -= DrawMoonlordLayer;

            //Auroracle layer
            IL.Terraria.Main.DoDraw -= DrawWater;

            //soulbound items
            IL.Terraria.UI.ChestUI.DepositAll -= PreventSoulboundStack;

            //dynamic map icons
            IL.Terraria.Main.DrawMap -= DynamicBossIcon;

            //jungle grass
            IL.Terraria.WorldGen.Convert -= JungleGrassConvert;
            IL.Terraria.WorldGen.hardUpdateWorld -= JungleGrassSpread;

            //grappling hooks on moving platforms
            IL.Terraria.Projectile.VanillaAI -= GrapplePlatforms;

            //Town NPC name swaps
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
        private void JungleGrassSpread(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            for (int k = 0; k < 3; k++)
            {
                int type;
                switch (k)
                {
                    case 0: type = ModContent.TileType<Tiles.JungleBloody.GrassJungleBloody>(); break;
                    case 2: type = ModContent.TileType<Tiles.JungleCorrupt.GrassJungleCorrupt>(); break;
                    case 1: type = ModContent.TileType<Tiles.JungleHoly.GrassJungleHoly>(); break;
                    default: type = 2; break;
                }

                for (int n = 0; n < 2; n++)
                {
                    c.TryGotoNext(i => i.MatchLdcI4(0), i => i.MatchStfld<Tile>("type"));
                    c.Index--;
                    c.Emit(OpCodes.Pop);
                    c.Emit(OpCodes.Ldc_I4, type);
                }
            }
        }

        private void JungleGrassConvert(ILContext il) //Fun stuff.
        {
            ILCursor c = new ILCursor(il);
            for (int k = 0; k < 3; k++)
            {
                int type;
                int index;
                switch (k)
                {
                    case 0: type = ModContent.TileType<Tiles.JungleBloody.GrassJungleBloody>(); break;
                    case 2: type = ModContent.TileType<Tiles.JungleCorrupt.GrassJungleCorrupt>(); break;
                    case 1: type = ModContent.TileType<Tiles.JungleHoly.GrassJungleHoly>(); break;
                    default: type = 2; break;
                }
                c.TryGotoNext(i => i.MatchLdsfld(typeof(TileID.Sets.Conversion).GetField("Grass")));
                c.TryGotoNext(i => i.MatchLdsfld(typeof(TileID.Sets.Conversion).GetField("Ice")));
                index = c.Index--;
                c.TryGotoPrev(i => i.MatchLdsfld(typeof(TileID.Sets.Conversion).GetField("Grass")));
                c.Index++;
                c.Emit(OpCodes.Pop);
                c.Emit(OpCodes.Ldc_I4, type);
                c.Emit(OpCodes.Ldloc_0);
                c.Emit(OpCodes.Ldloc_1);
                c.EmitDelegate<GrassConvertDelegate>(EmitGrassConvertDelegate);
                c.Emit(OpCodes.Brtrue, il.Instrs[index]);
                c.Emit(OpCodes.Ldsfld, typeof(TileID.Sets.Conversion).GetField("Grass"));
            }
            c.TryGotoNext(i => i.MatchLdfld<Tile>("wall"), i => i.MatchLdcI4(69)); //funny sex number!!!
            c.TryGotoPrev(i => i.MatchLdsfld<Main>("tile"));
            c.Index++;
            c.Emit(OpCodes.Ldc_I4, TileID.JungleGrass);
            c.Emit(OpCodes.Ldloc_0);
            c.Emit(OpCodes.Ldloc_1);
            c.EmitDelegate<GrassConvertDelegate>(EmitGrassConvertDelegate);
            c.Emit(OpCodes.Pop);
        }

        //IL edit for auroracle arena


        private delegate bool GrassConvertDelegate(int type, int x, int y);
        private bool EmitGrassConvertDelegate(int type, int x, int y)
        {
            Tile tile = Main.tile[x, y];
            if (tile.type == TileID.JungleGrass || tile.type == ModContent.TileType<Tiles.JungleBloody.GrassJungleBloody>() || tile.type == ModContent.TileType<Tiles.JungleCorrupt.GrassJungleCorrupt>() || tile.type == ModContent.TileType<Tiles.JungleHoly.GrassJungleHoly>())
            {
                tile.type = (ushort)type;
                return true;
            }
            return false;
        }

        //IL edit for dynamic map icons
        private void DynamicBossIcon(ILContext il)
        {
            //Hillariously repetitive IL edit to draw custom icons dynamically. Funny. Fuck vanilla.
            ILCursor c = new ILCursor(il);
            //the overlay map comes first
            c.TryGotoNext(i => i.MatchCallvirt<NPC>("GetBossHeadTextureIndex")); //only gonna break down one of these, finds a point in vanilla where we can reasonably draw boss icons

            c.EmitDelegate<DynamicIconDelegate>(EmitDynamicIconDelegateOverlay);

            c.TryGotoNext(i => i.MatchCallvirt<NPC>("GetBossHeadTextureIndex"));
            c.Index++;
            c.TryGotoNext(i => i.MatchCallvirt<NPC>("GetBossHeadTextureIndex"));
            c.Index++;
            c.TryGotoNext(i => i.MatchCallvirt<NPC>("GetBossHeadTextureIndex"));

            c.EmitDelegate<DynamicIconDelegate>(EmitDynamicIconDelegateMinimap);

            c.TryGotoNext(i => i.MatchCallvirt<NPC>("GetBossHeadTextureIndex"));
            c.Index++;
            c.TryGotoNext(i => i.MatchCallvirt<NPC>("GetBossHeadTextureIndex"));
            c.Index++;
            c.TryGotoNext(i => i.MatchCallvirt<NPC>("GetBossHeadTextureIndex"));

            c.EmitDelegate<DynamicIconDelegate>(EmitDynamicIconDelegateFullmap);
        }

        private delegate NPC DynamicIconDelegate(NPC npc);
        private NPC EmitDynamicIconDelegateOverlay(NPC npc)
        {
            if (npc?.active == true && npc.modNPC is NPCs.IDynamicMapIcon)
            {
                Vector2 npcPos = npc.Center;
                Vector2 framePos = (Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2));
                Vector2 target = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) + (npcPos - framePos) / 16 * Main.mapOverlayScale;

                float scale = (Main.mapMinimapScale * 0.25f * 2f + 1f) / 3f;

                (npc.modNPC as NPCs.IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White * Main.mapOverlayAlpha);
            }
            return npc;
        }
        private NPC EmitDynamicIconDelegateMinimap(NPC npc)
        {
            if (npc?.active == true && npc.modNPC is NPCs.IDynamicMapIcon)
            {
                Vector2 mapPos = new Vector2(Main.miniMapX, Main.miniMapY);
                Vector2 npcPos = npc.Center;
                Vector2 framePos = (Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2));
                Vector2 target = mapPos + Vector2.One * 117 + (npcPos - framePos) / 16 * Main.mapMinimapScale;

                if (target.X > Main.miniMapX + 15 && target.Y > Main.miniMapY + 15 && target.X < Main.miniMapX + 235 && target.Y < Main.miniMapY + 235) //only draw on the minimap
                {
                    float scale = (Main.mapMinimapScale * 0.25f * 2f + 1f) / 3f;

                    (npc.modNPC as NPCs.IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White);
                    if (new Rectangle((int)target.X - (int)(15 * scale), (int)target.Y - (int)(15 * scale), (int)(30 * scale), (int)(30 * scale)).Contains(Main.MouseScreen.ToPoint()))
                    {
                        Utils.DrawBorderString(Main.spriteBatch, npc.GivenOrTypeName, Main.MouseScreen + Vector2.One * 15, Main.mouseTextColorReal);
                    }
                }
            }
            return npc;
        }
        private NPC EmitDynamicIconDelegateFullmap(NPC npc)
        {
            if (npc?.active == true && npc.modNPC is NPCs.IDynamicMapIcon)
            {
                float mapScale = Main.mapFullscreenScale / Main.UIScale;

                float mapFullscrX = Main.mapFullscreenPos.X * mapScale;
                float mapFullscrY = Main.mapFullscreenPos.Y * mapScale;
                float mapX = -mapFullscrX + (Main.screenWidth / 2f);
                float mapY = -mapFullscrY + (Main.screenHeight / 2f);

                Vector2 mapPos = new Vector2(mapX, mapY);
                Vector2 npcPos = npc.Center;
                Vector2 target = mapPos + npcPos / 16 * Main.mapFullscreenScale;

                float scale = Main.UIScale;

                (npc.modNPC as NPCs.IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White);
                if (new Rectangle((int)target.X - (int)(15 * scale), (int)target.Y - (int)(15 * scale), (int)(30 * scale), (int)(30 * scale)).Contains(Main.MouseScreen.ToPoint()))
                {
                    Utils.DrawBorderString(Main.spriteBatch, npc.GivenOrTypeName, Main.MouseScreen + Vector2.One * 15, Main.mouseTextColorReal);
                }
            }
            return npc;
        }

        //IL edit to prevent quickstacking of soulbound items
        private void PreventSoulboundStack(ILContext il)
        {
            ILCursor c = new ILCursor(il);

            c.TryGotoNext(i => i.MatchLdloc(1), i => i.MatchLdcI4(1), i => i.MatchSub());
            Instruction target = c.Prev.Previous;

            c.TryGotoPrev(n => n.MatchLdfld<Item>("favorited"));
            c.Index++;

            c.Emit(OpCodes.Ldloc_0);
            c.EmitDelegate<SoulboundDelegate>(EmitSoulboundDel);
            c.Emit(OpCodes.Brtrue_S, target);
        }

        private delegate bool SoulboundDelegate(int index);
        private bool EmitSoulboundDel(int index)
        {
            return Main.LocalPlayer.inventory[index].modItem is Items.SoulboundItem;
        }
        #endregion
    }
}
