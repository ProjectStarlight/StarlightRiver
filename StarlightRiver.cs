using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.GUI;
using StarlightRiver.RiftCrafting;
using StarlightRiver.Tiles.Permafrost;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.UI;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver
{
    public partial class StarlightRiver : Mod
    {
        public Stamina stamina;
        public Collection collection;
        public Overlay overlay;
        public Infusion infusion;
        public Cooking cooking;
        public KeyInventory keyinventory;
        public TextCard textcard;
        public GUI.Codex codex;
        public CodexPopup codexpopup;
        public LootUI lootUI;
        public ChatboxOverUI Chatbox;
        public UIState ExtraNPCState;

        public UserInterface StaminaUserInterface;
        public UserInterface CollectionUserInterface;
        public UserInterface OverlayUserInterface;
        public UserInterface InfusionUserInterface;
        public UserInterface CookingUserInterface;
        public UserInterface KeyInventoryUserInterface;
        public UserInterface TextCardUserInterface;
        public UserInterface CodexUserInterface;
        public UserInterface CodexPopupUserInterface;
        public UserInterface LootUserInterface;
        public UserInterface ChatboxUserInterface;
        public UserInterface ExtraNPCInterface;

        public static ModHotKey Dash;
        public static ModHotKey Wisp;
        public static ModHotKey Purify;
        public static ModHotKey Smash;
        public static ModHotKey Superdash;

        public List<RiftRecipe> RiftRecipes;

        public static float Rotation;

        public enum AbilityEnum : int { dash, wisp, purify, smash, superdash };
        public static StarlightRiver Instance { get; set; }

        public static RenderTest lightingTest = new RenderTest();

        public StarlightRiver() { Instance = this; }

        public override void UpdateMusic(ref int music, ref MusicPriority priority)
        {
            if (Main.myPlayer != -1 && !Main.gameMenu && Main.LocalPlayer.active)
            {
                Player player = Main.LocalPlayer;

                if (player.GetModPlayer<BiomeHandler>().ZoneGlass)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/GlassPassive");
                    priority = MusicPriority.BiomeHigh;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneOvergrow)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/Overgrow");
                    priority = MusicPriority.BiomeHigh;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneVoidPre)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/VoidPre");
                    priority = MusicPriority.BossLow;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneJungleCorrupt)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleCorrupt");
                    priority = MusicPriority.BiomeMedium;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneJungleBloody)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleBloody");
                    priority = MusicPriority.BiomeMedium;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneJungleHoly)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/JungleHoly");
                    priority = MusicPriority.BiomeMedium;
                }

                if (player.GetModPlayer<BiomeHandler>().zoneAluminum)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/AluminumPassive");
                    priority = MusicPriority.BiomeHigh;
                }

                if (player.GetModPlayer<BiomeHandler>().zonePermafrost)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/PermafrostPassive");
                    priority = MusicPriority.BiomeMedium;
                }

                if (Main.tile[(int)player.Center.X / 16, (int)player.Center.Y / 16].wall == ModContent.WallType<AuroraBrickWall>() && !StarlightWorld.SquidBossDowned)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/SquidArena");
                    priority = MusicPriority.BiomeHigh;
                }
            }
            return;
        }

        public static void AutoloadRiftRecipes(List<RiftRecipe> target)
        {
            if (Instance.Code != null)
            {
                foreach (Type type in Instance.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(RiftRecipe))))
                {
                    target.Add((RiftRecipe)Activator.CreateInstance(type));
                }
            }
        }

        public override void Load()
        {
            //Shaders
            if (!Main.dedServ)
            {
                GameShaders.Misc["StarlightRiver:Distort"] = new MiscShaderData(new Ref<Effect>(GetEffect("Effects/Distort")), "Distort");

                Ref<Effect> screenRef4 = new Ref<Effect>(GetEffect("Effects/Shockwave"));
                Terraria.Graphics.Effects.Filters.Scene["ShockwaveFilter"] = new Terraria.Graphics.Effects.Filter(new ScreenShaderData(screenRef4, "ShockwavePass"), Terraria.Graphics.Effects.EffectPriority.VeryHigh);
                Terraria.Graphics.Effects.Filters.Scene["ShockwaveFilter"].Load();

                Ref<Effect> screenRef3 = new Ref<Effect>(GetEffect("Effects/WaterEffect"));
                Terraria.Graphics.Effects.Filters.Scene["WaterFilter"] = new Terraria.Graphics.Effects.Filter(new ScreenShaderData(screenRef3, "WaterPass"), Terraria.Graphics.Effects.EffectPriority.VeryHigh);
                Terraria.Graphics.Effects.Filters.Scene["WaterFilter"].Load();

                Ref<Effect> screenRef2 = new Ref<Effect>(GetEffect("Effects/AuraEffect"));
                Terraria.Graphics.Effects.Filters.Scene["AuraFilter"] = new Terraria.Graphics.Effects.Filter(new ScreenShaderData(screenRef2, "AuraPass"), Terraria.Graphics.Effects.EffectPriority.VeryHigh);
                Terraria.Graphics.Effects.Filters.Scene["AuraFilter"].Load();

                Ref<Effect> screenRef = new Ref<Effect>(GetEffect("Effects/BlurEffect"));
                Terraria.Graphics.Effects.Filters.Scene["BlurFilter"] = new Terraria.Graphics.Effects.Filter(new ScreenShaderData(screenRef, "BlurPass"), Terraria.Graphics.Effects.EffectPriority.High);
                Terraria.Graphics.Effects.Filters.Scene["BlurFilter"].Load();

                Ref<Effect> screenRef5 = new Ref<Effect>(GetEffect("Effects/Purity"));
                Terraria.Graphics.Effects.Filters.Scene["PurityFilter"] = new Terraria.Graphics.Effects.Filter(new ScreenShaderData(screenRef5, "PurityPass"), Terraria.Graphics.Effects.EffectPriority.High);
                Terraria.Graphics.Effects.Filters.Scene["PurityFilter"].Load();

                Ref<Effect> screenRef6 = new Ref<Effect>(GetEffect("Effects/LightShader"));
                Terraria.Graphics.Effects.Filters.Scene["Lighting"] = new Terraria.Graphics.Effects.Filter(new ScreenShaderData(screenRef6, "LightingPass"), Terraria.Graphics.Effects.EffectPriority.High);
                Terraria.Graphics.Effects.Filters.Scene["Lighting"].Load();

                Ref<Effect> screenRef7 = new Ref<Effect>(GetEffect("Effects/LightApplicator"));
                Terraria.Graphics.Effects.Filters.Scene["LightingApply"] = new Terraria.Graphics.Effects.Filter(new ScreenShaderData(screenRef7, "LightingApplyPass"), Terraria.Graphics.Effects.EffectPriority.High);
                Terraria.Graphics.Effects.Filters.Scene["LightingApply"].Load();
            }

            //Autoload Rift Recipes
            RiftRecipes = new List<RiftRecipe>();
            AutoloadRiftRecipes(RiftRecipes);

            //Hotkeys
            Dash = RegisterHotKey("Forbidden Winds", "LeftShift");
            Wisp = RegisterHotKey("Faeflame", "F");
            Purify = RegisterHotKey("[PH]Purify Crown", "N");
            Smash = RegisterHotKey("Gaia's Fist", "Z");
            Superdash = RegisterHotKey("Zzelera's Cloak", "Q");

            //UI
            if (!Main.dedServ)
            {
                StaminaUserInterface = new UserInterface();
                CollectionUserInterface = new UserInterface();
                OverlayUserInterface = new UserInterface();
                InfusionUserInterface = new UserInterface();
                CookingUserInterface = new UserInterface();
                KeyInventoryUserInterface = new UserInterface();
                TextCardUserInterface = new UserInterface();
                CodexUserInterface = new UserInterface();
                CodexPopupUserInterface = new UserInterface();
                LootUserInterface = new UserInterface();
                ChatboxUserInterface = new UserInterface();
                ExtraNPCInterface = new UserInterface();

                stamina = new Stamina();
                collection = new Collection();
                overlay = new Overlay();
                infusion = new Infusion();
                cooking = new Cooking();
                keyinventory = new KeyInventory();
                textcard = new TextCard();
                codex = new GUI.Codex();
                codexpopup = new CodexPopup();
                lootUI = new LootUI();
                Chatbox = new ChatboxOverUI();

                StaminaUserInterface.SetState(stamina);
                CollectionUserInterface.SetState(collection);
                OverlayUserInterface.SetState(overlay);
                InfusionUserInterface.SetState(infusion);
                CookingUserInterface.SetState(cooking);
                KeyInventoryUserInterface.SetState(keyinventory);
                TextCardUserInterface.SetState(textcard);
                CodexUserInterface.SetState(codex);
                CodexPopupUserInterface.SetState(codexpopup);
                LootUserInterface.SetState(lootUI);
                ChatboxUserInterface.SetState(Chatbox);
            }

            //particle systems
            if (!Main.dedServ)
            {
                LoadVitricBGSystems();
            }

            //Hooking
            HookOn();
            HookIL();
        }

        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            if (Rotation != 0)
            {
                var type = typeof(SpriteViewMatrix);
                var field = type.GetField("_transformationMatrix", BindingFlags.NonPublic | BindingFlags.Instance);

                Matrix rotation = Matrix.CreateRotationZ(Rotation);
                Matrix translation = Matrix.CreateTranslation(new Vector3(Main.screenWidth / 2, Main.screenHeight / 2, 0));
                Matrix translation2 = Matrix.CreateTranslation(new Vector3(Main.screenWidth / -2, Main.screenHeight / -2, 0));

                field.SetValue(Transform, (translation2 * rotation) * translation);
                base.ModifyTransformMatrix(ref Transform);
                Helper.UpdateTilt();
            }
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int MouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            int NPCChatIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: NPC / Sign Dialog"));
            if (MouseTextIndex != -1)
            {
                AddLayer(layers, StaminaUserInterface, stamina, MouseTextIndex, Stamina.visible);
                AddLayer(layers, CollectionUserInterface, collection, MouseTextIndex, Collection.visible);
                AddLayer(layers, OverlayUserInterface, overlay, 0, Overlay.visible);
                AddLayer(layers, InfusionUserInterface, infusion, MouseTextIndex, Infusion.visible);
                AddLayer(layers, CookingUserInterface, cooking, MouseTextIndex, Cooking.Visible);
                AddLayer(layers, KeyInventoryUserInterface, keyinventory, MouseTextIndex, KeyInventory.visible);
                AddLayer(layers, TextCardUserInterface, textcard, MouseTextIndex, TextCard.Visible);
                AddLayer(layers, CodexUserInterface, codex, MouseTextIndex, GUI.Codex.ButtonVisible);
                AddLayer(layers, CodexPopupUserInterface, codexpopup, MouseTextIndex, codexpopup.Timer > 0);
                AddLayer(layers, LootUserInterface, lootUI, MouseTextIndex, LootUI.Visible);
                AddLayer(layers, ChatboxUserInterface, Chatbox, NPCChatIndex, Main.player[Main.myPlayer].talkNPC > 0 && Main.npcShop <= 0 && !Main.InGuideCraftMenu);
                AddLayer(layers, ExtraNPCInterface, ExtraNPCState, MouseTextIndex, ExtraNPCState != null);
            }
        }

        private void AddLayer(List<GameInterfaceLayer> layers, UserInterface userInterface, UIState state, int index, bool visible)
        {
            string name = state == null ? "Unknown" : state.ToString();
            layers.Insert(index, new LegacyGameInterfaceLayer("StarlightRiver: " + name,
                delegate
                {
                    if (visible)
                    {
                        userInterface.Update(Main._drawInterfaceGameTime);
                        state.Draw(Main.spriteBatch);
                    }
                    return true;
                }, InterfaceScaleType.UI));
        }

        public override void Unload()
        {
            if (!Main.dedServ)
            {
                RiftRecipes = null;

                StaminaUserInterface = null;
                CollectionUserInterface = null;
                OverlayUserInterface = null;
                InfusionUserInterface = null;
                CookingUserInterface = null;
                TextCardUserInterface = null;
                CodexUserInterface = null;
                CodexPopupUserInterface = null;
                LootUserInterface = null;
                ChatboxUserInterface = null;
                ExtraNPCInterface = null;

                stamina = null;
                collection = null;
                overlay = null;
                infusion = null;
                cooking = null;
                textcard = null;
                codex = null;
                codexpopup = null;
                lootUI = null;
                Chatbox = null;
                ExtraNPCState = null;

                Instance = null;
                Dash = null;
                Superdash = null;
                Wisp = null;
                Smash = null;
                Purify = null;
            }

            UnhookIL();
            Main.OnPreDraw -= TestLighting;
        }

        #region NetEasy
        public override void PostSetupContent()
        {
            NetEasy.NetEasy.Register(this);
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NetEasy.NetEasy.HandleModule(reader, whoAmI);
        }
        #endregion
    }
}
