using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertTask2.Reader {
    [Table("GenericTask")]
    public class GenericTask {
        [Unique,PrimaryKey(),MaxLength(8),Column("Identifier")]
        public string Identifier { get; set; }
        [Column("Title")]
        public string Title { get; set; }
        [Column("IsOutside")]
        public bool IsOutside { get; set; }
        [Column("Category")]
        public string Category { get; set; }
        [Column("IsCompleted")]
        public bool IsCompleted { get; set; }
        [Ignore]
        public string StructuredTitle {
            get {
                return Identifier + "::" + Title;
            }
        }
        public override string ToString() {
            return StructuredTitle;
        }
    }
}
