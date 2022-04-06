using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SQLite;
using Newtonsoft.Json;

namespace LockAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiController : ControllerBase
    {
        SQLiteConnection sqliteConn;
        string apiId = "testapi";
        public ApiController()
        {
            sqliteConn = new SQLiteConnection("Data Source=LockDB.db; Version = 3; ");
            sqliteConn.Open();
        }



        //get request from app, log request in database, send request to lock, log response in database
       [HttpPost(Name = "OpenLock")]
        public async Task<bool> GetAsync(string userName, string userPass, string lockId)
        {
            LogEvent(userName, $"someone tried to open lock with ID {lockId}");
            if (!VerifyUser(userName, userPass))
            {
                return false;
            }
            HttpClient client = new HttpClient();

            string lockAddress = GetAddress(lockId);

            // send request to controller, wait for response
            string responseBody = await client.GetStringAsync($"http://{lockAddress}/{lockId}/{apiId}");

            // maybe return lock state or similar
            return true;
        }


        // send list of locks user has access to
        [HttpGet(Name = "GetLocks")]
        public string Get(string userName, string userPass)
        {
            LogEvent(userName, "GetLocks request");

            if (!VerifyUser(userName, userPass))
            {
                return "";
            }
            
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqliteConn.CreateCommand();

            sqlite_cmd.CommandText = $"SELECT LockId FROM UserLocks WHERE UserId = @UserId";

            sqlite_cmd.Parameters.Add("UserId", DbType.String);
            sqlite_cmd.Parameters["UserId"].Value = userName;

            SQLiteDataReader result = sqlite_cmd.ExecuteReader();

            // convert response to json
            DataTable dt = new DataTable();
            dt.Load(result);
            string json = JsonConvert.SerializeObject(dt);

            return json;
        }

        private string GetAddress(string lockId)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqliteConn.CreateCommand();

            sqlite_cmd.CommandText = $"SELECT Address FROM Locks WHERE Id = @LockId";

            sqlite_cmd.Parameters.Add("LockId", DbType.String);
            sqlite_cmd.Parameters["LockId"].Value = lockId;

            SQLiteDataReader result = sqlite_cmd.ExecuteReader();
            DataTable dt = new DataTable();
            dt.Load(result);

            return dt.Rows[0][0].ToString();
        }

        private bool VerifyUser(string userName, string userPass)
        {
            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqliteConn.CreateCommand();

            sqlite_cmd.CommandText = $"SELECT Id FROM Users WHERE Id = @UserId AND Hash = @userPass" ;

            sqlite_cmd.Parameters.Add("UserId", DbType.String);
            sqlite_cmd.Parameters["UserId"].Value = userName;

            sqlite_cmd.Parameters.Add("userPass", DbType.String);
            sqlite_cmd.Parameters["userPass"].Value = userPass;

            SQLiteDataReader result = sqlite_cmd.ExecuteReader();

            if (result.HasRows)
            {
                return true;
            }

            return false;
        }


        private void LogEvent(string userName, string eventDescription)
        {
            // get current time for log
            DateTime dateTime = DateTime.Now;

            SQLiteCommand sqlite_cmd;
            sqlite_cmd = sqliteConn.CreateCommand();

            sqlite_cmd.CommandText = $"INSERT INTO Log (LogTime, LogEvent, LogUserId) VALUES('{dateTime}',@logEvent,@logUserId); ";

            sqlite_cmd.Parameters.Add("logUserId", DbType.String);
            sqlite_cmd.Parameters["logUserId"].Value = userName;

            sqlite_cmd.Parameters.Add("logEvent", DbType.String);
            sqlite_cmd.Parameters["logEvent"].Value = eventDescription;


            sqlite_cmd.ExecuteNonQuery();
        }





    }
}