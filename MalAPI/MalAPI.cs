using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Specialized;

namespace MalAPI
{
    public struct Entry
    {
        public string id, title, english, synonyms, episodes, chapters, volumes, score, type, status, start_date, end_date, synopsis, image;
    }
    public struct ProgressEntry
    {
        //Duplicates of the main listing with some data missing
        //Stored since there appears to be no way to search the database with id
        //unused: english, score, synopsis
        public string id, title, english, synonyms, episodes, score, type, status, start_date, end_date, synopsis, image, chapters, volumes;

        //Main data
        public string my_id, my_watched_episodes, my_start_date, my_finish_date, my_score, my_status, my_rewatching, my_rewatching_ep, my_last_updated, my_tags, my_read_chapters, my_read_volumes, my_rereading, my_rereading_chap;
    }
    public struct UserInfo
    {
        public string userid, username;
        public string user_watching_anime, user_completed_anime, user_onhold_anime, user_dropped_anime, user_plantowatch_anime;
        public string user_reading_manga, user_completed_manga, user_onhold_manga, user_dropped_manga, user_plantoread_manga;

        public string user_days_spent_watching;
        public string user_days_spent_reading;
    }
    public struct AnimeValues
    {
        public int id;

        public int episode, status, score;
        public string storage_type, storage_value;
        public int times_rewatched, rewatch_value;
        public string date_start, date_finish;
        public int priority;
        public string enable_discussion, enable_rewatching;
        public string comments, tags;
    }

    public class MalAPI
    {
        //Vars
        private string user, pass;

        //Private
        private string GetWebData( string url, NameValueCollection postData = null )
        {
            try
            {
                WebClient client = new WebClient();
                client.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                client.Headers.Add("Accept-Language", " en-US");
                client.Headers.Add("Accept", " text/html, application/xhtml+xml, */*");

                client.UseDefaultCredentials = true;
                client.Credentials = new NetworkCredential(user, pass);

                string result;
                if (postData == null)
                {
                    Stream data = client.OpenRead(url);
                    StreamReader reader = new StreamReader(data);
                    result = reader.ReadToEnd();
                    data.Close();
                    reader.Close();
                }
                else
                {
                    result = Encoding.UTF8.GetString(client.UploadValues(url, "POST", postData));
                }

                return result;
            }
            catch (System.Net.WebException e)
            {
                Console.WriteLine("Network exception ("+url+"): " + e);
            }
            return null;
        }
        private XmlDocument GetDocument(string url, NameValueCollection postData = null)
        {
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(GetWebData(url, postData));

                return xml;
            }
            catch( XmlException e)
            {
                Console.WriteLine("XML exception ("+url+"): " + e);
            }
            return null;
        }

