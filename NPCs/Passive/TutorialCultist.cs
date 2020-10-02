using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using StarlightRiver.Core;
using StarlightRiver.GUI;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace StarlightRiver.NPCs.Passive
{
    public class TutorialCultist : ModNPC
    {
        int textState = 0;

        public override void SetDefaults()
        {
            npc.townNPC = true;
            npc.friendly = true;
            npc.width = 18;
            npc.height = 40;
            npc.aiStyle = -1;
            npc.damage = 10;
            npc.defense = 15;
            npc.lifeMax = 250;
            npc.HitSound = SoundID.NPCHit1;
            npc.DeathSound = SoundID.NPCDeath1;
            npc.knockBackResist = 0.5f;
            npc.dontTakeDamage = true;
            npc.dontCountMe = true;
        }

        public override string GetChat()
        {
            textState = 0;
            RichTextBox.visible = true;
            RichTextBox.ClearButtons();

            SetData();
            RichTextBox.AddButton("[]Next", Debug);

            return "";
        }

        private void Debug()
        {
            textState++;

            if(textState == 1)
            {
                RichTextBox.ClearButtons();
            }
        }

        public override void AI()
        {
            if (RichTextBox.talking == npc)
            {
                SetData();

                if (Main.player[Main.myPlayer].talkNPC != npc.whoAmI)
                {
                    RichTextBox.visible = false;
                    RichTextBox.SetData(null, "", "");
                }
            }
        }

        private void SetData()
        {
            switch (textState)
            {
                case 0:
                RichTextBox.SetData(npc, "[PH]Tutorial Cultist",
                    "[]Something soemthing clever lore something." +
                    "[]You can pick up those" +
                    MakeAuroraText("Magical Orbs ") +
                    "[]To shield yourself from" +
                    "[<color:255, 80, 80>]spikes" +
                    "[]. Be careful though, they wont come back untill the" +
                    "[<color:120, 160, 255>]next night" +
                    "[]Here have alot more text in able to test the wrapping and to make sure that the height measurement works correctly for button placement. la da dada a adada dasij oaiwhapo afhqwhpfjn aoihjaw oihjw ijna"
                ); break;

                case 1:
                    RichTextBox.SetData(npc, "[PH]Tutorial Cultist",
                        "[]This is what I would say next if I had more dialogue"
                    ); break;

            }
        }

        private string MakeAuroraText(string message)
        {
            string output = "";
            for (int k = 0; k < message.Length; k++)
            {
                float sin = 1 + (float)Math.Sin(-StarlightWorld.rottime + k / 3);
                float cos = 1 + (float)Math.Cos(-StarlightWorld.rottime + k / 3);
                Color color = new Color(0.5f + cos * 0.2f, 0.8f, 0.5f + sin * 0.2f) * 1.1f;

                Vector2 off = new Vector2(0, (float)(Math.Sin(-StarlightWorld.rottime * 2 + k / 2f) * 2));

                output += Markdown.SetCharMarkdown(message[k], color, off, Vector3.UnitX);
            }
            return output;
        }
    }
}
