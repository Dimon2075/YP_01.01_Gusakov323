using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace УП_01._01_Gusakov323
{
    internal class Core
    {
        public static ReadWriteNoSleepEntities Context = new ReadWriteNoSleepEntities();

        public static void ResetContext()
        {
            Context = new ReadWriteNoSleepEntities();
        }
    }
}
