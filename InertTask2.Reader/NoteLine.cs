using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertTask2.Reader {
    public class NoteLine {
        public string Line { get; set; }
        public int LineIndex { get; set; }
        public int FromIndex { get; set; }
        public int ToIndex { get; set; }
        public override string ToString() {
            return string.Format("Line={0}, LineIndex={1}, FromIndex={2}, ToIndex={3}", Line, LineIndex, FromIndex, ToIndex);
        }
    }
}
