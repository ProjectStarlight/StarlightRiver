using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;

namespace StarlightRiver.Core.Loaders
{
	class UILoader : IOrderedLoadable
    {
        public static List<UserInterface> UserInterfaces;
        public static List<SmartUIState> UIStates;

        public float Priority { get => 1.1f; }

        public void Load()
        {
            if (Main.dedServ)
                return;

            Mod Mod = StarlightRiver.Instance;

            UserInterfaces = new List<UserInterface>();
            UIStates = new List<SmartUIState>();

            foreach (Type t in Mod.Code.GetTypes())
            {
                if (t.IsSubclassOf(typeof(SmartUIState)))
                {
                    var state = (SmartUIState)Activator.CreateInstance(t, null);
                    var userInterface = new UserInterface();
                    userInterface.SetState(state);

                    UIStates?.Add(state);
                    UserInterfaces?.Add(userInterface);
                }
            }
        }

        public void Unload()
        {
            UserInterfaces = null;
            UIStates = null;
        }

        public static void AddLayer(List<GameInterfaceLayer> layers, UserInterface userInterface, UIState state, int index, bool visible, InterfaceScaleType scale)
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
                }, scale));
        }

        public static T GetUIState<T>() where T : SmartUIState => UIStates.FirstOrDefault(n => n is T) as T;

        public static void ReloadState<T>() where T : SmartUIState
        {
            var index = UIStates.IndexOf(GetUIState<T>());
            UIStates[index] = (T)Activator.CreateInstance(typeof(T), null);
            UserInterfaces[index] = new UserInterface();
            UserInterfaces[index].SetState(UIStates[index]);
        }
    }

    class AutoUISystem : ModSystem
	{
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            for (int k = 0; k < UILoader.UIStates.Count; k++)
            {
                var state = UILoader.UIStates[k];
                UILoader.AddLayer(layers, UILoader.UserInterfaces[k], state, state.InsertionIndex(layers), state.Visible, state.Scale);
            }
        }
    }
}
