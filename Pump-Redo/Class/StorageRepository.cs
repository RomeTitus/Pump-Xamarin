using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Auth.Repository;
using Newtonsoft.Json;
using Pump.Database;
using Pump.Database.Table;

namespace Pump.Class
{
    public class StorageRepository : IUserRepository
    {
        private readonly DatabaseController _database = new DatabaseController();


        public Task<bool> UserExistsAsync()
        {
            return Task.FromResult(_database.GetUserAuthentication() != null);
        }

        public Task<(UserInfo userInfo, FirebaseCredential credential)> ReadUserAsync()
        {
            var userDetail = _database.GetUserAuthentication() ?? new UserAuthentication();
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(userDetail.UserInfo);
            var userAuth = JsonConvert.DeserializeObject<FirebaseCredential>(userDetail.FirebaseCredential);
            return Task.FromResult((userInfo, userAuth));
        }

        public Task SaveUserAsync(User user)
        {
            var userAuthentication = new UserAuthentication
            {
                UserInfo = JsonConvert.SerializeObject(user.Info),
                FirebaseCredential = JsonConvert.SerializeObject(user.Credential)
            };

            _database.SaveUserAuthentication(userAuthentication);
            return Task.CompletedTask;
        }

        public Task DeleteUserAsync()
        {
            _database.DeleteUserAuthentication();
            return Task.CompletedTask;
        }
    }
}