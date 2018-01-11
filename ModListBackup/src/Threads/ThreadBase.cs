using System;
using System.Threading;

namespace ModListBackup.Threads {

    internal abstract class ThreadBase {
        internal readonly Action OnFinishedAction;
        internal Thread _Thread;
        internal bool OnlyOneAllowed;

        internal ThreadBase(Type owner, Action onFinishAction = null) {
            this.OnFinishedAction = onFinishAction;
            OwnerType = owner;
        }

        internal Type OwnerType { get; private set; }
        internal int ThreadId => _Thread.ManagedThreadId;

        internal virtual void Abort() {
            _Thread.Abort();
        }

        internal abstract void Start();

        internal void OnFinished() {
            if (_Thread.ThreadState == ThreadState.Running) {
                OnFinishedAction();
            }
            ThreadFactory.TryDestroy(this);
        }
    }
}