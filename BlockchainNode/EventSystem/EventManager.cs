namespace StructEventSystem
{
    public static class EventManager
    {
        private static Dictionary<Type, List<EventListenerBase>> _subscribers;

        static EventManager()
        {
            _subscribers = new Dictionary<Type, List<EventListenerBase>>();
        }

        public static void AddListener<T>(EventListener<T> listener) where T : struct
        {
            Type evenType = typeof(T);

            if (!_subscribers.ContainsKey(evenType))
                _subscribers[evenType] = new List<EventListenerBase>();

            if (!SubscriptionExists(evenType, listener))
                _subscribers[evenType].Add(listener);
        }
        public static void RemoveListener<T>(EventListener<T> listener) where T : struct
        {
            Type eventType = typeof(T);

            if (!_subscribers.ContainsKey(eventType)) { return; }

            var indexToRemove = _subscribers[eventType].FindIndex(x => x == listener);
            if (indexToRemove != -1)
            {
                _subscribers[eventType].RemoveAt(indexToRemove);
                if (_subscribers[eventType].Count == 0)
                    _subscribers.Remove(eventType);
            }
        }
        public static void TriggerEvent<T>(T eventToTrigger) where T : struct
        {
            if (_subscribers.Count == 0) { return; }
            Type eventType = typeof(T);
            if (!_subscribers.ContainsKey(eventType)) { return; }

            var fixedList = new List<EventListenerBase>(_subscribers[eventType]);
            foreach (var eBase in fixedList)
            {
                var e = eBase as EventListener<T>;
                if (e != null)
                    e.OnEvent(eventToTrigger);
            }
        }

        private static bool SubscriptionExists(Type type, EventListenerBase listener)
        {
            if (_subscribers.ContainsKey(type))
                return _subscribers[type].Contains(listener);
            return false;
        }
    }

    public static class EventRegistrator
    {
        public static void StartListening<T>(this EventListener<T> caller) where T : struct
        {
            EventManager.AddListener<T>(caller);
        }
        public static void StopListening<T>(this EventListener<T> caller) where T : struct
        {
            EventManager.RemoveListener<T>(caller);
        }
    }


    public interface EventListenerBase { };

    public interface EventListener<T> : EventListenerBase where T : struct
    {
        void OnEvent(T eventType);
    };
}

