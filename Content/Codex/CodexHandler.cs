﻿using StarlightRiver.Core.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Codex
{
	internal class CodexHandler : ModPlayer
    {
        public int CodexState = 0; //0 = none, 1 = normal, 2 = void
        public List<CodexEntry> Entries = new List<CodexEntry>();

        public override void SaveData(TagCompound tag)
        {
            tag[nameof(CodexState)] = CodexState;
            tag[nameof(Entries)] = Entries;
        }

        public override void LoadData(TagCompound tag)
        {
            CodexState = tag.GetInt(nameof(CodexState));

            Entries = new List<CodexEntry>();
            var entriesToLoad = tag.GetList<TagCompound>(nameof(Entries));

            foreach (Type type in Mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(CodexEntry))))
            {
                CodexEntry ThisEntry = (CodexEntry)Activator.CreateInstance(type);
                Entries.Add(ThisEntry);
            }

            if (entriesToLoad == null || entriesToLoad.Count == 0) return;

            foreach (TagCompound tagc in entriesToLoad)
            {
                CodexEntry entry = CodexEntry.DeserializeData(tagc);
                if (entry != null && Entries.FirstOrDefault(n => n.GetType() == entry.GetType()) != null) //find and replace needed entries with save data
                {
                    int index = Entries.IndexOf(Entries.FirstOrDefault(n => n.GetType() == entry.GetType()));
                    Entries[index] = entry;
                }
            }
        }

        public override void OnEnterWorld(Player Player)
        {
            if (Entries.Count == 0) //failsafe incase the Player dosent load for some reason
            {
                foreach (Type type in Mod.Code.GetTypes().Where(t => t.IsSubclassOf(typeof(CodexEntry))))
                {
                    CodexEntry ThisEntry = (CodexEntry)Activator.CreateInstance(type);
                    Entries.Add(ThisEntry);
                }
            }
            UILoader.ReloadState<Content.GUI.Codex>();
        }
    }
}