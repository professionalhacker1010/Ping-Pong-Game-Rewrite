/*
Yarn Spinner is licensed to you under the terms found in the file LICENSE.md.
*/

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System;
using System.Linq;

namespace Yarn.Unity
{
    /// <summary>
    /// The DialogueRunner component acts as the interface between your game and
    /// Yarn Spinner.
    /// </summary>
    [AddComponentMenu("Scripts/Yarn Spinner/Custom Dialogue Runner")]
    public class CustomDialogueRunner : DialogueRunner, IActionRegistration
    {

        /// <summary>
        /// A Unity event that is called once the line has started.
        /// </summary>
        public StringUnityEvent onLineStart;

        /// <summary>
        /// A Unity event that is called once the line has started.
        /// </summary>
        public StringUnityEvent onLineUpdate;

        /// <summary>
        /// A Unity event that is called once the line has started.
        /// </summary>
        public UnityEvent onLineComplete;

        private IEnumerator ContinueDialogueWhenLinesAvailable()
        {
            // Wait until lineProvider.LinesAvailable becomes true
            while (lineProvider.LinesAvailable == false)
            {
                yield return null;
            }

            // And then run our dialogue.
            ContinueDialogue();
        }

        /// <summary>
        /// Unloads all nodes from the <see cref="Dialogue"/>.
        /// </summary>
        public void Clear()
        {
            Assert.IsFalse(IsDialogueRunning, "You cannot clear the dialogue system while a dialogue is running.");
            Dialogue.UnloadAll();
        }

        #region Private Properties/Variables/Procedures

        /// <summary>
        /// The <see cref="LocalizedLine"/> currently being displayed on
        /// the dialogue views.
        /// </summary>
        public string CurrentLineText { get { return CurrentLine.Text.Text; } }

        /// <summary>
        ///  The collection of dialogue views that are currently either
        ///  delivering a line, or dismissing a line from being on screen.
        /// </summary>
        private readonly HashSet<DialogueViewBase> ActiveDialogueViews = new HashSet<DialogueViewBase>();

        Action<int> selectAction;

        /// <summary>
        /// The underlying object that executes Yarn instructions
        /// and provides lines, options and commands.
        /// </summary>
        /// <remarks>
        /// Automatically created on first access.
        /// </remarks>
        private Dialogue _dialogue;

        /// <summary>
        /// The current set of options that we're presenting.
        /// </summary>
        /// <remarks>
        /// This value is <see langword="null"/> when the <see
        /// cref="CustomDialogueRunner"/> is not currently presenting options.
        /// </remarks>
        private OptionSet currentOptions;

        /// <summary>
        /// Prepares the Dialogue Runner for start.
        /// </summary>
        /// <remarks>If <see cref="startAutomatically"/> is <see langword="true"/>, the Dialogue Runner will start.</remarks>
        private void Start()
        {
            if (yarnProject != null && startAutomatically)
            {
                StartDialogue(startNode);
            }
        }

