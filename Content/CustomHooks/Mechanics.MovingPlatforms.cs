using Microsoft.Xna.Framework;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using StarlightRiver.Content.NPCs.BaseTypes;
using Terraria.ID;

namespace StarlightRiver.Content.CustomHooks
{
    class MovingPlatforms : HookGroup
    {
        //Orig is called when appropriate, but this is still messing with vanilla behavior. Also IL.
        public override SafetyLevel Safety => SafetyLevel.Questionable;

        public override void Load()
        {
            On.Terraria.Player.Update_NPCCollision += PlatformCollision;

            IL.Terraria.Projectile.VanillaAI += GrapplePlatforms;
        }

        public override void Unload()
        {
            IL.Terraria.Projectile.VanillaAI -= GrapplePlatforms;
        }

        private void PlatformCollision(On.Terraria.Player.orig_Update_NPCCollision orig, Player self)
        {
            if (self.controlDown)
            {
                self.GetModPlayer<StarlightPlayer>().platformTimer = 5;
                NetMessage.SendData(MessageID.PlayerControls);
            }

            if (self.controlDown || self.GetModPlayer<StarlightPlayer>().platformTimer > 0 || self.GoingDownWithGrapple)
            {
                orig(self);
                return;
            }

            foreach (NPC npc in Main.npc)
            {
                if (!npc.active || npc.modNPC == null || !(npc.modNPC is MovingPlatform))
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

            var mp = self.GetModPlayer<GravityPlayer>();

            if (mp.controller != null && mp.controller.npc.active)
                self.velocity.Y = 0;

            orig(self);
        }

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
                    if (n.active && n.modNPC is MovingPlatform && n.Hitbox.Intersects(proj.Hitbox))
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
    }
}