        private List<Entry> SearchGeneric(string query, string type)
        {
            XmlDocument xml = GetDocument("https://myanimelist.net/api/" + type + "/search.xml?q=" + query);
            if (xml != null)
            {
                List<Entry> results = new List<Entry>();

                XmlNodeList nodes = xml.SelectNodes(type + "/entry");
                for (int i=0; i<nodes.Count; i++)
                {
                    Entry ent = new Entry();
                    ent.id = nodes[i].SelectSingleNode("id").InnerText;
                    ent.title = nodes[i].SelectSingleNode("title").InnerText;
                    ent.english = nodes[i].SelectSingleNode("english").InnerText;
                    ent.synonyms = nodes[i].SelectSingleNode("synonyms").InnerText;

                    if (type == "manga")
                    {
                        ent.chapters = nodes[i].SelectSingleNode("chapters").InnerText;
                        ent.volumes = nodes[i].SelectSingleNode("volumes").InnerText;
                    }
                    else
                    {
                        ent.episodes = nodes[i].SelectSingleNode("episodes").InnerText;
                    }

                    ent.score = nodes[i].SelectSingleNode("score").InnerText;
                    ent.type = nodes[i].SelectSingleNode("type").InnerText;
                    ent.status = nodes[i].SelectSingleNode("status").InnerText;
                    ent.start_date = nodes[i].SelectSingleNode("start_date").InnerText;
                    ent.end_date = nodes[i].SelectSingleNode("end_date").InnerText;
                    ent.synopsis = nodes[i].SelectSingleNode("synopsis").InnerText;
                    ent.image = nodes[i].SelectSingleNode("image").InnerText;

                    results.Add(ent);
                }

                return results;
            }

            return null;

        }
        private List<ProgressEntry> GetUserListGeneric(string type)
        {
            XmlDocument xml = GetDocument("https://myanimelist.net/malappinfo.php?u=" + this.user + "&status=all&type=" + type);
            if (xml != null)
            {
                List<ProgressEntry> results = new List<ProgressEntry>();

                XmlNodeList nodes = xml.SelectNodes("/myanimelist/" + type);
                for (int i = 0; i < nodes.Count; i++)
                {
                    ProgressEntry entry = new ProgressEntry();

                    //Global data
                    if (type == "manga")
                    {
                        entry.id = nodes[i].SelectSingleNode("series_mangadb_id").InnerText;
                    }
                    else
                    {
                        entry.id = nodes[i].SelectSingleNode("series_animedb_id").InnerText;
                    }
                    entry.title = nodes[i].SelectSingleNode("series_title").InnerText;
                    entry.synonyms = nodes[i].SelectSingleNode("series_synonyms").InnerText;
                    entry.type = nodes[i].SelectSingleNode("series_type").InnerText;

                    if (type == "manga")
                    {
                        entry.chapters = nodes[i].SelectSingleNode("series_chapters").InnerText;
                        entry.volumes = nodes[i].SelectSingleNode("series_volumes").InnerText;
                    }
                    else
                    {
                        entry.episodes = nodes[i].SelectSingleNode("series_episodes").InnerText;
                    }

                    entry.status = nodes[i].SelectSingleNode("series_status").InnerText;
                    entry.start_date = nodes[i].SelectSingleNode("series_start").InnerText;
                    entry.end_date = nodes[i].SelectSingleNode("series_end").InnerText;
                    entry.image = nodes[i].SelectSingleNode("series_image").InnerText;

                    //User data
                    entry.my_id = nodes[i].SelectSingleNode("my_id").InnerText;
                    if (type == "manga")
                    {
                        entry.my_read_chapters = nodes[i].SelectSingleNode("my_read_chapters").InnerText;
                        entry.my_read_volumes = nodes[i].SelectSingleNode("my_read_volumes").InnerText;
                    }
                    else
                    {
                        entry.my_watched_episodes = nodes[i].SelectSingleNode("my_watched_episodes").InnerText;
                    }
                    entry.my_start_date = nodes[i].SelectSingleNode("my_start_date").InnerText;
                    entry.my_finish_date = nodes[i].SelectSingleNode("my_finish_date").InnerText;
                    entry.my_score = nodes[i].SelectSingleNode("my_score").InnerText;
                    entry.my_status = nodes[i].SelectSingleNode("my_status").InnerText;
                    if (type == "manga")
                    {
                        entry.my_rereading = nodes[i].SelectSingleNode("my_rereadingg").InnerText;
                        entry.my_rereading_chap = nodes[i].SelectSingleNode("my_rereading_chap").InnerText;
                    }
                    else
                    {
                        entry.my_rewatching = nodes[i].SelectSingleNode("my_rewatching").InnerText;
                        entry.my_rewatching_ep = nodes[i].SelectSingleNode("my_rewatching_ep").InnerText;
                    }
                    entry.my_last_updated = nodes[i].SelectSingleNode("my_last_updated").InnerText;
                    entry.my_tags = nodes[i].SelectSingleNode("my_tags").InnerText;

                    results.Add(entry);
                }

                return results;
            }
            return null;
        }

        private XmlDocument EncodeAnimeUpdateEntry( AnimeValues entry )
        {
            XmlDocument xml = new XmlDocument();

            XmlNode master = xml.CreateElement("entry");
            xml.AppendChild(master);

            XmlNode elm = xml.CreateElement("episode");
            elm.InnerText = entry.episode.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("status");
            elm.InnerText = entry.status.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("score");
            elm.InnerText = entry.score.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("storage_type");
            elm.InnerText = entry.storage_type;
            master.AppendChild(elm);

            elm = xml.CreateElement("storage_value");
            elm.InnerText = entry.storage_value;
            master.AppendChild(elm);

            elm = xml.CreateElement("times_rewatched");
            elm.InnerText = entry.times_rewatched.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("rewatch_value");
            elm.InnerText = entry.rewatch_value.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("date_start");
            elm.InnerText = entry.date_start;
            master.AppendChild(elm);

            elm = xml.CreateElement("date_finish");
            elm.InnerText = entry.date_finish;
            master.AppendChild(elm);

            elm = xml.CreateElement("priority");
            elm.InnerText = entry.priority.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("enable_discussion");
            elm.InnerText = entry.enable_discussion;
            master.AppendChild(elm);

            elm = xml.CreateElement("enable_rewatching");
            elm.InnerText = entry.enable_rewatching;
            master.AppendChild(elm);

            elm = xml.CreateElement("comments");
            elm.InnerText = entry.comments;
            master.AppendChild(elm);

            elm = xml.CreateElement("tags");
            elm.InnerText = entry.tags;
            master.AppendChild(elm);

            return xml;
        }

