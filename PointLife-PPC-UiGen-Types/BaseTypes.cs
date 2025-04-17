using BrokeProtocol.Entities;
using RSG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace PointLife.UiGen.BaseFields
{
    public class BaseMenuFields
    {
        /// <summary>
        /// List of fields in the menu, tracking for UpdateAll.
        /// </summary>
        public List<BaseField> Fields { get; } = new();

        /// <summary>
        /// Updates all fields by getting their values.
        /// </summary>
        /// <returns>A promise that resolves when all fields have been received from the client.</returns>
        public virtual IPromise UpdateAll()
        {
            return Promise.All(Fields.Select(x => x.GetValue()));
        }
    }

    public abstract class BaseField(string name, ShPlayer player)
    {
        /// <summary>
        /// The name of the field.
        /// </summary>
        public string Name { get; internal set; } = name;

        /// <summary>
        /// Gets the player associated with the field.
        /// </summary>
        public ShPlayer Player { get; internal set; } = player;

        internal Promise? Promise;

        /// <summary>
        /// Receives data from the client and updates the value.
        /// </summary>
        public virtual Promise GetValue()
        {
            if (Promise == null || Promise.CurState == PromiseState.Resolved)
            {
                return Promise = new Promise();
            }
            else
            {
                return Promise;
            }
        }
    }

    // User Input
    public class TextField(string name, ShPlayer player, string callBackEvent) : BaseField(name, player)
    {
        /// <summary>
        /// The current text value of the TextField.
        /// </summary>
        public string Text { get; private set; }

        /// <inheritdoc/>
        public override Promise GetValue()
        {
            Player.svPlayer.GetTextFieldText(Name, callBackEvent);
            return base.GetValue();
        }


        /// <summary>
        /// Updates the TextField text.
        /// </summary>
        /// <param name="text">The new text of the TextField.</param>
        [Obsolete("This is a input field, it can't be updated due to BP missing methode")]
        public void UpdateTo(string text)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Receives data from the client and updates the value.
        /// !!Only to be called from the generated code.!!
        /// </summary>
        /// <param name="data">The new value.</param>
        public void _DataReceivedFromClient(string data)
        {
            if (Promise.CurState == PromiseState.Resolved)
            {
                UnityEngine.Debug.LogWarning($"{Name} was already resolved!");
                if (Text != data)
                {
                    UnityEngine.Debug.LogError($"{Name} was already resolved, while diffrent value arrived!");
                }
                Text = data;
            }
            else
            {
                Text = data;
                Promise.Resolve();
            }
        }
    }

    // Label
    public class TextElement(string name, ShPlayer player, string callBackEvent) : BaseField(name, player)
    {
        /// <summary>
        /// The current text value. Use <see cref="UpdateTo(string)"/> to change it.
        /// </summary>
        public string Text { get; private set; }

        /// <inheritdoc/>
        [Obsolete("This is a label, it can't be get Valued due to BP missing methode")]
        public override Promise GetValue()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the TextElement text.
        /// </summary>
        /// <param name="text">The new text of the TextElement.</param>
        public void UpdateTo(string text)
        {
            Text = text;
            Player.svPlayer.SetTextElementText(Name, text);
        }

        /// <summary>
        /// Receives data from the client and updates the value.
        /// !!Only to be called from the generated code.!!
        /// </summary>
        /// <param name="data">The new value.</param>
        public void DataReceivedFromClient(string data)
        {
            if (Promise.CurState == PromiseState.Resolved)
            {
                UnityEngine.Debug.LogWarning($"{Name} was already resolved!");
                if (Text != data)
                {
                    UnityEngine.Debug.LogError($"{Name} was already resolved, while diffrent value arrived!");
                }
                Text = data;
            }
            else
            {
                Text = data;
                Promise.Resolve();
            }
        }
    }

    public class CheckboxField(string name, ShPlayer player, string callBackEvent) : BaseField(name, player)
    {
        public bool CheckboxValue { get; internal set; }

        /// <inheritdoc/>
        public override Promise GetValue()
        {
            player.svPlayer.GetToggleValue(Name, callBackEvent);
            return base.GetValue();
        }

        /// <summary>
        /// Updates the checkbox value.
        /// </summary>
        /// <param name="value">The new value of the checkbox.</param>
        public void UpdateTo(bool value)
        {
            Player.svPlayer.SetToggleValue(Name, value);
        }

        /// <summary>
        /// Receives data from the client and updates the value.
        /// !!Only to be called from the generated code.!!
        /// </summary>
        /// <param name="data">The new value.</param>
        public void _DataReceivedFromClient(bool data)
        {
            if (Promise.CurState == RSG.PromiseState.Resolved)
            {
                UnityEngine.Debug.LogWarning($"{Name} was already resolved!");
                if (CheckboxValue != data)
                {
                    UnityEngine.Debug.LogError($"{Name} was already resolved, while diffrent value arrived!");
                }
                CheckboxValue = data;
            }
            else
            {
                CheckboxValue = data;
                Promise.Resolve();
            }
        }
    }

    public class ProgressBarField(string name, ShPlayer player, string callBackEvent) : BaseField(name, player)
    {
        public float Value { get; internal set; }

        [Obsolete("Progress Bar can only be set.", true)]
        public override Promise GetValue()
        {
            return base.GetValue();
        }

        public void UpdateTo(float value)
        {
            Value = value;
            Player.svPlayer.SetProgressBarValue(Name, Value);
        }
    }
}