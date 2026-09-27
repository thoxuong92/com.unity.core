using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.Core.Events
{
    /// <summary>
    /// Marker interface for all game events.
    /// Structs or classes implementing this can be dispatched via EventBus.
    /// </summary>
    public interface IEvent
    {
    }

    /// <summary>
    /// Type-safe, decoupled Event Bus for Unity Core Framework.
    /// Eliminates direct references between game modules and gameplay logic.
    /// </summary>
    public static class EventBus
    {
        private static readonly Dictionary<Type, List<Delegate>> Subscriptions = new Dictionary<Type, List<Delegate>>();

        /// <summary>
        /// Subscribes a listener callback to an event type.
        /// </summary>
        public static void Subscribe<T>(Action<T> listener) where T : IEvent
        {
            if (listener == null) return;
            var type = typeof(T);

            if (!Subscriptions.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                Subscriptions[type] = list;
            }

            if (!list.Contains(listener))
            {
                list.Add(listener);
            }
        }

        /// <summary>
        /// Unsubscribes a listener callback from an event type.
        /// </summary>
        public static void Unsubscribe<T>(Action<T> listener) where T : IEvent
        {
            if (listener == null) return;
            var type = typeof(T);

            if (Subscriptions.TryGetValue(type, out var list))
            {
                list.Remove(listener);
                if (list.Count == 0)
                {
                    Subscriptions.Remove(type);
                }
            }
        }

        /// <summary>
        /// Publishes an event to all subscribed listeners.
        /// </summary>
        public static void Publish<T>(T eventData) where T : IEvent
        {
            var type = typeof(T);
            if (Subscriptions.TryGetValue(type, out var list))
            {
                // Create copy of listeners to allow unsubscribing inside callbacks safely
                var listenersCopy = new List<Delegate>(list);
                for (int i = 0; i < listenersCopy.Count; i++)
                {
                    try
                    {
                        if (listenersCopy[i] is Action<T> callback)
                        {
                            callback.Invoke(eventData);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        /// <summary>
        /// Clears all event subscriptions (useful when switching scenes or resetting).
        /// </summary>
        public static void ClearAll()
        {
            Subscriptions.Clear();
        }
    }
}
