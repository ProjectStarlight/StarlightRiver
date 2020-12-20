using Microsoft.Xna.Framework;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core;

namespace StarlightRiver.Content.Bosses.GlassBoss
{
    internal class VitricBossPlatformUp : MovingPlatform
    {
        public const int MaxHeight = 880;
        public override string Texture => Directory.GlassBossDir + "VitricBossPlatform";

        public override bool CheckActive() => false;
        public override void DrawEffects(ref Color drawColor) => drawColor *= 1.4f;

        public override void SafeSetDefaults()
        {
            npc.width = 220;
            npc.height = 16;
            npc.noTileCollide = true;
            npc.dontCountMe = true;
        }

        public override void SafeAI()
        {
            /*AI fields:
             * 0: state
             * 1: rise time left
             */

            if (npc.ai[0] == 0)
                if (npc.ai[1] > 0)
                {
                    npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Risetime;
                    npc.ai[1]--;
                }
                else npc.velocity.Y = 0;

            if (npc.ai[0] == 1)
            {
                npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Scrolltime * 0.999f;
                if (npc.position.Y <= StarlightWorld.VitricBiome.Y * 16 + 16 * 16)
                    npc.position.Y += MaxHeight;
            }
        }
    }

    internal class VitricBossPlatformDown : VitricBossPlatformUp
    {
        public override void SafeAI()
        {
            /*AI fields:
             * 0: state
             * 1: rise time left
             */

            if (npc.ai[0] == 0)
                if (npc.ai[1] > 0)
                {
                    npc.velocity.Y = -(float)MaxHeight / VitricBackdropLeft.Risetime;
                    npc.ai[1]--;
                }
                else npc.velocity.Y = 0;

            if (npc.ai[0] == 1)
            {
                npc.velocity.Y = (float)MaxHeight / VitricBackdropLeft.Scrolltime * 0.999f;
                if (npc.position.Y >= StarlightWorld.VitricBiome.Y * 16 + 16 * 16 + MaxHeight)
                    npc.position.Y -= MaxHeight;
            }
        }
    }

    internal class VitricBossPlatformUpSmall : VitricBossPlatformUp
    {
        public override string Texture => Directory.GlassBossDir + "VitricBossPlatformSmall";

        public override void SafeSetDefaults()
        {
            npc.width = 100;
            npc.height = 16;
            npc.noTileCollide = true;
            npc.dontCountMe = true;
        }
    }

    internal class VitricBossPlatformDownSmall : VitricBossPlatformDown
    {
        public override string Texture => Directory.GlassBossDir + "VitricBossPlatformSmall";

        public override void SafeSetDefaults()
        {
            npc.width = 100;
            npc.height = 16;
            npc.noTileCollide = true;
            npc.dontCountMe = true;
        }
    }
}