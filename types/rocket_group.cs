using System;
using System.Collections.Generic;

namespace interception.libraries.rocketfasterpermissions.types {
    internal class rocket_group {
        //public string display_name { get; set; }
        //public string prefix { get; set; }
        //public string suffix { get; set; }
        //public string color { get; set; }
        public HashSet<ulong> members { get; set; }
        public HashSet<string> permissions { get; set; }
        public string parent_group { get; set; }
        //public short priority { get; set; }
    }
}
