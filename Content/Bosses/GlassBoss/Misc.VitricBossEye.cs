using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.NPCs.Boss.VitricBoss
{
    internal class VitricBossEye
    {
        private Vector2 Position;
        public VitricBoss Parent;
        private int Timer;
        private readonly int Index;

        public VitricBossEye(Vector2 pos, int index)
        {
            Position = pos;
            Index = index;
        }

        public void Draw(SpriteBatch sb)
        {
            if (Parent == null) return;
            Texture2D tex = GetTexture("StarlightRiver/NPCs/Boss/VitricBoss/VitricBossEye");
            float rot = (Parent.npc.position + Position - Main.player[Parent.npc.target].Center).ToRotation();
            if (Parent.npc.target >= 200) rot = 0;
            Color color = new Color(160, 220, 250);
            if ((Parent.npc.ai[0] > 360 || Timer >= 1) && Timer < 15) Timer++;
            if (Parent.npc.ai[1] != (int)VitricBoss.AIStates.SpawnAnimation && Parent.npc.ai[0] % (120) == Index * 6) Timer = 1;
            if (Parent.npc.ai[1] == (int)VitricBoss.AIStates.Anger)
            {
                rot = StarlightWorld.rottime * 4 + Index / 7f;
                color = new Color(255, 120, 120);
                Timer = 15;
            }

            sb.Draw(tex, Parent.npc.position + Position + new Vector2(-1, 0).RotatedBy(rot) * 5 - Main.screenPosition, tex.Frame(), color, 0, tex.Size() / 2, Timer / 15f, 0, 0);
        }
    }
}