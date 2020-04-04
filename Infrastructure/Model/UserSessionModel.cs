using System;
using System.Linq;
using System.Threading.Tasks;
using Database.ContraWebDB;
using Infrastructure.Schema;

namespace Infrastructure.Model
{
    public static class UserSessionModel
    {
        public static async Task<UserSession?> Find(User user)
        {
            await using ContraWebDBContext contraDB = new ContraWebDBContext();

            user_session? match = contraDB.user_session.SingleOrDefault(v => v.username == user.Username);
            return match == null ? null : new UserSession(match);
        }

        public static async Task<UserSession?> Find(string token)
        {
            await using ContraWebDBContext contraDB = new ContraWebDBContext();

            user_session? match = contraDB.user_session.SingleOrDefault(v => v.token == token);
            return match == null ? null : new UserSession(match);
        }

        public static async Task Create(User user, string token)
        {
            await using ContraWebDBContext contraDB = new ContraWebDBContext();

            user_session dbSession = new user_session
            {
                username     = user.Username,
                token        = token,
                created_at   = DateTime.Now,
                refreshed_at = DateTime.Now
            };

            contraDB.Add(dbSession);
            await contraDB.SaveChangesAsync();
        }

        public static async Task Delete(User user)
        {
            await using ContraWebDBContext contraDB = new ContraWebDBContext();

            user_session? match = contraDB.user_session.SingleOrDefault(v => v.username == user.Username);
            if (match == null)
                return;

            contraDB.Remove(match);
            await contraDB.SaveChangesAsync();
        }

        public static async Task Refresh(User user)
        {
            await using ContraWebDBContext contraDB = new ContraWebDBContext();

            user_session? match = contraDB.user_session.SingleOrDefault(v => v.username == user.Username);
            if (match == null)
                return;

            match.refreshed_at = DateTime.Now;

            contraDB.Update(match);
            await contraDB.SaveChangesAsync();
        }
    }
}