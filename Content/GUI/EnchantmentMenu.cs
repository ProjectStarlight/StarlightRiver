using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.UI;
using Terraria;
using static Terraria.ModLoader.ModContent;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using Terraria.GameContent.UI.Elements;
using StarlightRiver.Content.Abilities;
using Terraria.ModLoader;
using ReLogic.Graphics;
using StarlightRiver.Abilities.AbilityContent.Infusions;
using StarlightRiver.Content.Abilities.ForbiddenWinds;
using Terraria.ID;
using StarlightRiver.Content.NPCs.Town;
using Terraria.GameContent.UI;
using StarlightRiver.Core.Loaders;

namespace StarlightRiver.Content.GUI
{
    class EnchantmentMenu : SmartUIState
    {
        public override int InsertionIndex(List<GameInterfaceLayer> layers) => layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));

        public override bool Visible => visible;

        public override InterfaceScaleType Scale => InterfaceScaleType.Game;

        public static EnchantNPC activeEnchanter;

        private static Vector2 centerPoint;
        public static Vector2 CenterPoint => centerPoint - Main.screenPosition;

        public static bool active;
        public static bool visible;
        private static List<ArmorSlot> slots = new List<ArmorSlot>();

        private static List<EnchantButton> buttons = new List<EnchantButton>();

        public override void OnInitialize()
        {
            slots.Clear();
            buttons.Clear();
            active = false;
            visible = false;

            for (int k = 0; k < 3; k++)
            {
                var newSlot = new ArmorSlot(k);
                slots.Add(newSlot);
                Append(newSlot);
            }
        }

        public static void SetActive(Vector2 worldPoint, EnchantNPC enchanter)
        {
            activeEnchanter = enchanter;

            centerPoint = worldPoint;
            visible = true;
            active = true;

            for (int k = 0; k < 3; k++)
            {
                slots[k].animationTimer = 0;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            spriteBatch.End();
            spriteBatch.Begin(default, BlendState.Additive, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            foreach(UIElement element in Elements.Where(n => n is EnchantButton))
            {
                (element as EnchantButton).DrawAdditive(spriteBatch);
            }

            spriteBatch.End();
            spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

            if (Main.LocalPlayer.controlHook) //Temporary closing logic
            {
                active = false;
                Main.LocalPlayer.GetModPlayer<StarlightPlayer>().ScreenMoveHold = false;
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (buttons.Count == 0 && CheckFull())
                CheckForSets();

            if (!CheckFull())
            {
                foreach (UIElement element in buttons)
                {
                    RemoveChild(element);
                }

                buttons.Clear();
            }

            base.Update(gameTime);
        }

        public static bool CheckFull()
        {
            for (int k = 0; k < 3; k++)
            {
                if (slots[k].item.IsAir || slots[k].item.GetGlobalItem<ArmorEnchantment.EnchantedArmorGlobalItem>().Enchantment != null)
                    return false;
            }

            return true;
        }

        public static void CheckForSets() //false implies some slots are empty/invalid, will return true even if no valid enchants exist but a full set is in the UI
        {
            for (int k = 0; k < ArmorEnchantLoader.Enchantments.Count; k++)
            {
                var enchant = ArmorEnchantLoader.Enchantments[k];
                if (enchant.IsAvailable(slots[0].item, slots[1].item, slots[2].item))
                {
                    var button = new EnchantButton(enchant);
                    buttons.Add(button);
                    UILoader.GetUIState<EnchantmentMenu>().Append(button);
                }
            }

            if(buttons.Count > 0)
            {
                for(int k = 0; k < buttons.Count; k++)
                {
                    float rot = (float)(Math.PI * 2f * (k / (float)buttons.Count));
                    buttons[k].SetCenter(CenterPoint + Vector2.UnitY.RotatedBy(rot) * -300);
                }
            }
        }

        public static bool TryEnchantSet(ArmorEnchantment.ArmorEnchantment enchant) //Minigame goes here eventually
        {
            for (int k = 0; k < 3; k++)
            {
                if (slots[k].item.IsAir) return false;
            }

            ArmorEnchantment.ArmorEnchantment.EnchantArmor(slots[0].item, slots[1].item, slots[2].item, enchant);
            return true;
        }
    }

    class ArmorSlot : UIElement
    {
        public int animationTimer;
        public Item item = new Item();
        private readonly int slotIndex;

        private ParticleSystem slotParticles = new ParticleSystem(AssetDirectory.GUI + "WhiteCircle", ParticleUpdate);
        private ParticleSystem leafParticles = new ParticleSystem(AssetDirectory.GUI + "OGLeaf", LeafUpdate);

        public ArmorSlot(int index)
        {
            slotIndex = index;

            SetCenter(EnchantmentMenu.CenterPoint - Vector2.UnitY * 200);
            Width.Set(108, 0);
            Height.Set(108, 0);
        }

        public override void Update(GameTime gameTime)
        {
            Main.LocalPlayer.mouseInterface = true;

            if (EnchantmentMenu.active && animationTimer < 180)
                animationTimer++;
            if (!EnchantmentMenu.active)
                animationTimer--;

            if (animationTimer <= 0)
            {
                EnchantmentMenu.visible = false;
                EnchantmentMenu.activeEnchanter.enchanting = false;
            }

            float rad = 200;

            if (animationTimer <= 90)
            {
                float rot = (animationTimer / 90f) * ((float)Math.PI / 3 + (float)Math.PI / 1.5f * slotIndex) * Helpers.Helper.BezierEase(animationTimer / 90f);
                SetCenter(EnchantmentMenu.CenterPoint - Vector2.UnitY.RotatedBy(rot) * rad);
            }
            else
            {
                float rot = ((float)Math.PI / 3 + (float)Math.PI / 1.5f * slotIndex);
                SetCenter(EnchantmentMenu.CenterPoint - Vector2.UnitY.RotatedBy(rot) * rad);
            }

            Recalculate();
            base.Update(gameTime);
        }

        public static void ParticleUpdate(Particle particle)
        {
            particle.Position += particle.Velocity;
            particle.Scale *= 0.95f;

            if (particle.StoredPosition == Vector2.One)
                particle.Velocity *= 0.9f;

            if (particle.Scale <= 0.1f)
                particle.Timer = 0;
        }

        public static void LeafUpdate(Particle particle)
        {
            particle.Position.X += (float)Math.Sin((particle.Timer + particle.Velocity.X) * 0.05f) * 0.6f;
            particle.Position.Y += particle.Velocity.Y;
            particle.Rotation = (particle.Position.X - particle.StoredPosition.X) * -0.02f;
            particle.Color *= 0.99f;

            particle.Timer--;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var closedTex = GetTexture(AssetDirectory.GUI + "EnchantSlotClosed");
            var openTex = GetTexture(AssetDirectory.GUI + "EnchantSlotOpen");
            var iconTex = GetTexture(AssetDirectory.GUI + "EnchantSlotIcon");

            leafParticles.DrawParticles(spriteBatch);

            if (animationTimer <= 90) //animation for opening
            {
                spriteBatch.Draw(closedTex, GetDimensions().Center(), new Rectangle(0, 0, 34, 34), Color.White, 0, Vector2.One * 17, 1, 0, 0);
            }
            else if (animationTimer <= 110)
            {
                spriteBatch.Draw(closedTex, GetDimensions().Center(), new Rectangle(0, 0, 34, 34), Color.White, 0, Vector2.One * 17, 1, 0, 0);
                spriteBatch.Draw(closedTex, GetDimensions().Center(), new Rectangle(0, 34, 34, 34), Color.Lerp(Color.Transparent, Color.LightYellow, (animationTimer - 90) / 20f), 0, Vector2.One * 17, 1, 0, 0);
            }
            else if (animationTimer <= 140)
            {
                spriteBatch.Draw(closedTex, GetDimensions().Center(), new Rectangle(0, 34, 34, 34), Color.LightYellow, 0, Vector2.One * 17, 1, 0, 0);
                spriteBatch.Draw(openTex, GetDimensions().Center(), new Rectangle(0, 108, 98, 108), Color.LightYellow, 0, Vector2.One * 49, (animationTimer - 110) / 30f, 0, 0);
            }
            else if (animationTimer <= 160)
            {
                spriteBatch.Draw(openTex, GetDimensions().Center(), new Rectangle(0, 0, 98, 106), Color.White, 0, Vector2.One * 49, 1, 0, 0);
                spriteBatch.Draw(iconTex, GetDimensions().Center() + Vector2.UnitY * 6, new Rectangle(0, 18 * slotIndex, 20, 18), Color.White, 0, Vector2.One * 9, 1, 0, 0);
                spriteBatch.Draw(openTex, GetDimensions().Center(), new Rectangle(0, 106, 98, 106), Color.Lerp(Color.LightYellow, Color.Transparent, (animationTimer - 140) / 20f), 0, Vector2.One * 49, 1, 0, 0);
            }
            else //drawing while open
            {
                spriteBatch.Draw(openTex, GetDimensions().Center(), new Rectangle(0, 0, 98, 106), Color.White, 0, Vector2.One * 49, 1, 0, 0);
                spriteBatch.Draw(iconTex, GetDimensions().Center() + Vector2.UnitY * 6, new Rectangle(0, 18 * slotIndex, 20, 18), Color.White, 0, Vector2.One * 9, 1, 0, 0);

                if (!item.IsAir)
                {
                    Texture2D itemTexture = item.type > ItemID.Count ? GetTexture(item.modItem.Texture) : GetTexture("Terraria/Item_" + item.type);
                    float scale = itemTexture.Frame().Size().Length() < 52 ? 1 : 52f / itemTexture.Frame().Size().Length();

                    spriteBatch.Draw(itemTexture, GetDimensions().Center(), itemTexture.Frame(), Color.White, 0, itemTexture.Frame().Size() / 2, scale, 0, 0);

                    var pos = GetDimensions().Center() + Vector2.UnitY * 45 + Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(5);
                    slotParticles.AddParticle(new Particle(pos, Vector2.UnitY * -Main.rand.NextFloat(0.4f, 0.6f), 0, Main.rand.NextFloat(0.6f, 1.2f), ItemRarity.GetColor(item.rare) * 0.8f, 1, Vector2.Zero));
                }

                if (Main.rand.Next(20) == 0)
                {
                    var pos = GetDimensions().Center() + Vector2.UnitY.RotatedByRandom(1f) * (35 + Main.rand.NextFloat(5));
                    leafParticles.AddParticle(new Particle(pos, new Vector2(Main.rand.NextFloat(500), Main.rand.NextFloat(0.4f, 0.8f)), 0, Main.rand.NextFloat(0.5f, 0.7f), Color.White, 120, pos));
                }
            }

            slotParticles.DrawParticles(spriteBatch);
        }

        public override void Click(UIMouseEvent evt)
        {
            if (item.IsAir && !Main.mouseItem.IsAir && CheckValid(Main.mouseItem))
            {
                item = Main.mouseItem.Clone();
                Main.mouseItem.TurnToAir();

                Main.PlaySound(SoundID.DD2_DarkMageHealImpact);

                for (int k = 0; k < 50; k++)
                    slotParticles.AddParticle(new Particle(GetDimensions().Center() + Vector2.UnitY * 45, Vector2.One.RotatedByRandom(6.28f) * Main.rand.NextFloat(8), 0, Main.rand.NextFloat(0.4f, 0.6f), ItemRarity.GetColor(item.rare), 1, Vector2.One));
            }

            else if (!item.IsAir && Main.mouseItem.IsAir)
            {
                Main.mouseItem = item.Clone();
                item.TurnToAir();

                Main.PlaySound(SoundID.MenuTick);
            }
        }

        public bool CheckValid(Item item)
        {
            if (item.vanity)
                return false;

            switch (slotIndex)
            {
                case 0: if (item.headSlot != -1) return true; break;
                case 1: if (item.bodySlot != -1) return true; break;
                case 2: if (item.legSlot != -1) return true; break;
            }

            return false;
        }

        public void SetCenter(Vector2 pos)
        {
            Left.Set(pos.X - Width.Pixels / 2, 0);
            Top.Set(pos.Y - Height.Pixels / 2, 0);
        }
    }

    class EnchantButton : UIElement
    {
        private readonly ArmorEnchantment.ArmorEnchantment enchant;
        private int animationTimer = 0;

        public EnchantButton(ArmorEnchantment.ArmorEnchantment enchant)
        {
            this.enchant = enchant;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var tex = GetTexture(enchant.Texture);
            var pos = GetDimensions().Center();

            spriteBatch.Draw(tex, pos, null, Color.White, 0, tex.Size() / 2, 1, 0, 0);
        }

        public void DrawAdditive(SpriteBatch spriteBatch) //batched and drawn in parent
        {
            var tex = GetTexture("StarlightRiver/Assets/Keys/Glow");
            var pos = GetDimensions().Center();
            var scale = 1 + animationTimer / 30f;
            var opacity = 1 - animationTimer / 30f;

            spriteBatch.Draw(tex, pos, null, enchant.Color * opacity, 0, tex.Size() / 2, scale, 0, 0);

            animationTimer++;
        }

        public void SetCenter(Vector2 pos)
        {
            Width.Set(48, 0);
            Height.Set(48, 0);

            Left.Set(pos.X - Width.Pixels / 2, 0);
            Top.Set(pos.Y - Height.Pixels / 2, 0);

            Recalculate();
        }

        public override void Click(UIMouseEvent evt)
        {
            if (EnchantmentMenu.TryEnchantSet(enchant.MakeRealCopy()))
                EnchantmentMenu.CheckForSets();
        }
    }
}
