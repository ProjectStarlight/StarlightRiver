using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Abilities
{
    public partial class AbilityHandler : ModPlayer
    {
        //All players store 1 instance of each ability. This instance is changed to the infusion variant if an infusion is equipped.
        public Dash dash = new Dash(Main.LocalPlayer);

        public Wisp wisp = new Wisp(Main.LocalPlayer);
        public Pure pure = new Pure(Main.LocalPlayer);
        public Smash smash = new Smash(Main.LocalPlayer);
        public Superdash sdash = new Superdash(Main.LocalPlayer);

        //A list of all ability instances is kept to easily check things globally across the player's abilities.
        public List<Ability> Abilities = new List<Ability>();

        //The players stamina stats.
        public int StatStaminaMax = 0;

        public int StatStaminaMaxTemp = 0;
        public int StatStaminaMaxPerm = 1;
        public int StatStamina = 1;
        public int StatStaminaRegenMax = 210;
        public int StatStaminaRegen = 0;

        //Holds the player's wing or rocket boot timer, since they must be disabled to move upwards correctly.
        private float StoredAccessoryTime = 0;

        public override TagCompound Save()
        {
            return new TagCompound
            {
                //ability unlock data
                [nameof(dash)] = dash.Locked,
                [nameof(wisp)] = wisp.Locked,
                [nameof(pure)] = pure.Locked,
                [nameof(smash)] = smash.Locked,
                [nameof(sdash)] = sdash.Locked,

                //permanent stamina amount
                [nameof(StatStaminaMaxPerm)] = StatStaminaMaxPerm,

                //infusion data
                [nameof(slot1)] = slot1,
                [nameof(slot2)] = slot2,

                [nameof(HasSecondSlot)] = HasSecondSlot
            };
        }

        public override void Load(TagCompound tag)
        {
            //dash
            dash = new Dash(player);
            dash.Locked = tag.GetBool(nameof(dash));
            Abilities.Add(dash);
            //wisp
            wisp = new Wisp(player);
            wisp.Locked = tag.GetBool(nameof(wisp));
            Abilities.Add(wisp);
            //pure
            pure = new Pure(player);
            pure.Locked = tag.GetBool(nameof(pure));
            //Abilities.Add(pure);
            //smash
            smash = new Smash(player);
            smash.Locked = tag.GetBool(nameof(smash));
            //Abilities.Add(smash);
            //shadow dash
            sdash = new Superdash(player);
            sdash.Locked = tag.GetBool(nameof(sdash));
            //Abilities.Add(sdash);

            //loads the player's maximum stamina.
            StatStaminaMaxPerm = tag.GetInt(nameof(StatStaminaMaxPerm));

            //loads infusion data.
            slot1 = tag.Get<Item>(nameof(slot1)); if (string.IsNullOrEmpty(slot1.Name)) { slot1 = null; }
            slot2 = tag.Get<Item>(nameof(slot2)); if (string.IsNullOrEmpty(slot2.Name)) { slot2 = null; }
            HasSecondSlot = tag.GetBool(nameof(HasSecondSlot));
        }

        //Updates the Ability list with the latest info
        public void SetList()
        {
            Abilities.Clear();
            Abilities.Add(dash);
            Abilities.Add(wisp);
            Abilities.Add(pure);
            Abilities.Add(smash);
            //Abilities.Add(sdash);
        }

        public override void ResetEffects()
        {
            //Resets the player's stamina to prevent issues with gaining infinite stamina or stamina regeneration.
            StatStaminaMax = StatStaminaMaxTemp + StatStaminaMaxPerm;
            StatStaminaMaxTemp = 0;
            StatStaminaRegenMax = 240;

            if (Abilities.Any(ability => ability.Active))
            {
                // The player cant use items while casting an ability.
                player.noItems = true;
                player.noBuilding = true;
            }

            SetList(); //Update the list to ensure all interactions work correctly
        }

        public override void ProcessTriggers(TriggersSet triggersSet)
        {
            //Dismounts player from mount if any ability (apart from Purify) is used
            if (StarlightRiver.Dash.JustPressed || StarlightRiver.Wisp.JustPressed || StarlightRiver.Smash.JustPressed || StarlightRiver.Superdash.JustPressed)
                player.mount.Dismount(player);
            //Activates one of the player's abilities on the appropriate keystroke.
            if (StarlightRiver.Dash.JustPressed) { triggersSet.Jump = false; dash.StartAbility(player); }
            if (StarlightRiver.Wisp.JustPressed) { wisp.StartAbility(player); }
            if (StarlightRiver.Purify.JustPressed) { pure.StartAbility(player); }
            if (StarlightRiver.Smash.JustPressed) { smash.StartAbility(player); }
            if (StarlightRiver.Superdash.JustPressed) { sdash.StartAbility(player); }
        }

        public override void PreUpdate()
        {
            //Executes the ability's use code while it's active.
            if (player.GetModPlayer<Dragons.DragonHandler>().DragonMounted)
                foreach (Ability ability in Abilities.Where(ability => ability.Active)) { ability.InUseDragon(); ability.UseEffectsDragon(); }
            else
                foreach (Ability ability in Abilities.Where(ability => ability.Active)) { ability.InUse(); ability.UseEffects(); }

            //Decrements internal cooldowns of abilities.
            foreach (Ability ability in Abilities.Where(ability => ability.Cooldown > 0)) { ability.Cooldown--; }

            //Ability cooldown Effects
            foreach (Ability ability in Abilities.Where(ability => ability.Cooldown == 1)) { ability.OffCooldownEffects(); }

            //Physics fuckery due to redcode being retarded
            if (Abilities.Any(ability => ability.Active))
            {
                player.velocity.Y += 0.01f; //Required to ensure that the game never thinks we hit the ground when using an ability. Thanks redcode!

                // We need to store the player's wing or rocket boot time and set the effective time to zero while an ability is active to move upwards correctly. Thanks redcode!
                if (StoredAccessoryTime == 0) { StoredAccessoryTime = ((player.wingTimeMax > 0) ? player.wingTime : player.rocketTime + 1); }
                player.wingTime = 0;
                player.rocketTime = 0;
                player.rocketRelease = true;
                player.fallStart = (int)player.Center.Y;
                player.fallStart2 = (int)player.Center.Y;
            }

            //This restores the player's wings or rocket boots after the ability is over.
            else if (StoredAccessoryTime > 0)
            {
                player.velocity.Y += 0.01f; //We need to do this the frame after also.

                //Makes the determination between which of the two flight accessories the player has.
                if (player.wingTimeMax > 0) { player.wingTime = StoredAccessoryTime; }
                else { player.rocketTime = (int)StoredAccessoryTime - 1; }
                StoredAccessoryTime = 0;
            }

            //Dont exceed max stamina or regenerate stamina when full.
            if (StatStamina >= StatStaminaMax)
            {
                StatStamina = StatStaminaMax;
                StatStaminaRegen = StatStaminaRegenMax;
            }

            //The player's stamina regeneration.
            if (StatStaminaRegen <= 0 && StatStamina < StatStaminaMax)
            {
                StatStamina++;
                StatStaminaRegen = StatStaminaRegenMax;
            }

            //Regenerate only when abilities are not active.
            if (!Abilities.Any(a => a.Active)) { StatStaminaRegen--; }

            //If the player is dead, drain their stamina and disable all of their abilities.
            if (player.dead)
            {
                StatStamina = 0;
                StatStaminaRegen = StatStaminaRegenMax;
                foreach (Ability ability in Abilities) { ability.Active = false; }
            }
        }

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
            if (wisp.Active || sdash.Active)
            {
                foreach (PlayerLayer layer in layers) { layer.visible = false; }
            }
        }
    }
}