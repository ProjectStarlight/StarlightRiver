using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

using StarlightRiver.Core;
using StarlightRiver.Content.Abilities;

namespace StarlightRiver.Content.CustomHooks
{
    class CharacterSelectAddons : HookGroup
    {
        //Relies on other mods not doing anything visual here to work properly, but there is very little actual danger here, its just some extra draw calls in the UI element. 
        public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            On.Terraria.GameContent.UI.Elements.UICharacterListItem.DrawSelf += DrawSpecialCharacter;
        }

        //TODO: Create a reflection cache for this, or implement a global reflection caching utility
        private readonly FieldInfo _playerPanel = typeof(UICharacterListItem).GetField("_playerPanel", BindingFlags.NonPublic | BindingFlags.Instance);
        private readonly FieldInfo _player = typeof(UICharacter).GetField("_player", BindingFlags.NonPublic | BindingFlags.Instance);

        private void DrawSpecialCharacter(On.Terraria.GameContent.UI.Elements.UICharacterListItem.orig_DrawSelf orig, UICharacterListItem self, SpriteBatch spriteBatch)
        {
            orig(self, spriteBatch);
            Vector2 origin = new Vector2(self.GetDimensions().X, self.GetDimensions().Y);

            //hooray double reflection, fuck you vanilla
            UICharacter character = (UICharacter)_playerPanel.GetValue(self);

            Player player = (Player)_player.GetValue(character);
            AbilityHandler mp = player.GetHandler();
            CodexHandler mp2 = player.GetModPlayer<CodexHandler>();

            if (mp == null || mp2 == null) { return; }

            float playerStamina = mp.StaminaMax;

            Rectangle box = new Rectangle((int)(origin + new Vector2(86, 66)).X, (int)(origin + new Vector2(86, 66)).Y, 80, 25);
            Rectangle box2 = new Rectangle((int)(origin + new Vector2(172, 66)).X, (int)(origin + new Vector2(86, 66)).Y, 104, 25);

            spriteBatch.Draw(ModContent.GetTexture("StarlightRiver/Assets/GUI/box"), box, Color.White); //Stamina box
            spriteBatch.Draw(ModContent.GetTexture("StarlightRiver/Assets/GUI/box"), box2, Color.White); //Codex box

            if (mp.AnyUnlocked)//Draw stamina if any unlocked
            {
                spriteBatch.Draw(ModContent.GetTexture("StarlightRiver/Assets/GUI/Stamina"), origin + new Vector2(91, 68), Color.White);
                Utils.DrawBorderString(spriteBatch, playerStamina + " SP", origin + new Vector2(118, 68), Color.White);
            }
            else//Myserious if locked
            {
                spriteBatch.Draw(ModContent.GetTexture("StarlightRiver/Assets/GUI/Stamina3"), origin + new Vector2(91, 68), Color.White);
                Utils.DrawBorderString(spriteBatch, "???", origin + new Vector2(118, 68), Color.White);
            }

            if (mp2.CodexState != 0)//Draw codex percentage if unlocked
            {
                var bookTex = mp2.CodexState == 2 ? ("StarlightRiver/Assets/GUI/Book2Closed") : ("StarlightRiver/Assets/GUI/Book1Closed");
                var drawTex = ModContent.GetTexture(bookTex);
                int percent = (int)(mp2.Entries.Count(n => !n.Locked) / (float)mp2.Entries.Count * 100f);
                spriteBatch.Draw(drawTex, origin + new Vector2(178, 60), Color.White);
                Utils.DrawBorderString(spriteBatch, percent + "%", origin + new Vector2(212, 68), percent >= 100 ? new Color(255, 205 + (int)(Math.Sin(Main.time / 50000 * 100) * 40), 50) : Color.White);
            }
            else//Mysterious if locked
            {
                spriteBatch.Draw(ModContent.GetTexture("StarlightRiver/Assets/GUI/BookLocked"), origin + new Vector2(178, 60), Color.White * 0.4f);
                Utils.DrawBorderString(spriteBatch, "???", origin + new Vector2(212, 68), Color.White);
            }


            var abilities = Ability.GetAbilityInstances();

            //Draw ability Icons
            for (int k = 0; k < abilities.Length; k++)
            {
                try
                {
                    var ability = abilities[(abilities.Length - 1) - k];
                    var texture = player.GetHandler().Unlocked(ability.GetType())
                        ? ability.Texture
                        : ability.TextureLocked;
                    spriteBatch.Draw(ModContent.GetTexture(texture), origin + new Vector2(536 - k * 32, 62), Color.White);
                }

                catch (Exception e) { }
            }


            if (player.statLifeMax > 400) //why vanilla dosent do this I dont know
            {
                spriteBatch.Draw(Main.heart2Texture, origin + new Vector2(80, 37), Color.White);
            }
        }
    }
}