        //Public
        public void SetCredentials(string user, string pass)
        {
            this.user = user;
            this.pass = pass;
        }

        public bool ValidateUser()
        {
            XmlDocument xml = GetDocument("https://myanimelist.net/api/account/verify_credentials.xml");

            if (xml != null)
            {
                string userstr = xml.SelectSingleNode("/user/username").InnerText;

                if (userstr == user)
                {
                    return true;
                }
            }

            return false;
        }
        public UserInfo GetUserInfo()
        {
            UserInfo info = new UserInfo();
            XmlDocument xml = GetDocument("https://myanimelist.net/malappinfo.php?u=" + this.user + "&status=all&type=anime");
            if (xml != null)
            {
                info.userid = xml.SelectSingleNode("/myanimelist/myinfo/user_id").InnerText;
                info.username = xml.SelectSingleNode("/myanimelist/myinfo/user_name").InnerText;

                info.user_watching_anime = xml.SelectSingleNode("/myanimelist/myinfo/user_watching").InnerText;
                info.user_completed_anime = xml.SelectSingleNode("/myanimelist/myinfo/user_completed").InnerText;
                info.user_onhold_anime = xml.SelectSingleNode("/myanimelist/myinfo/user_onhold").InnerText;
                info.user_dropped_anime = xml.SelectSingleNode("/myanimelist/myinfo/user_dropped").InnerText;
                info.user_plantowatch_anime = xml.SelectSingleNode("/myanimelist/myinfo/user_plantowatch").InnerText;
                info.user_days_spent_watching = xml.SelectSingleNode("/myanimelist/myinfo/user_days_spent_watching").InnerText;
            }
            xml = GetDocument("https://myanimelist.net/malappinfo.php?u=" + this.user + "&status=all&type=manga");
            if (xml != null)
            {
                info.user_reading_manga = xml.SelectSingleNode("/myanimelist/myinfo/user_reading").InnerText;
                info.user_completed_manga = xml.SelectSingleNode("/myanimelist/myinfo/user_completed").InnerText;
                info.user_onhold_manga = xml.SelectSingleNode("/myanimelist/myinfo/user_onhold").InnerText;
                info.user_dropped_manga = xml.SelectSingleNode("/myanimelist/myinfo/user_dropped").InnerText;
                info.user_plantoread_manga = xml.SelectSingleNode("/myanimelist/myinfo/user_plantoread").InnerText;
                info.user_days_spent_reading = xml.SelectSingleNode("/myanimelist/myinfo/user_days_spent_watching").InnerText;
            }

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

        public bool AddAnime( AnimeValues entry )
        {

            /*StringWriter sw = new StringWriter();
            XmlTextWriter tx = new XmlTextWriter(sw);
            EncodeAnimeUpdateEntry(entry).WriteTo(tx);

            string encoderes = sw.ToString();*/

            //Console.WriteLine( "Anime Entry: " + encoderes + " | " + EncodeAnimeUpdateEntry(entry) );
            //return false;

            //Console.WriteLine( GetWebData("https://myanimelist.net/api/animelist/add/32995.xml") );
            //return false;

            NameValueCollection post = new NameValueCollection();

            post.Add("id", entry.id.ToString() );
            post.Add("data", EncodeAnimeUpdateEntry(entry).OuterXml );

            

            //post.Add("data", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<entry>\n<episode>1</episode>\n<status>1</status>\n</entry>");

            string str = GetWebData("https://myanimelist.net/api/animelist/add/" + entry.id.ToString() + ".xml", post);
            return str == "Created";
        }
        void UpdateAnime() { }
        void DeleteAnime() { }

        void AddManga() { }
        void UpdateManga() { }
        void DeleteManga() { }

        //Meta
        public override string ToString()
        {
            return "MyAnimeList (unofficial) Web API";
        }
    }
}
