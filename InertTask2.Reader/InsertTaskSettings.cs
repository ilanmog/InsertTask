using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertTask2.Reader {
    [Table("InsertTaskSetting")]
    public class InsertTaskSetting {
        [PrimaryKey(),Column("Name"),Unique]
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
