using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Verse;

namespace ModListBackup.Threads {
    internal class ThreadFactory {
        static ThreadFactory _instance;
        readonly List<ThreadBase> Threads = new List<ThreadBase>();

        internal static ThreadFactory Instance() {
            Log.Message("Getting Thread Factory Instance");
            if (_instance == null) {
                _instance = new ThreadFactory();
            }
            return _instance;
        }

        internal static void Start(ThreadBase thread) {
            if (thread == null) {
                throw new ArgumentNullException(nameof(thread));
            }

            if (thread.OnlyOneAllowed) {
                foreach(var t in _instance.Threads) {
                    t.Abort();
                }
            }

            thread.Start();
            _instance.Threads.Add(thread);
        }

        internal static void StopAllThreads() {
            foreach (var thread in _instance.Threads) {
                thread.Abort();
            }
        }

        internal static void StopAnyThreadsOwnedBy(Type owner) {
            foreach (var thread in AllThreadsOfOwner(owner)) {
                thread.Abort();
            }
            Update();
        }

        internal static IEnumerable<ThreadBase> AllThreadsOfOwner(Type owner) {
            foreach (var thread in _instance.Threads.Where(t=> t.OwnerType == owner)) {
                yield return thread;
            }
        }

        internal static void TryDestroy(ThreadBase thread) {
            if (!_instance.Threads.Contains(thread)) {
                Log.Message("Thread for owner does not exist: " + thread.OwnerType);
            } else {
                _instance.Threads.Remove(thread);
            }

            thread = null;
        }

        internal static void Update() {
            //Memory leak
            _instance.Threads.RemoveAll(t => t._Thread.ThreadState == (ThreadState.Aborted | ThreadState.AbortRequested | ThreadState.Stopped));
        }
    }
}
