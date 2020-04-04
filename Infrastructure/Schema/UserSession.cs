using System;
using Database.ContraWebDB;

namespace Infrastructure.Schema
{
    public class UserSession
    {
        public string   Username    { get; set; }
        public string   Token       { get; set; }
        public DateTime CreatedAt   { get; set; }
        public DateTime RefreshedAt { get; set; }

        public UserSession(User user, string token)
        {
            Username = user.Username;
            Token    = token;
        }

        public UserSession(user_session userSession)
        {
            Username    = userSession.username;
            Token       = userSession.token;
            CreatedAt   = userSession.created_at;
            RefreshedAt = userSession.refreshed_at;
        }
    }
}