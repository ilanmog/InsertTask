using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertTask2.Reader {
    [Table("ActualFile")]
    public class ActualFile {
        [Column("Path")]
        public string Path { get; set; }
    }
}
