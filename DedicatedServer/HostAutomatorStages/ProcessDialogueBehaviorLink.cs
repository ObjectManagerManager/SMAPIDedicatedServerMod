using DedicatedServer.Config;
using StardewValley;
using StardewValley.Menus;
using System.Reflection;

namespace DedicatedServer.HostAutomatorStages
{
    internal class ProcessDialogueBehaviorLink : BehaviorLink
    {
        private static FieldInfo textBoxFieldInfo = typeof(NamingMenu).GetField("textBox", BindingFlags.NonPublic | BindingFlags.Instance);

        private static MethodInfo itemListMenuInfo = typeof(ItemListMenu).GetMethod("okClicked", BindingFlags.Instance | BindingFlags.NonPublic);

        private ModConfig config;

        public ProcessDialogueBehaviorLink(ModConfig config, BehaviorLink next = null) : base(next)
        {
            this.config = config;
        }

        public override void Process(BehaviorState state)
        {
            if (Game1.activeClickableMenu != null)
            {
                if (Game1.activeClickableMenu is DialogueBox db)
                {
                    if (state.HasBetweenDialoguesWaitTicks())
                    {
                        state.DecrementBetweenDialoguesWaitTicks();
                    }
                    else if (!db.isQuestion)
                    {
                        db.receiveLeftClick(0, 0); // Skip the non-question dialogue
                        state.SkipDialogue();
                    }
                    else
                    {
                        // For question dialogues, determine which question is being asked based on the
                        // question / response text
                        int mushroomsResponseIdx = -1;
                        int batsResponseIdx = -1;
                        int yesResponseIdx = -1;
                        int noResponseIdx = -1;
                        for (int i = 0; i < db.responses.Count; i++)
                        {
                            var response = db.responses[i];
                            var lowercaseText = response.responseText.ToLower();
                            if (lowercaseText == "mushrooms")
                            {
                                mushroomsResponseIdx = i;
                            }
                            else if (lowercaseText == "bats")
                            {
                                batsResponseIdx = i;
                            }
                            else if (lowercaseText == "yes")
                            {
                                yesResponseIdx = i;
                            }
                            else if (lowercaseText == "no")
                            {
                                noResponseIdx = i;
                            }
                        }

                        db.selectedResponse = 0;
                        if (mushroomsResponseIdx >= 0 && batsResponseIdx >= 0)
                        {
                            // This is the cave question. Answer based on mod config.
                            if (config.MushroomsOrBats.ToLower() == "mushrooms")
                            {
                                db.selectedResponse = mushroomsResponseIdx;
                            }
                            else if (config.MushroomsOrBats.ToLower() == "bats")
                            {
                                db.selectedResponse = batsResponseIdx;
                            }
                        }
                        else if (yesResponseIdx >= 0 && noResponseIdx >= 0)
                        {
                            // This is the pet question. Answer based on mod config.
                            if (config.AcceptPet)
                            {
                                db.selectedResponse = yesResponseIdx;
                            }
                            else
                            {
                                db.selectedResponse = noResponseIdx;
                            }
                        }

                        db.receiveLeftClick(0, 0);
                        state.SkipDialogue();
                    }
                }
                else if (Game1.activeClickableMenu is NamingMenu nm)
                {
                    if (state.HasBetweenDialoguesWaitTicks())
                    {
                        state.DecrementBetweenDialoguesWaitTicks();
                    }
                    else
                    {
                        TextBox textBox = (TextBox) textBoxFieldInfo.GetValue(nm);
                        textBox.Text = config.PetName;
                        textBox.RecieveCommandInput('\r');
                        state.SkipDialogue();
                    }
                }
                else if (Game1.activeClickableMenu is LevelUpMenu lum) 
                {
                    if (state.HasBetweenDialoguesWaitTicks())
                    {
                        state.DecrementBetweenDialoguesWaitTicks();
                    }
                    else
                    {
                        lum.okButtonClicked();
                    }
                }
                else if (Game1.activeClickableMenu is ItemListMenu ilm)
                {
                    // Lost item dialog when the host faints
                    itemListMenuInfo?.Invoke(ilm, new object[] { });
                    state.SkipDialogue();
                }
                else
                {
                    state.ClearBetweenDialoguesWaitTicks();
                    processNext(state);
                }
            }
            else
            {
                state.ClearBetweenDialoguesWaitTicks();
                processNext(state);
            }
        }
    }
}
