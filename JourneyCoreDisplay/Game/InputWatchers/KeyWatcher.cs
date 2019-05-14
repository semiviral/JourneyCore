using System;
using System.Collections.Generic;
using System.Linq;
using SFML.Window;

namespace JourneyCoreLib.Game.InputWatchers
{
    public class KeyWatcher
    {
        private List<KeyWatch> _watchedKeys;

        public KeyWatcher()
        {
            _watchedKeys = new List<KeyWatch>();
        }

        #region METHODS

        public void AddWatchedKeyAction(Keyboard.Key key, Action<Keyboard.Key> keyAction)
        {
            if (!GetWatchedKeys().Contains(key))
            {
                _watchedKeys.Add(new KeyWatch(key));
            }

            GetKeyWatch(key).AddKeyAction(keyAction);
        }

        public void RemoveWatchedKeyAction(Keyboard.Key key, Action<Keyboard.Key> keyAction)
        {
            if (!GetWatchedKeys().Contains(key))
            {
                throw new ArgumentException($"Keyboard.Key {key} does not exist in watched keys list.");
            }

            GetKeyWatch(key).RemoveKeyAction(keyAction);
        }

        public List<Keyboard.Key> GetWatchedKeys()
        {
            return _watchedKeys.Select(keyWatch => keyWatch.Key).ToList();
        }

        public KeyWatch GetKeyWatch(Keyboard.Key key)
        {
            return _watchedKeys.SingleOrDefault(keyWatch => keyWatch.Key.Equals(key));
        }

        public void CheckWatchedKeys()
        {
            foreach (Keyboard.Key watchedKey in GetWatchedKeys())
            {
                if (Keyboard.IsKeyPressed(watchedKey))
                {
                    GetKeyWatch(watchedKey).Invoke();
                }
            }
        }

        #endregion
    }
}
