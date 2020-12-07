using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using StarlightRiver.GUI;
using StarlightRiver.RiftCrafting;
using StarlightRiver.Tiles;
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
using Terraria.Graphics.Effects;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver
{
    public partial class StarlightRiver : Mod
    {
        public Stamina stamina;
        public Collection collection;
        public ParticleOverlay overlay;
        public Infusion infusion;
        public CookingUI cooking;
        public KeyInventory keyinventory;
        public TextCard textcard;
        public GUI.Codex codex;
        public CodexPopup codexpopup;
        public LootUI lootUI;
        public ChatboxOverUI Chatbox;
        public UIState ExtraNPCState;
        public RichTextBox RichText;

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
        public UserInterface RichTextInterface;

        public AbilityHotkeys AbilityKeys { get; private set; }

        public List<RiftRecipe> RiftRecipes;

        public static float Rotation;

        public static RenderTest lightingTest = null;

        public static StarlightRiver Instance { get; set; }

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

                if (player.GetModPlayer<BiomeHandler>().ZoneGlassTemple)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/GlassTemple");
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

                if (Main.tile[(int)player.Center.X / 16, (int)player.Center.Y / 16].wall == ModContent.WallType<AuroraBrickWall>() && !StarlightWorld.HasFlag(WorldFlags.SquidBossDowned))
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/SquidArena");
                    priority = MusicPriority.BiomeHigh;
                }

                if (player.GetModPlayer<BiomeHandler>().ZoneOvergrow)
                {
                    music = GetSoundSlot(SoundType.Music, "Sounds/Music/Overgrow");
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

        public static void AutoloadFurniture()
        {
            if (Instance.Code != null)
            {
                foreach (Type type in Instance.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(AutoFurniture))))
                {
                    (Activator.CreateInstance(type) as AutoFurniture).Load(Instance);
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
                Filters.Scene["ShockwaveFilter"] = new Filter(new ScreenShaderData(screenRef4, "ShockwavePass"), EffectPriority.VeryHigh);
                Filters.Scene["ShockwaveFilter"].Load();

                Ref<Effect> screenRef3 = new Ref<Effect>(GetEffect("Effects/WaterEffect"));
                Filters.Scene["WaterFilter"] = new Filter(new ScreenShaderData(screenRef3, "WaterPass"), EffectPriority.VeryHigh);
                Filters.Scene["WaterFilter"].Load();

                Ref<Effect> screenRef2 = new Ref<Effect>(GetEffect("Effects/AuraEffect"));
                Filters.Scene["AuraFilter"] = new Filter(new ScreenShaderData(screenRef2, "AuraPass"), EffectPriority.VeryHigh);
                Filters.Scene["AuraFilter"].Load();

                Ref<Effect> screenRef = new Ref<Effect>(GetEffect("Effects/BlurEffect"));
                Filters.Scene["BlurFilter"] = new Filter(new ScreenShaderData(screenRef, "BlurPass"), EffectPriority.High);
                Filters.Scene["BlurFilter"].Load();

                Ref<Effect> screenRef5 = new Ref<Effect>(GetEffect("Effects/Purity"));
                Filters.Scene["PurityFilter"] = new Filter(new ScreenShaderData(screenRef5, "PurityPass"), EffectPriority.High);
                Filters.Scene["PurityFilter"].Load();

                Ref<Effect> screenRef6 = new Ref<Effect>(GetEffect("Effects/LightShader"));
                Filters.Scene["Lighting"] = new Filter(new ScreenShaderData(screenRef6, "LightingPass"), EffectPriority.High);
                Filters.Scene["Lighting"].Load();

                Ref<Effect> screenRef7 = new Ref<Effect>(GetEffect("Effects/LightApply"));
                Filters.Scene["LightingApply"] = new Filter(new ScreenShaderData(screenRef7, "LightingApplyPass"), EffectPriority.High);
                Filters.Scene["LightingApply"].Load();

                Ref<Effect> screenRef8 = new Ref<Effect>(GetEffect("Effects/pixelationFull"));
                Filters.Scene["Pixelation"] = new Filter(new ScreenShaderData(screenRef8, "PixelationPass"), EffectPriority.Medium);
                Filters.Scene["Pixelation"].Load();

                Ref<Effect> screenRefCrystal = new Ref<Effect>(GetEffect("Effects/CrystalRefraction"));
                Filters.Scene["Crystal"] = new Filter(new ScreenShaderData(screenRefCrystal, "CrystalPass"), EffectPriority.High);
                Filters.Scene["Crystal"].Load();

                Ref<Effect> screenRefIceCrystal = new Ref<Effect>(GetEffect("Effects/IceCrystal"));
                Filters.Scene["IceCrystal"] = new Filter(new ScreenShaderData(screenRefIceCrystal, "IcePass"), EffectPriority.High);
                Filters.Scene["IceCrystal"].Load();

                Ref<Effect> screenRefWaves = new Ref<Effect>(GetEffect("Effects/Waves"));
                Filters.Scene["Waves"] = new Filter(new ScreenShaderData(screenRefWaves, "WavesPass"), EffectPriority.High);
                Filters.Scene["Waves"].Load();

                Ref<Effect> screenRefWaterShine = new Ref<Effect>(GetEffect("Effects/WaterShine"));
                Filters.Scene["WaterShine"] = new Filter(new ScreenShaderData(screenRefWaterShine, "WaterShinePass"), EffectPriority.High);
                Filters.Scene["WaterShine"].Load();

                lightingTest = new RenderTest();
            }

            //Autoload Rift Recipes
            RiftRecipes = new List<RiftRecipe>();
            AutoloadRiftRecipes(RiftRecipes);

            //Furniture
            AutoloadFurniture();

            //UI
            if (!Main.dedServ)
            {
                //Hotkeys
                AbilityKeys = new AbilityHotkeys(this);
                AbilityKeys.LoadDefaults();

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
                RichTextInterface = new UserInterface();

                stamina = new Stamina();
                collection = new Collection();
                overlay = new ParticleOverlay();
                infusion = new Infusion();
                cooking = new CookingUI();
                keyinventory = new KeyInventory();
                textcard = new TextCard();
                codex = new GUI.Codex();
                codexpopup = new CodexPopup();
                lootUI = new LootUI();
                Chatbox = new ChatboxOverUI();
                RichText = new RichTextBox();

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
                RichTextInterface.SetState(RichText);
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

        private readonly FieldInfo _transformMatrix = typeof(SpriteViewMatrix).GetField("_transformationMatrix", BindingFlags.NonPublic | BindingFlags.Instance);
        public override void ModifyTransformMatrix(ref SpriteViewMatrix Transform)
        {
            if (Rotation != 0)
            {

                Matrix rotation = Matrix.CreateRotationZ(Rotation);
                Matrix translation = Matrix.CreateTranslation(new Vector3(Main.screenWidth / 2, Main.screenHeight / 2, 0));
                Matrix translation2 = Matrix.CreateTranslation(new Vector3(Main.screenWidth / -2, Main.screenHeight / -2, 0));

                _transformMatrix.SetValue(Transform, (translation2 * rotation) * translation);
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
                AddLayer(layers, OverlayUserInterface, overlay, 0, ParticleOverlay.visible);
                AddLayer(layers, InfusionUserInterface, infusion, MouseTextIndex, Infusion.visible);
                AddLayer(layers, CollectionUserInterface, collection, MouseTextIndex, Collection.visible);
                AddLayer(layers, CookingUserInterface, cooking, MouseTextIndex, CookingUI.Visible);
                AddLayer(layers, KeyInventoryUserInterface, keyinventory, MouseTextIndex, KeyInventory.visible);
                AddLayer(layers, TextCardUserInterface, textcard, MouseTextIndex, TextCard.Visible);
                AddLayer(layers, CodexUserInterface, codex, MouseTextIndex, GUI.Codex.ButtonVisible);
                AddLayer(layers, CodexPopupUserInterface, codexpopup, MouseTextIndex, codexpopup.Timer > 0);
                AddLayer(layers, LootUserInterface, lootUI, MouseTextIndex, LootUI.Visible);
                AddLayer(layers, ChatboxUserInterface, Chatbox, NPCChatIndex, Main.player[Main.myPlayer].talkNPC > 0 && Main.npcShop <= 0 && !Main.InGuideCraftMenu);
                AddLayer(layers, ExtraNPCInterface, ExtraNPCState, MouseTextIndex, ExtraNPCState != null);
                AddLayer(layers, RichTextInterface, RichText, MouseTextIndex, RichTextBox.visible);
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
                RichTextInterface = null;

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
                RichText = null;

                Instance = null;
                AbilityKeys.Unload();
            }

            UnhookIL();
            Main.OnPreDraw -= TestLighting;
        }

        #region NetEasy
        public override void PostSetupContent()
        {
            NetEasy.NetEasy.Register(this);
            InitWorldGenChests();
            CallBossChecklist();
        }

        public override void HandlePacket(BinaryReader reader, int whoAmI)
        {
            NetEasy.NetEasy.HandleModule(reader, whoAmI);
        }
        #endregion
    }
}