        /// <summary>
        /// Forward the line to the dialogue UI.
        /// </summary>
        /// <param name="line">The line to send to the dialogue views.</param>
        internal void HandleLine(Line line)
        {
            // TODO: make a new "lines for node" method that can be called so that people can manually call the preload

            // it is possible at this point depending on the flow into handling the line that the line provider hasn't finished it's loads
            // as such we will need to hold here until the line provider has gotten all it's lines loaded
            // in testing this has been very hard to trigger without having bonkers huge nodes jumping to very asset rich nodes
            // so if you think you are going to hit this you should preload all the lines ahead of time
            // but don't worry about it most of the time
            if (lineProvider.LinesAvailable)
            {
                // we just move on normally
                HandleLineInternal();
            }
            else
            {
                StartCoroutine(WaitUntilLinesAvailable());
            }

            IEnumerator WaitUntilLinesAvailable()
            {
                while (!lineProvider.LinesAvailable)
                {
                    yield return null;
                }
                HandleLineInternal();
            }
            void HandleLineInternal()
            {
                // Get the localized line from our line provider
                CurrentLine = lineProvider.GetLocalizedLine(line);

                // Expand substitutions
                var text = Dialogue.ExpandSubstitutions(CurrentLine.RawText, CurrentLine.Substitutions);

                if (text == null)
                {
                    Debug.LogWarning($"Dialogue Runner couldn't expand substitutions in Yarn Project [{yarnProject.name}] node [{CurrentNodeName}] with line ID [{CurrentLine.TextID}]. "
                        + "This usually happens because it couldn't find text in the Localization. The line may not be tagged properly. "
                        + "Try re-importing this Yarn Program. "
                        + "For now, Dialogue Runner will swap in CurrentLine.RawText.");
                    text = CurrentLine.RawText;
                }

                // Render the markup
                Dialogue.LanguageCode = lineProvider.LocaleCode;

                try
                {
                    CurrentLine.Text = Dialogue.ParseMarkup(text);
                }
                catch (Yarn.Markup.MarkupParseException e)
                {
                    // Parsing the markup failed. We'll log a warning, and
                    // produce a markup result that just contains the raw text.
                    Debug.LogWarning($"Failed to parse markup in \"{text}\": {e.Message}");
                    CurrentLine.Text = new Yarn.Markup.MarkupParseResult
                    {
                        Text = text,
                        Attributes = new List<Yarn.Markup.MarkupAttribute>()
                    };
                }

                // Clear the set of active dialogue views, just in case
                ActiveDialogueViews.Clear();

                onLineStart.Invoke(CurrentLine.Text.Text);

                // the following is broken up into two stages because otherwise if the 
                // first view happens to finish first once it calls dialogue complete
                // it will empty the set of active views resulting in the line being considered
                // finished by the runner despite there being a bunch of views still waiting
                // so we do it over two loops.
                // the first finds every active view and flags it as such
                // the second then goes through them all and gives them the line

                // Mark this dialogue view as active
                foreach (var dialogueView in dialogueViews)
                {
                    if (dialogueView == null || dialogueView.isActiveAndEnabled == false)
                    {
                        continue;
                    }

                    ActiveDialogueViews.Add(dialogueView);
                }
                // Send line to all active dialogue views
                foreach (var dialogueView in dialogueViews)
                {
                    if (dialogueView == null || dialogueView.isActiveAndEnabled == false)
                    {
                        continue;
                    }

                    dialogueView.RunLine(CurrentLine,
                        () => DialogueViewCompletedDelivery(dialogueView));
                }
            }
        }

        // called by the runner when a view has signalled that it needs to interrupt the current line
        void InterruptLine()
        {
            ActiveDialogueViews.Clear();

            foreach (var dialogueView in dialogueViews)
            {
                if (dialogueView == null || dialogueView.isActiveAndEnabled == false)
                {
                    continue;
                }

                ActiveDialogueViews.Add(dialogueView);
            }

            foreach (var dialogueView in dialogueViews)
            {
                dialogueView.InterruptLine(CurrentLine, () => DialogueViewCompletedInterrupt(dialogueView));
            }
        }

        /// <summary>
        /// Indicates to the DialogueRunner that the user has selected an
        /// option
        /// </summary>
        /// <param name="optionIndex">The index of the option that was
        /// selected.</param>
        /// <exception cref="InvalidOperationException">Thrown when the
        /// <see cref="IsOptionSelectionAllowed"/> field is <see
        /// langword="true"/>, which is the case when <see
        /// cref="DialogueViewBase.RunOptions(DialogueOption[],
        /// Action{int})"/> is in the middle of being called.</exception>
        void SelectedOption(int optionIndex)
        {
/*            if (IsOptionSelectionAllowed == false)
            {
                throw new InvalidOperationException("Selecting an option on the same frame that options are provided is not allowed. Wait at least one frame before selecting an option.");
            }*/

            // Mark that this is the currently selected option in the
            // Dialogue
            Dialogue.SetSelectedOption(optionIndex);

            if (runSelectedOptionAsLine)
            {
                foreach (var option in currentOptions.Options)
                {
                    if (option.ID == optionIndex)
                    {
                        HandleLine(option.Line);
                        return;
                    }
                }

                Debug.LogError($"Can't run selected option ({optionIndex}) as a line: couldn't find the option's associated {nameof(Line)} object");
                ContinueDialogue();
            }
            else
            {
                ContinueDialogue();
            }

        }

