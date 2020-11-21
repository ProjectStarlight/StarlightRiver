using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using StarlightRiver.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.CustomHooks
{
    class PlayerCollisionHandling : HookGroup
    {
        //Orig is called when appropriate, but this is still messing with vanilla behavior.
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public override void Load()
        {
            On.Terraria.Player.Update_NPCCollision += PlatformCollision;
        }

        private void PlatformCollision(On.Terraria.Player.orig_Update_NPCCollision orig, Player self)
        {
            // TODO this needs synced somehow
            if (self.controlDown)
                self.GetModPlayer<StarlightPlayer>().platformTimer = 5;

            if (self.controlDown || self.GetModPlayer<StarlightPlayer>().platformTimer > 0 || self.GoingDownWithGrapple)
            {
                orig(self);
                return;
            }

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.modNPC == null || !(npc.modNPC is NPCs.MovingPlatform))
                    continue;

                Rectangle playerRect = new Rectangle((int)self.position.X, (int)self.position.Y + (self.height), self.width, 1);
                Rectangle npcRect = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, 8 + (self.velocity.Y > 0 ? (int)self.velocity.Y : 0));

                if (playerRect.Intersects(npcRect) && self.position.Y <= npc.position.Y)
                {
                    if (!self.justJumped && self.velocity.Y >= 0)
                    {
                        self.gfxOffY = npc.gfxOffY;
                        self.velocity.Y = 0;
                        self.fallStart = (int)(self.position.Y / 16f);
                        self.position.Y = npc.position.Y - self.height + 4;
                        orig(self);
                    }
                }
            }

            var mp = self.GetModPlayer<NPCs.GravityPlayer>();

            if (mp.controller != null && mp.controller.npc.active)
                self.velocity.Y = 0;

            orig(self);
        }
    }
}