using Microsoft.Xna.Framework;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Town
{
    class EnchantNPC : ModNPC
    {
        int textState = 0;
        bool enchanting;

        public override string Texture => "StarlightRiver/Assets/NPCs/Town/EnchantNPC";

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
            RichTextBox.AddButton("[]Chat", Chat);
            RichTextBox.AddButton("[<color:200, 140, 255>]Enchant", OpenEnchantUI);
            RichTextBox.AddButton("[]Move Altar", PackUp);

            return "";
        }

        private void Chat()
        {
            textState = Main.rand.Next(1, 5);
        }

        private void OpenEnchantUI()
        {
            Main.NewText("IMPLEMENT THE UI YOU LAZY BITCH");
            ZoomHandler.SetZoomAnimation(2.5f, 60);
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = npc.Center + new Vector2(0, -200);
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 120;
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveHold = true;
            enchanting = true;
        }

        private void PackUp()
        {
            Main.NewText("IMPLEMENT THE STRUCTURE YOU LAZY BITCH");
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
                    RichTextBox.SetData(npc, "[PH]Enchantress",
                        "[]Hello hello generic greeting message thing"
                    ); break;

                case 1:
                    RichTextBox.SetData(npc, "[PH]Enchantress",
                        "[]Generic Chat Text 1"
                    ); break;

                case 2:
                    RichTextBox.SetData(npc, "[PH]Enchantress",
                        "[]Generic Chat Text 2"
                    ); break;

                case 3:
                    RichTextBox.SetData(npc, "[PH]Enchantress",
                        "[]Generic Chat Text 3"
                    ); break;

                case 4:
                    RichTextBox.SetData(npc, "[PH]Enchantress",
                        "[]Generic Chat Text 4"
                    ); break;
            }
        }
    }
}
