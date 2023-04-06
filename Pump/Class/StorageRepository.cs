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

        public Task<(UserInfo userInfo, FirebaseCredential credential)> ReadUserAsync()
        {
            var userDetail = _database.GetUserAuthentication() ?? new UserAuthentication();
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(userDetail.UserInfo);
            var userAuth = JsonConvert.DeserializeObject<FirebaseCredential>(userDetail.UserInfo);
            return Task.FromResult((userInfo, userAuth));
        }

        public bool UserExists()
        {
            return _database.GetUserAuthentication() != null;
        }

        public (UserInfo userInfo, FirebaseCredential credential) ReadUser()
        {
            var userDetail = _database.GetUserAuthentication() ?? new UserAuthentication();
            var userInfo = JsonConvert.DeserializeObject<UserInfo>(userDetail.UserInfo);
            var userAuth = JsonConvert.DeserializeObject<FirebaseCredential>(userDetail.UserInfo);
            return (userInfo, userAuth);
        }

        public void SaveUser(User user)
        {
            var userAuthentication = new UserAuthentication
            {
                UserInfo = JsonConvert.SerializeObject(user.Info),
                FirebaseCredential = JsonConvert.SerializeObject(user.Credential)
            };

            _database.SaveUserAuthentication(userAuthentication);
        }

        public void DeleteUser()
        {
            _database.DeleteUserAuthentication();
        }
    }
}