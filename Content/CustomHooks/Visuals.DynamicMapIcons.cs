using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;

using StarlightRiver.Core;

namespace StarlightRiver.Content.CustomHooks
{
    class DynamicMapIcons : HookGroup
    {
        //Just drawing, so it shooould be alright? not the best finding though. not the best at all.
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            IL.Terraria.Main.DrawMap += DynamicBossIcon;
        }

        public override void Unload()
        {
            IL.Terraria.Main.DrawMap -= DynamicBossIcon;
        }

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
            if (npc?.active == true && npc.modNPC is IDynamicMapIcon)
            {
                Vector2 npcPos = npc.Center;
                Vector2 framePos = (Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2));
                Vector2 target = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) + (npcPos - framePos) / 16 * Main.mapOverlayScale;

                float scale = (Main.mapMinimapScale * 0.25f * 2f + 1f) / 3f;

                (npc.modNPC as IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White * Main.mapOverlayAlpha);
            }
            return npc;
        }

        private NPC EmitDynamicIconDelegateMinimap(NPC npc)
        {
            if (npc?.active == true && npc.modNPC is IDynamicMapIcon)
            {
                Vector2 mapPos = new Vector2(Main.miniMapX, Main.miniMapY);
                Vector2 npcPos = npc.Center;
                Vector2 framePos = (Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2));
                Vector2 target = mapPos + Vector2.One * 117 + (npcPos - framePos) / 16 * Main.mapMinimapScale;

                if (target.X > Main.miniMapX + 15 && target.Y > Main.miniMapY + 15 && target.X < Main.miniMapX + 235 && target.Y < Main.miniMapY + 235) //only draw on the minimap
                {
                    float scale = (Main.mapMinimapScale * 0.25f * 2f + 1f) / 3f;

                    (npc.modNPC as IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White);

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
            if (npc?.active == true && npc.modNPC is IDynamicMapIcon)
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

                (npc.modNPC as IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White);

                if (new Rectangle((int)target.X - (int)(15 * scale), (int)target.Y - (int)(15 * scale), (int)(30 * scale), (int)(30 * scale)).Contains(Main.MouseScreen.ToPoint()))
                {
                    Utils.DrawBorderString(Main.spriteBatch, npc.GivenOrTypeName, Main.MouseScreen + Vector2.One * 15, Main.mouseTextColorReal);
                }
            }
            return npc;
        }
    }
}