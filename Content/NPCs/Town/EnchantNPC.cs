using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Content.GUI;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace StarlightRiver.Content.NPCs.Town
{
	class EnchantNPC : ModNPC
    {
        int textState = 0;
        public bool enchanting;

        public override string Texture => "StarlightRiver/Assets/NPCs/Town/EnchantNPC";

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
            EnchantmentMenu.SetActive(NPC.Center + new Vector2(0, -300), this);
            //ZoomHandler.SetZoomAnimation(2.5f, 60);
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTarget = NPC.Center + new Vector2(0, -300);
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveTime = 120;
            Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveHold = true;
            Main.LocalPlayer.SetTalkNPC(-1);
            enchanting = true;

            UILoader.GetUIState<RichTextBox>().Visible = false;
        }

        private void PackUp()
        {
            Main.NewText("IMPLEMENT THE STRUCTURE YOU LAZY BITCH");
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

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) //Temporary solution untill this can be drawn by the structure
        {
            spriteBatch.Draw(ModContent.Request<Texture2D>(AssetDirectory.GUI + "EnchantOver").Value, NPC.Center + new Vector2(0, -300) - Main.screenPosition, null, Color.White, 0, Vector2.One * 160, 1, 0, 0);

            if(!enchanting)
                spriteBatch.Draw(ModContent.Request<Texture2D>(AssetDirectory.GUI + "EnchantSlotClosed").Value, NPC.Center + new Vector2(0, -500) - Main.screenPosition, new Rectangle(0, 0, 34, 34), Color.White, 0, Vector2.One * 17, 1, 0, 0);

            return true;
        }

        private void SetData()
        {
            switch (textState)
            {
                case 0:
                    RichTextBox.SetData(NPC, "[PH]Enchantress",
                        "[]Hello hello generic greeting message thing"
                    ); break;

                case 1:
                    RichTextBox.SetData(NPC, "[PH]Enchantress",
                        "[]Generic Chat Text 1"
                    ); break;

                case 2:
                    RichTextBox.SetData(NPC, "[PH]Enchantress",
                        "[]Generic Chat Text 2"
                    ); break;

                case 3:
                    RichTextBox.SetData(NPC, "[PH]Enchantress",
                        "[]Generic Chat Text 3"
                    ); break;

                case 4:
                    RichTextBox.SetData(NPC, "[PH]Enchantress",
                        "[]Generic Chat Text 4"
                    ); break;
            }
        }
    }
}
