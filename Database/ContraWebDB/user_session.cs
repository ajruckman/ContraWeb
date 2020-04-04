using System;
using System.Collections.Generic;

namespace Database.ContraWebDB
{
    public partial class user_session
    {
        public string username { get; set; }
        public string token { get; set; }
        public DateTime created_at { get; set; }
        public DateTime refreshed_at { get; set; }

        public virtual user usernameNavigation { get; set; }
    }
}
