using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.NPCs.BaseTypes;
using StarlightRiver.Core;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace StarlightRiver.Content.Bosses.SquidBoss
{
	class IcePlatform : MovingPlatform
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;

        public override void SafeSetDefaults()
        {
            NPC.width = 200;
            NPC.height = 32;
        }

        public override void SafeAI()
        {
            if (NPC.ai[2] == 0) NPC.ai[2] = NPC.position.Y;

            if (NPC.ai[3] != 0)
            {
                if (NPC.ai[3] > 360) NPC.position.Y += 8;

                if (NPC.ai[3] <= 90 && NPC.ai[3] > 0) NPC.position.Y -= 9;

                NPC.ai[3]--;

                return;
            }

            if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<ArenaActor>()))
            {
                ArenaActor actor = Main.npc.FirstOrDefault(n => n.active && n.type == ModContent.NPCType<ArenaActor>()).ModNPC as ArenaActor;

                if (NPC.position.Y >= NPC.ai[2]) NPC.ai[1] = 0;

                if (NPC.position.Y + 18 >= actor.WaterLevel) NPC.ai[1] = 1;

                if (NPC.ai[1] == 1 && (!Main.tile[(int)NPC.Center.X / 16, (int)NPC.Center.Y / 16 - 5].HasTile || actor.WaterLevel - 18 > NPC.position.Y)) NPC.position.Y = actor.WaterLevel - 18;
            }
        }
    }

    class IcePlatformSmall : MovingPlatform, IUnderwater
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

        public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
        {
            spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, NPC.position - Main.screenPosition, Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16));
        }

        public override void SafeSetDefaults()
        {
            NPC.width = 100;
            NPC.height = 20;
        }

        public override void SafeAI()
        {
            if (NPC.ai[0] == 0) NPC.ai[0] = NPC.position.Y;

            if (BeingStoodOn)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, Terraria.ID.DustID.Ice);
                NPC.ai[1]++;
            }
            else if (NPC.ai[1] > 0) 
                NPC.ai[1]--;

            int maxStandTime = Main.masterMode ? 2 : 20;

            if (NPC.ai[1] >= maxStandTime) 
                NPC.velocity.Y += 0.3f;
            else if (NPC.position.Y > NPC.ai[0]) 
                NPC.velocity.Y = -1;
            else 
                NPC.velocity.Y = 0;
        }
    }

    class GoldPlatform : MovingPlatform, IUnderwater
    {
        public override string Texture => AssetDirectory.SquidBoss + Name;
        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) => false;

        public void DrawUnderWater(SpriteBatch spriteBatch, int NPCLayer)
        {
            spriteBatch.Draw(ModContent.Request<Texture2D>(Texture).Value, NPC.position - Main.screenPosition, Lighting.GetColor((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16));
        }

        public override void SafeSetDefaults()
        {
            NPC.width = 200;
            NPC.height = 20;
        }

        public override void SafeAI()
        {
            if (NPC.ai[0] == 0) NPC.ai[0] = NPC.position.Y;

            if (BeingStoodOn && StarlightWorld.HasFlag(WorldFlags.SquidBossOpen))
            {
                if (NPC.velocity.Y < 1.5f) NPC.velocity.Y += 0.02f;

                if (NPC.position.Y - NPC.ai[0] > 1600) NPC.velocity.Y = 0;
            }
            else if (NPC.position.Y > NPC.ai[0]) NPC.velocity.Y = -6;
            else NPC.velocity.Y = 0;
        }
    }
}
