using Microsoft.Xna.Framework;
using MonoMod.Cil;
using StarlightRiver.Core;
using Terraria;

namespace StarlightRiver.Content.CustomHooks
{
	class DynamicMapIcons : HookGroup
    {
        //Just drawing, so it shooould be alright? not the best finding though. not the best at all.
        public override SafetyLevel Safety => SafetyLevel.Fragile;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            IL.Terraria.Main.DrawMap += DynamicBossIcon;
        }

        public override void Unload()
        {
            IL.Terraria.Main.DrawMap -= DynamicBossIcon;
        }

        private void DynamicBossIcon(ILContext il)
        {
            //Hillariously repetitive IL edit to draw custom icons dynamically.
            ILCursor c = new ILCursor(il);
            //the overlay map comes first
            c.TryGotoNext(i => i.MatchCallvirt<NPC>("GetBossHeadTextureIndex"));

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

        private delegate NPC DynamicIconDelegate(NPC NPC);

        private NPC EmitDynamicIconDelegateOverlay(NPC NPC)
        {
            if (NPC?.active == true && NPC.ModNPC is IDynamicMapIcon)
            {
                Vector2 NPCPos = NPC.Center;
                Vector2 framePos = (Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2));
                Vector2 target = new Vector2(Main.screenWidth / 2, Main.screenHeight / 2) + (NPCPos - framePos) / 16 * Main.mapOverlayScale;

                float scale = (Main.mapMinimapScale * 0.25f * 2f + 1f) / 3f;

                (NPC.ModNPC as IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White * Main.mapOverlayAlpha);
            }
            return NPC;
        }

        private NPC EmitDynamicIconDelegateMinimap(NPC NPC)
        {
            if (NPC?.active == true && NPC.ModNPC is IDynamicMapIcon)
            {
                Vector2 mapPos = new Vector2(Main.miniMapX, Main.miniMapY);
                Vector2 NPCPos = NPC.Center;
                Vector2 framePos = (Main.screenPosition + new Vector2(Main.screenWidth / 2, Main.screenHeight / 2));
                Vector2 target = mapPos + Vector2.One * 117 + (NPCPos - framePos) / 16 * Main.mapMinimapScale;

                if (target.X > Main.miniMapX + 15 && target.Y > Main.miniMapY + 15 && target.X < Main.miniMapX + 235 && target.Y < Main.miniMapY + 235) //only draw on the minimap
                {
                    float scale = (Main.mapMinimapScale * 0.25f * 2f + 1f) / 3f;

                    (NPC.ModNPC as IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White);

                    if (new Rectangle((int)target.X - (int)(15 * scale), (int)target.Y - (int)(15 * scale), (int)(30 * scale), (int)(30 * scale)).Contains(Main.MouseScreen.ToPoint()))
                    {
                        Utils.DrawBorderString(Main.spriteBatch, NPC.GivenOrTypeName, Main.MouseScreen + Vector2.One * 15, Main.MouseTextColorReal);
                    }
                }
            }
            return NPC;
        }

        private NPC EmitDynamicIconDelegateFullmap(NPC NPC)
        {
            if (NPC?.active == true && NPC.ModNPC is IDynamicMapIcon)
            {
                float mapScale = Main.mapFullscreenScale / Main.UIScale;

                float mapFullscrX = Main.mapFullscreenPos.X * mapScale;
                float mapFullscrY = Main.mapFullscreenPos.Y * mapScale;
                float mapX = -mapFullscrX + (Main.screenWidth / 2f);
                float mapY = -mapFullscrY + (Main.screenHeight / 2f);

                Vector2 mapPos = new Vector2(mapX, mapY);
                Vector2 NPCPos = NPC.Center;
                Vector2 target = mapPos + NPCPos / 16 * Main.mapFullscreenScale;

                float scale = Main.UIScale;

                (NPC.ModNPC as IDynamicMapIcon).DrawOnMap(Main.spriteBatch, target, scale, Color.White);

                if (new Rectangle((int)target.X - (int)(15 * scale), (int)target.Y - (int)(15 * scale), (int)(30 * scale), (int)(30 * scale)).Contains(Main.MouseScreen.ToPoint()))
                {
                    Utils.DrawBorderString(Main.spriteBatch, NPC.GivenOrTypeName, Main.MouseScreen + Vector2.One * 15, Main.MouseTextColorReal);
                }
            }
            return NPC;
        }
    }
}