        /// <summary>
        /// Called when a <see cref="DialogueViewBase"/> has finished
        /// delivering its line.
        /// </summary>
        /// <param name="dialogueView">The view that finished delivering
        /// the line.</param>
        private void DialogueViewCompletedDelivery(DialogueViewBase dialogueView)
        {
            // A dialogue view just completed its delivery. Remove it from
            // the set of active views.
            ActiveDialogueViews.Remove(dialogueView);

            // Have all of the views completed? 
            if (ActiveDialogueViews.Count == 0)
            {
                DismissLineFromViews(dialogueViews);
                onLineComplete.Invoke();
            }
        }

        // this is similar to the above but for the interrupt
        // main difference is a line continues automatically every interrupt finishes
        private void DialogueViewCompletedInterrupt(DialogueViewBase dialogueView)
        {
            ActiveDialogueViews.Remove(dialogueView);

            if (ActiveDialogueViews.Count == 0)
            {
                DismissLineFromViews(dialogueViews);
                onLineComplete.Invoke();
            }
        }

        void ContinueDialogue(bool dontRestart = false)
        {
            if (dontRestart == true)
            {
                if (Dialogue.IsActive == false)
                {
                    return;
                }
            }

            CurrentLine = null;
            Dialogue.Continue();
        }

        /// <summary>
        /// Called by a <see cref="DialogueViewBase"/> derived class from
        /// <see cref="dialogueViews"/> to inform the <see
        /// cref="DialogueRunner"/> that the user intents to proceed to the
        /// next line.
        /// </summary>
        public void OnViewRequestedInterrupt()
        {
            if (CurrentLine == null)
            {
                Debug.LogWarning("Dialogue runner was asked to advance but there is no current line");
                return;
            }

            // asked to advance when there are no active views
            // this means the views have already processed the lines as needed
            // so we can ignore this action
            if (ActiveDialogueViews.Count == 0)
            {
                Debug.Log("user requested advance, all views finished, ignoring interrupt");
                return;
            }

            // now because lines are fully responsible for advancement the only advancement allowed is interruption
            InterruptLine();
        }

        private void DismissLineFromViews(IEnumerable<DialogueViewBase> dialogueViews)
        {
            ActiveDialogueViews.Clear();

            foreach (var dialogueView in dialogueViews)
            {
                // Skip any dialogueView that is null or not enabled
                if (dialogueView == null || dialogueView.isActiveAndEnabled == false)
                {
                    continue;
                }

                // we do this in two passes - first by adding each
                // dialogueView into ActiveDialogueViews, then by asking
                // them to dismiss the line - because calling
                // view.DismissLine might immediately call its completion
                // handler (which means that we'd be repeatedly returning
                // to zero active dialogue views, which means
                // DialogueViewCompletedDismissal will mark the line as
                // entirely done)
                ActiveDialogueViews.Add(dialogueView);
            }

            foreach (var dialogueView in dialogueViews)
            {
                if (dialogueView == null || dialogueView.isActiveAndEnabled == false)
                {
                    continue;
                }

                dialogueView.DismissLine(() => DialogueViewCompletedDismissal(dialogueView));
            }
        }

        private void DialogueViewCompletedDismissal(DialogueViewBase dialogueView)
        {
            // A dialogue view just completed dismissing its line. Remove
            // it from the set of active views.
            ActiveDialogueViews.Remove(dialogueView);

            // Have all of the views completed dismissal? 
            if (ActiveDialogueViews.Count == 0)
            {
                // Then we're ready to continue to the next piece of
                // content.
                ContinueDialogue();
            }
        }
        #endregion


    }
}
