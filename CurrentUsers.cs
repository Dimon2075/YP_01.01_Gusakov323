using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace УП_01._01_Gusakov323
{
    public static class CurrentUser
    {
        public static Users User { get; set; }
        public static bool IsAdmin => User?.RoleID == 3;
        public static bool IsAuthor => User?.RoleID == 2;
        public static bool IsFrozen => User?.IsFrozen == true;

    }
}
