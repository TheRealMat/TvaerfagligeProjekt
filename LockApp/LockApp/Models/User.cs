namespace LockApp.Models
{
    public class User
    {
        public User(string name, string pass, string apiIp)
        {
            this.name = name;
            this.pass = pass;
            this.apiIp = apiIp;
        }
        public string name;
        public string pass;
        public string apiIp;
    }
}
