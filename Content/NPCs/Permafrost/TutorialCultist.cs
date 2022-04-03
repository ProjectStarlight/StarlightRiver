using Microsoft.Xna.Framework;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Permafrost
{
	public class TutorialCultist : ModNPC
    {
        int textState = 0;

        public override string Texture => "StarlightRiver/Assets/NPCs/Permafrost/TutorialCultist";

        public override void SetDefaults()
        {
            NPC.townNPC = true;
            NPC.friendly = true;
            NPC.width = 18;
            NPC.height = 40;
            NPC.aiStyle = -1;
            NPC.damage = 10;
            NPC.defense = 15;
            NPC.lifeMax = 250;
            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.knockBackResist = 0.5f;
            NPC.dontTakeDamage = true;
            NPC.dontCountMe = true;
        }

        public override string GetChat()
        {
            textState = 0;
            UILoader.GetUIState<RichTextBox>().Visible = true;
            RichTextBox.ClearButtons();

            SetData();
            RichTextBox.AddButton("[]Next", Debug);

            return "";
        }

        private void Debug()
        {
            textState++;
        }

        private void Close()
        {
            UILoader.GetUIState<RichTextBox>().Visible = false;
            RichTextBox.SetData(null, "", "");
            Main.player[Main.myPlayer].talkNPC = -1;
            textState = 0;
        }

        public override void AI()
        {
            if (RichTextBox.talking == NPC)
            {
                SetData();

                if (Main.player[Main.myPlayer].talkNPC != NPC.whoAmI)
                {
                    UILoader.GetUIState<RichTextBox>().Visible = false;
                    RichTextBox.SetData(null, "", "");
                }
            }
        }

        private void SetData()
        {
            switch (StarlightWorld.SquidNPCProgress) //yeah, I really should make a full system for this with localization files and everything, and not hardcode this into huge gross switch statements. Remind me to do that at some point. TODO
            {
                case 0: //initial encounter, may be on the surface or in the lower temples. This is only ever skipped if your first encounter happens to be already in the permafrost (unlikely)
                    switch (textState)
                    {
                        case 0:
                            RichTextBox.SetData(NPC, "[PH]Tutorial Cultist",
                            "[]This message should appear on first visit"
                            ); break;

                        case 1:
                            RichTextBox.SetData(NPC, "[PH]Tutorial Cultist",
                            "[]This message is the second one to appear on first visit"
                            ); break;

                        default:
                            Close();
                            StarlightWorld.SquidNPCProgress = 1; //move to next quotes
                            break;
                    } break;

                case 1: //example case for a second encounter
                    switch (textState)
                    {
                        case 0:
                            RichTextBox.SetData(NPC, "[PH]Tutorial Cultist",
                            "[]This message should appear on the next visit"
                            ); break;

                        case 1:
                            RichTextBox.SetData(NPC, "[PH]Tutorial Cultist",
                            "[]This message too... i guess"
                            ); break;

                        default:
                            Close();
                            break;
                    }
                    break;

                case -1: //when he is found in the permafrost, this ends his """quest"""
                    switch (textState)
                    {
                        case 0:
                            RichTextBox.SetData(NPC, "[PH]Tutorial Cultist",
                            "[]damn found me"
                            ); break;

                        case 1:
                            RichTextBox.SetData(NPC, "[PH]Tutorial Cultist",
                            "[]Bend over."
                            ); break;

                        default:
                            Close();
                            break;
                    }
                    break;
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
