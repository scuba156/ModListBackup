using ModListBackup.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Verse;

namespace ModListBackup.Threads {

    internal class GetDirectorySizeTask {
        private readonly List<DirectoryInfo> Paths;
        internal long Result;
        private Thread _Thread;
        private Action<long> OnFinishedAction;

        internal GetDirectorySizeTask(List<DirectoryInfo> paths, Action<long> onFinishedAction = null) {
            Paths = paths;
            OnFinishedAction = onFinishedAction;
            //OnlyOneAllowed = true;
        }

        internal void Start() {
            Log.Message("Starting thread");
            Result = 0;
            _Thread = new Thread(() => {
                foreach (var path in Paths) {
                    if (_Thread.ThreadState == ThreadState.AbortRequested) {
                        return;
                    }
                    Result += PathUtils.GetDirectorySize(path);
                }
                if (_Thread.ThreadState == ThreadState.AbortRequested || OnFinishedAction == null) {
                    return;
                }
                OnFinishedAction(Result);
            });
            _Thread.Start();
        }
    }
}