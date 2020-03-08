using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Linq;

public class Loom
{
    public static bool RunAsync(Action a)
    {
        return interLoom.RunAsync(a);
    }

    public static void RunOnMainThread(Action<object> taction, object tparam)
    {
        interLoom.QueueOnMainThread(taction, tparam, 0);
    }

    public static void RunOnMainThread(Action<object> taction, object tparam, float fDelay)
    {
        interLoom.QueueOnMainThread(taction, tparam, fDelay);
    }

    class interLoom : MonoBehaviour
    {

        private static interLoom _current;

        static bool initialized = false;

        public static void Initialize()
        {
            if (!initialized)
            {

                if (!Application.isPlaying)
                    return;
                initialized = true;
                var g = new GameObject("interLoom");
                _current = g.AddComponent<interLoom>();
#if !ARTIST_BUILD
                DontDestroyOnLoad(g);
#endif
            }

        }
        struct NoDelayedQueueItem
        {
            public Action<object> action;
            public object param;
        }

        private List<NoDelayedQueueItem> _actions = new List<NoDelayedQueueItem>();
        struct DelayedQueueItem
        {
            public float time;
            public Action<object> action;
            public object param;
        }
        private List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();

        List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();

        public static void QueueOnMainThread(Action<object> taction, object tparam, float time)
        {
            if (!initialized)
            {
                Initialize();
            }

            if (time != 0)
            {
                lock (_current._delayed)
                {
                    _current._delayed.Add(new DelayedQueueItem { time = Time.time + time, action = taction, param = tparam });
                }
            }
            else
            {
                lock (_current._actions)
                {
                    _current._actions.Add(new NoDelayedQueueItem { action = taction, param = tparam });
                }
            }
        }

        public static bool RunAsync(Action a)
        {
            if (!initialized)
            {
                Initialize();
            }
            //Interlocked.Increment(ref numThreads);
            return ThreadPool.QueueUserWorkItem(RunAction, a);
        }

        private static void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch
            {
            }
            //finally
            //{
            //    Interlocked.Decrement(ref numThreads);
            //}

        }

        List<NoDelayedQueueItem> _currentActions = new List<NoDelayedQueueItem>();

        // Update is called once per frame
        private void LateUpdate()
        {
            if (_actions.Count > 0)
            {
                lock (_actions)
                {
                    _currentActions.Clear();
                    _currentActions.AddRange(_actions);
                    _actions.Clear();
                }
                for (int i = 0; i < _currentActions.Count; i++)
                {
                    _currentActions[i].action(_currentActions[i].param);
                }
            }

            if (_delayed.Count > 0)
            {
                lock (_delayed)
                {
                    _currentDelayed.Clear();
                    _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
                    for (int i = 0; i < _currentDelayed.Count; i++)
                    {
                        _delayed.Remove(_currentDelayed[i]);
                    }
                }

                for (int i = 0; i < _currentDelayed.Count; i++)
                {
                    _currentDelayed[i].action(_currentDelayed[i].param);
                }
            }
        }
    }

}

