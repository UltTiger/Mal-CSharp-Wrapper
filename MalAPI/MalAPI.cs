using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace MalAPI
{
    public struct Entry
    {
        string id, title, english, synonyms, episodes, score, type, status, start_date, end_date, synopsis, image;
    }
    public struct ProgressEntry
    {
        //Duplicates of the main listing with some data missing
        //Stored since there appears to be no way to search the database with id
        //unused: english, score, synopsis
        string id, title, english, synonyms, episodes, score, type, status, start_date, end_date, synopsis, image, chapters, volumes;

        //Main data
        string my_id, my_watched_episodes, my_start_date, my_finish_date, my_score, my_status, my_rewatching, my_rewatching_ep, my_last_updated, my_tags, my_read_chapters, my_read_volumes, my_rereading, my_rereading_chap;
    }
    public struct UserInfo
    {
        string userid, username;
        string user_watching_anime, user_completed_anime, user_onhold_anime, user_dropped_anime, user_plantowatch_anime;
        string user_reading_manga, user_completed_manga, user_onhold_manga, user_dropped_manga, user_plantoread_manga;

        string user_days_spent_watching;
        string user_days_spent_reading;
    }


    public class MalAPI
    {
        public string GetWebDataRaw( string url )
        {
            WebClient client = new WebClient();
            client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");

            Stream data = client.OpenRead( url );
            StreamReader reader = new StreamReader(data);
            string result = reader.ReadToEnd();
            data.Close();
            reader.Close();

            return result;
        }


        private string user, pass;

        private List<Entry> SearchGeneric(string query, string type)
        {
            Console.WriteLine("Not implemented");
            List<Entry> e = new List<Entry>();
            return e;
        }
        private List<ProgressEntry> GetUserListGeneric(string type)
        {
            Console.WriteLine("Not implemented");
            List<ProgressEntry> e = new List<ProgressEntry>();
            return e;
        }


        public void SetCredentials(string user, string pass)
        {
            this.user = user;
            this.pass = pass;
        }

        public bool ValidateUser()
        {
            Console.WriteLine("Not implemented");
            return false;
        }
        public UserInfo GetUserInfo()
        {
            Console.WriteLine("Not implemented");
            UserInfo info = new UserInfo();

            return info;
        }

        public List<Entry> SearchAnime(string query)
        {
            return SearchGeneric(query, "anime");
        }

        public List<Entry> SearchManga(string query)
        {
            return SearchGeneric(query, "manga");
        }

        public List<ProgressEntry> GetUserListAnime()
        {
            return GetUserListGeneric("anime");
        }

        public List<ProgressEntry> GetUserListManga()
        {
            return GetUserListGeneric("manga");
        }

        void AddAnime() { }
        void UpdateAnime() { }
        void DeleteAnime() { }

        void AddManga() { }
        void UpdateManga() { }
        void DeleteManga() { }

        public override string ToString()
        {
            return "MyAnimeList (unofficial) Web API";
        }
    }
}
