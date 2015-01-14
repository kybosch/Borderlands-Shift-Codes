using BorderlandsShiftKeys;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BorderlandsShiftKeys
{
    class Program
    {
        static void Main(string[] args)
        {
            string url = "http://orcz.com/Borderlands_Pre-Sequel:_Shift_Codes";
            new ShiftKeyUpdater().CheckDatabaseKeys(new ShiftKeyUpdater().GetShiftKeys(url));
            new ShiftKeyUpdater().SendEmail();
        }
    }
}
