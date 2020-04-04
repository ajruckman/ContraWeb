using System;
using System.Collections.Generic;

namespace Database.ContraWebDB
{
    public partial class user
    {
        public string username { get; set; }
        public string salt { get; set; }
        public string password { get; set; }
        public string role { get; set; }

        public virtual user_session user_session { get; set; }
    }
}
