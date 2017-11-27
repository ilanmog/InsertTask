using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InertTask2.Reader {
    public class DBHandlerFactory : IDBHandlerFactory {
        public IDBHandler CreateDBHandler() {
            return new SqLiteDBHandler();
        }
    }
}
