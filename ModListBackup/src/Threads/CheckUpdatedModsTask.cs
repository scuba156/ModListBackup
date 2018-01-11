using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModListBackup.Threads {
    internal class CheckUpdatedModsTask : ThreadBase {
        private readonly string[] Paths;

        internal override void Start() {
            
        }

        internal override void Abort() {
            throw new NotImplementedException();
        }

        internal CheckUpdatedModsTask(string[] paths, Type owner, Action onFinishAction = null) : base(owner, onFinishAction) {
            Paths = paths;

        }
    }
}
