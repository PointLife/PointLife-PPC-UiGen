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
        public List<BaseField> Fields = new List<BaseField>();

        public virtual IPromise UpdateAll(ShPlayer player)
        {
            return Promise.All(Fields.Select(x => x.GetValue()));
        }
    }

    public abstract class BaseField(string name, ShPlayer player)
    {
        public string Name { get; internal set; } = name;
        public ShPlayer Player { get; internal set; } = player;

        internal Promise? Promise;

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
        public string Text { get; set; }

        public override Promise GetValue()
        {
            Player.svPlayer.GetTextFieldText(Name, callBackEvent);
            return base.GetValue();
        }

        [Obsolete("This is a input field, it can't be updated due to BP missing methode")]
        public void UpdateTo(string text)
        {
            throw new NotImplementedException();
        }

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
        public string Text { get; set; }

        [Obsolete("This is a label, it can't be get Valued due to BP missing methode")]
        public override Promise GetValue()
        {
            throw new NotImplementedException();
        }

        public void UpdateTo(string text)
        {
            Player.svPlayer.SetTextElementText(Name, text);
        }

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

    public class CheckboxField(string name, ShPlayer player, string callBackEvent) : BaseField(name, player)
    {
        public bool CheckboxValue { get; internal set; }

        public override Promise GetValue()
        {
            player.svPlayer.GetToggleValue(Name, callBackEvent);
            return base.GetValue();
        }

        public void UpdateTo(bool value)
        {
            Player.svPlayer.SetToggleValue(Name, value);
        }

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
}