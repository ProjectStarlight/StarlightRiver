using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StarlightRiver.Abilities;
using StarlightRiver.Content.Foregrounds;
using StarlightRiver.Content.Tiles.Permafrost;
using StarlightRiver.Core;
using StarlightRiver.Core.Loaders;
using StarlightRiver.Helpers;
using StarlightRiver.RiftCrafting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Terraria;
using Terraria.Graphics;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;
using Terraria.UI;

namespace StarlightRiver
{
    public partial class StarlightRiver : Mod
    {
        public AbilityHotkeys AbilityKeys { get; private set; }

        public List<RiftRecipe> RiftRecipes;

        public List<Foreground> foregrounds;

        private List<ILoadable> loadCache;

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

        public override void Load()
        {
            foreach (Type type in Code.GetTypes())
            {
                if (!type.IsAbstract && type.GetInterfaces().Contains(typeof(ILoadable)))
                {
                    var instance = Activator.CreateInstance(type);
                    loadCache.Add(instance as ILoadable);
                    (instance as ILoadable).Load();
                }
            }

            //Shaders
            if (!Main.dedServ)
            {
                /*
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
                */

                lightingTest = new RenderTest();
            }

            foregrounds = new List<Foreground>();

            //Autoload Rift Recipes
            RiftRecipes = new List<RiftRecipe>();
            AutoloadRiftRecipes(RiftRecipes);

            //UI
            if (!Main.dedServ)
            {
                //Hotkeys
                AbilityKeys = new AbilityHotkeys(this);
                AbilityKeys.LoadDefaults();
            }
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
            for (int k = 0; k < UILoader.UIStates.Count; k++)
            {
                var state = UILoader.UIStates[k];
                UILoader.AddLayer(layers, UILoader.UserInterfaces[k], state, state.InsertionIndex(layers), state.Visible);
            }
        }

        public override void Unload()
        {
            foreach (var loadable in loadCache)
            {
                loadable.Unload();
            }
            loadCache = null;

            if (!Main.dedServ)
            {
                RiftRecipes = null;

                Instance = null;
                AbilityKeys.Unload();
            }
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
