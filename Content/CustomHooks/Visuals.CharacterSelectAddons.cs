using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Codex;
using StarlightRiver.Content.Abilities;
using StarlightRiver.Core;
using System;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.IO;
using Terraria.ModLoader;

namespace StarlightRiver.Content.CustomHooks
{
	class CharacterSelectAddons : HookGroup
    {
        public static ParticleSystem sparkles = new ParticleSystem(AssetDirectory.GUI + "Sparkle", updateSparkles);

		//Relies on other mods not doing anything visual here to work properly, but there is very little actual danger here, its just some extra draw calls in the UI element. 
		public override SafetyLevel Safety => SafetyLevel.Safe;

        public override void Load()
        {
            if (Main.dedServ)
                return;

            On.Terraria.GameContent.UI.Elements.UICharacterListItem.DrawSelf += DrawSpecialCharacter;
            On.Terraria.GameContent.UI.Elements.UICharacterListItem.ctor += ShiftTextOver;
        }

        private static void updateSparkles(Particle particle)
        {
            particle.Timer--;

            particle.Scale = (float)Math.Sin(particle.Timer / 60f * 3.14f) * particle.StoredPosition.X;

            particle.Color = new Color(255, 230, (byte)(Math.Sin(particle.Timer / 60f * 3.14f) * 200)) * (float)Math.Sin(particle.Timer / 60f * 3.14f);

            particle.Rotation += 0.15f;
        }

        private void ShiftTextOver(On.Terraria.GameContent.UI.Elements.UICharacterListItem.orig_ctor orig, UICharacterListItem self, PlayerFileData data, int snapPointIndex)
		{
            orig(self, data, snapPointIndex);

            FieldInfo info = typeof(UICharacterListItem).GetField("_buttonLabel", BindingFlags.Instance | BindingFlags.NonPublic);
            UIText text = info.GetValue(self) as UIText;

            text.Left.Set(280, 0);
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

            player.UpdateEquips(0);

            float playerStamina = mp.StaminaMaxDefault;
            int codexProgress = (int)(mp2.Entries.Count(n => !n.Locked) / (float)mp2.Entries.Count * 100f);

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
                spriteBatch.Draw(drawTex, origin + new Vector2(178, 60), Color.White);
                Utils.DrawBorderString(spriteBatch, codexProgress + "%", origin + new Vector2(212, 68), codexProgress >= 100 ? new Color(255, 205 + (int)(Math.Sin(Main.time / 50000 * 100) * 40), 50) : Color.White);
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
                var ability = abilities[(abilities.Length - 1) - k];
                var texture = player.GetHandler().Unlocked(ability.GetType())
                    ? ability.PreviewTexture
                    : ability.PreviewTextureOff;
                spriteBatch.Draw(ModContent.GetTexture(texture), origin + new Vector2(540 - k * 32, 64), Color.White);
            }

            if (player.statLifeMax > 400) //why vanilla dosent do this I dont know
            {
                spriteBatch.Draw(Main.heart2Texture, origin + new Vector2(80, 37), Color.White);
            }

            if(player.GetModPlayer<ShieldPlayer>().MaxShield > 0)
			{
                var barrierTex = ModContent.GetTexture(AssetDirectory.GUI + "ShieldHeartOver");
                var barrierTex2 = ModContent.GetTexture(AssetDirectory.GUI + "ShieldHeart");
                var barrierTex3 = ModContent.GetTexture(AssetDirectory.GUI + "ShieldHeartLine");

                var pos = origin + new Vector2(80, 37);
                var width = barrierTex.Width / 2;
                var source = new Rectangle(0, 0, width, barrierTex.Height);
                var target = new Rectangle((int)pos.X, (int)pos.Y, width, barrierTex.Height);
                var lineTarget = new Rectangle((int)pos.X + width - 2, (int)pos.Y, 2, barrierTex.Height);
                var lineSource = new Rectangle(width - 2, 0, 2, barrierTex.Height);

                spriteBatch.Draw(barrierTex2, target, source, Color.White * 0.25f);
                spriteBatch.Draw(barrierTex, target, source, Color.White);
                spriteBatch.Draw(barrierTex3, lineTarget, lineSource, Color.White);
            }

            if( //player is "complete"
                player.statLifeMax == 500 &&
                player.statManaMax == 200 && 
                playerStamina == 5 && 
                codexProgress == 100 &&
                !abilities.Any(n => !player.GetHandler().Unlocked(n.GetType()))
                )
			{
                var borderTex = ModContent.GetTexture(AssetDirectory.GUI + "GoldBorder");
                spriteBatch.Draw(borderTex, origin, Color.White);

                if (Main.rand.Next(3) == 0)
                {
                    if(Main.rand.Next(4) > 0)
                        sparkles.AddParticle(new Particle(origin + new Vector2(Main.rand.Next(borderTex.Width), Main.rand.NextBool() ? 2 : borderTex.Height - 2), Vector2.Zero, 0, 0, new Color(255, 230, 0), 60, new Vector2(Main.rand.NextFloat(0.4f, 0.7f), 0), new Rectangle(0, 0, 15, 15)));
                    else
                        sparkles.AddParticle(new Particle(origin + new Vector2(Main.rand.NextBool() ? 2 : borderTex.Width - 2, Main.rand.Next(borderTex.Height)), Vector2.Zero, 0, 0, new Color(255, 230, 0), 60, new Vector2(Main.rand.NextFloat(0.4f, 0.7f), 0), new Rectangle(0, 0, 15, 15)));
                }

                sparkles.DrawParticles(spriteBatch);
            }

            player.ResetEffects();
        }
    }
}
