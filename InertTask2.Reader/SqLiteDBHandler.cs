using SQLite;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertTask2.Reader {
    public class SqLiteDBHandler : IDBHandler {
        private SQLiteConnection _db;

        public void ClearTasks() {
            _db.DeleteAll<GenericTask>();
        }

        public InsertTaskSetting GetSetting(string property) {
            return _db.Find<InsertTaskSetting>(property);
        }

        public ICollection<GenericTask> GetTasks() {
            return _db.Table<GenericTask>().ToList();
        }

        public void Init() {
            SetupSql();
        }

        public void InsertGenericTask(GenericTask task) {
            _db.Insert(task);
        }

        public void MergeActualFiles(IEnumerable<ActualFile> actualFiles) {
            _db.DeleteAll<ActualFile>();
            _db.InsertAll(actualFiles);
        }
        public void MergeSettings(InsertTaskSetting settings) {
            _db.InsertOrReplace(settings);
        }
        private void SetupSql() {
            string dbPath = Path.Combine(
                System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), "insertTask.db3");
            _db = new SQLiteConnection(dbPath);
            _db.CreateTable<GenericTask>();
            _db.CreateTable<InsertTaskSetting>();
            _db.CreateTable<ActualFile>();
        }
    }
}
