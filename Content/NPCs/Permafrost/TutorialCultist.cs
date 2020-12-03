using Microsoft.Xna.Framework;
using StarlightRiver.Buffs;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

using StarlightRiver.Core;
using StarlightRiver.Content.GUI;

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
            UILoader.GetUIState<RichTextBox>().Visible = true;
            RichTextBox.ClearButtons();

            SetData();
            RichTextBox.AddButton("[]Next", Debug);

            return "";
        }

        private void Debug()
        {
            textState++;

            if (textState == 3)
            {
                RichTextBox.ClearButtons();
                RichTextBox.AddButton("[]Recieve Blessing", Bless);
            }
        }

        private void Bless()
        {
            UILoader.GetUIState<RichTextBox>().Visible = false;
            RichTextBox.SetData(null, "", "");
            Main.player[Main.myPlayer].talkNPC = -1;

            Main.LocalPlayer.AddBuff(BuffType<SpikeImmuneBuff>(), 3600); //TODO: this may need manual sync later? not sure (Remember this is really being called from a UI)
        }

        public override void AI()
        {
            if (RichTextBox.talking == npc)
            {
                SetData();

                if (Main.player[Main.myPlayer].talkNPC != npc.whoAmI)
                {
                    UILoader.GetUIState<RichTextBox>().Visible = false;
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
                        "[]Hello hello generic greeting message thing"
                    ); break;

                case 1:
                    RichTextBox.SetData(npc, "[PH]Tutorial Cultist",
                        "[]You can pick up those" +
                        MakeAuroraText("Magical Orbs ") +
                        "[]To shield yourself from" +
                        "[<color:255, 80, 80>]spikes" +
                        "[]. Be careful though, they wont come back untill the" +
                        "[<color:120, 160, 255>]next night"
                    ); break;

                case 2:
                    RichTextBox.SetData(npc, "[PH]Tutorial Cultist",
                        "[]Something Something scary ominous" +
                        "[<color:255,0,0>]WARNING!!!" +
                        "[]something something dont touch squigga"
                    ); break;

                case 3:
                    RichTextBox.SetData(npc, "[PH]Tutorial Cultist",
                        "[]Ooohh let me give you magic no spike spell!" +
                        MakeAuroraText("OohhHhhhh gimmie them tiddies bitch!")
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
