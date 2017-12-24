using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertTask2.Reader {
    public interface IDBHandler {
        void Init();
        InsertTaskSetting GetSetting(string property);
        void MergeActualFiles(IEnumerable<ActualFile> actualFiles);
        void MergeSettings(InsertTaskSetting settings);
        void InsertGenericTask(GenericTask task);
        void ClearTasks();
        ICollection<GenericTask> GetTasks();
    }
}
