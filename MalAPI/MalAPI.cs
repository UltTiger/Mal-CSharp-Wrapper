using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Specialized;

namespace MalAPI
{
    public enum ItemType
    {
        Anime,
        Manga
    }
    public enum ItemStatus
    {
        WatchingOrReading,
        Complete,
        OnHold,
        Dropped,
        PlanToWatchOrRead
    }
    public enum ItemScore
    {
        Zero,
        Appalling,
        Horrible,
        VeryBad,
        Bad,
        Average,
        Fine,
        Good,
        VeryGood,
        Great,
        Masterpiece
    }

    public struct Entry
    {
        public int id, episodes, chapters, volumes;
        public float score;
        public string title, type, english, synonyms, status, start_date, end_date, synopsis, image;
    }
    public struct ProgressEntry
    {
        //Duplicates of the main listing with some data missing
        //Stored since there appears to be no way to search the database with id
        //unused: english, score, synopsis
        public int id, type, episodes, status;
        public string title, english, synonyms, start_date, end_date, synopsis, image, chapters, volumes;

        //Main data
        public int my_id, my_watched_episodes, my_score, my_status, my_rewatching_ep, my_read_chapters, my_read_volumes, my_rereading_chap;
        public string my_start_date, my_finish_date, my_rewatching, my_last_updated, my_tags, my_rereading;
    }
    public struct UserInfo
    {
        public int userid;
        public string username;
        public int user_watching_anime, user_completed_anime, user_onhold_anime, user_dropped_anime, user_plantowatch_anime;
        public int user_reading_manga, user_completed_manga, user_onhold_manga, user_dropped_manga, user_plantoread_manga;

        public float user_days_spent_watching;
        public float user_days_spent_reading;
    }
    public struct AnimeValues
    {
        public int id;

        public int episode, status, score;
        public string storage_type, storage_value;
        public int times_rewatched, rewatch_value;
        public string date_start, date_finish;
        public int priority;
        public bool enable_discussion, enable_rewatching;
        public string comments, tags;
    }
    public struct MangaValues
    {
        public int id;

        public int chapter, volume, status, score, times_reread, reread_value;
        public string date_start, date_finish;
        public int priority;
        public bool enable_discussion, enable_rereading;
        public string comments, scan_group, tags;
        public int retail_volumes;
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
                    ent.id = int.Parse( nodes[i].SelectSingleNode("id").InnerText );
                    ent.title = nodes[i].SelectSingleNode("title").InnerText;
                    ent.english = nodes[i].SelectSingleNode("english").InnerText;
                    ent.synonyms = nodes[i].SelectSingleNode("synonyms").InnerText;

                    if (type == "manga")
                    {
                        ent.chapters = int.Parse(nodes[i].SelectSingleNode("chapters").InnerText);
                        ent.volumes = int.Parse(nodes[i].SelectSingleNode("volumes").InnerText);
                    }
                    else
                    {
                        ent.episodes = int.Parse(nodes[i].SelectSingleNode("episodes").InnerText);
                    }

                    ent.score = float.Parse(nodes[i].SelectSingleNode("score").InnerText);
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
                        entry.id = int.Parse(nodes[i].SelectSingleNode("series_mangadb_id").InnerText);
                    }
                    else
                    {
                        entry.id = int.Parse(nodes[i].SelectSingleNode("series_animedb_id").InnerText);
                    }
                    entry.title = nodes[i].SelectSingleNode("series_title").InnerText;
                    entry.synonyms = nodes[i].SelectSingleNode("series_synonyms").InnerText;
                    entry.type = int.Parse(nodes[i].SelectSingleNode("series_type").InnerText);

                    if (type == "manga")
                    {
                        entry.chapters = nodes[i].SelectSingleNode("series_chapters").InnerText;
                        entry.volumes = nodes[i].SelectSingleNode("series_volumes").InnerText;
                    }
                    else
                    {
                        entry.episodes = int.Parse(nodes[i].SelectSingleNode("series_episodes").InnerText);
                    }

                    entry.status = int.Parse(nodes[i].SelectSingleNode("series_status").InnerText);
                    entry.start_date = nodes[i].SelectSingleNode("series_start").InnerText;
                    entry.end_date = nodes[i].SelectSingleNode("series_end").InnerText;
                    entry.image = nodes[i].SelectSingleNode("series_image").InnerText;

                    //User data
                    entry.my_id = int.Parse(nodes[i].SelectSingleNode("my_id").InnerText);
                    if (type == "manga")
                    {
                        entry.my_read_chapters = int.Parse(nodes[i].SelectSingleNode("my_read_chapters").InnerText);
                        entry.my_read_volumes = int.Parse(nodes[i].SelectSingleNode("my_read_volumes").InnerText);
                    }
                    else
                    {
                        entry.my_watched_episodes = int.Parse(nodes[i].SelectSingleNode("my_watched_episodes").InnerText);
                    }
                    entry.my_start_date = nodes[i].SelectSingleNode("my_start_date").InnerText;
                    entry.my_finish_date = nodes[i].SelectSingleNode("my_finish_date").InnerText;
                    entry.my_score = int.Parse(nodes[i].SelectSingleNode("my_score").InnerText);
                    entry.my_status = int.Parse(nodes[i].SelectSingleNode("my_status").InnerText);
                    if (type == "manga")
                    {
                        entry.my_rereading = nodes[i].SelectSingleNode("my_rereadingg").InnerText; //The original XML has this spelling mistake
                        entry.my_rereading_chap = int.Parse(nodes[i].SelectSingleNode("my_rereading_chap").InnerText);
                    }
                    else
                    {
                        entry.my_rewatching = nodes[i].SelectSingleNode("my_rewatching").InnerText;
                        entry.my_rewatching_ep = int.Parse(nodes[i].SelectSingleNode("my_rewatching_ep").InnerText);
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
            elm.InnerText = entry.enable_discussion.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("enable_rewatching");
            elm.InnerText = entry.enable_rewatching.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("comments");
            elm.InnerText = entry.comments;
            master.AppendChild(elm);

            elm = xml.CreateElement("tags");
            elm.InnerText = entry.tags;
            master.AppendChild(elm);

            return xml;
        }
        private XmlDocument EncodeMangaUpdateEntry(MangaValues entry)
        {
            XmlDocument xml = new XmlDocument();

            XmlNode master = xml.CreateElement("entry");
            xml.AppendChild(master);

            XmlNode elm = xml.CreateElement("chapter");
            elm.InnerText = entry.chapter.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("volume");
            elm.InnerText = entry.volume.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("status");
            elm.InnerText = entry.status.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("score");
            elm.InnerText = entry.score.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("times_reread");
            elm.InnerText = entry.times_reread.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("reread_value");
            elm.InnerText = entry.reread_value.ToString();
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
            elm.InnerText = entry.enable_discussion.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("enable_rereading");
            elm.InnerText = entry.enable_rereading.ToString();
            master.AppendChild(elm);

            elm = xml.CreateElement("comments");
            elm.InnerText = entry.comments;
            master.AppendChild(elm);

            elm = xml.CreateElement("scan_group");
            elm.InnerText = entry.scan_group;
            master.AppendChild(elm);

            elm = xml.CreateElement("retail_volumes");
            elm.InnerText = entry.retail_volumes.ToString();
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
                info.userid = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_id").InnerText);
                info.username = xml.SelectSingleNode("/myanimelist/myinfo/user_name").InnerText;

                info.user_watching_anime = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_watching").InnerText);
                info.user_completed_anime = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_completed").InnerText);
                info.user_onhold_anime = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_onhold").InnerText);
                info.user_dropped_anime = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_dropped").InnerText);
                info.user_plantowatch_anime = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_plantowatch").InnerText);
                info.user_days_spent_watching = float.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_days_spent_watching").InnerText);
            }
            xml = GetDocument("https://myanimelist.net/malappinfo.php?u=" + this.user + "&status=all&type=manga");
            if (xml != null)
            {
                info.user_reading_manga = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_reading").InnerText);
                info.user_completed_manga = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_completed").InnerText);
                info.user_onhold_manga = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_onhold").InnerText);
                info.user_dropped_manga = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_dropped").InnerText);
                info.user_plantoread_manga = int.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_plantoread").InnerText);
                info.user_days_spent_reading = float.Parse(xml.SelectSingleNode("/myanimelist/myinfo/user_days_spent_watching").InnerText);
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
            NameValueCollection post = new NameValueCollection();

            post.Add("id", entry.id.ToString() );
            post.Add("data", EncodeAnimeUpdateEntry(entry).OuterXml );

            string str = GetWebData("https://myanimelist.net/api/animelist/add/" + entry.id.ToString() + ".xml", post);
            return str == "Created";
        }
        public bool UpdateAnime(AnimeValues entry)
        {
            NameValueCollection post = new NameValueCollection();

            post.Add("id", entry.id.ToString());
            post.Add("data", EncodeAnimeUpdateEntry(entry).OuterXml);

            string str = GetWebData("https://myanimelist.net/api/animelist/update/" + entry.id.ToString() + ".xml", post);
            return str == "Updated";
        }
        public bool DeleteAnime( int id )
        {
            NameValueCollection post = new NameValueCollection();

            post.Add("id", id.ToString());

            string str = GetWebData("https://myanimelist.net/api/animelist/delete/" + id.ToString() + ".xml", post);
            return str == "Deleted";
        }

        public bool AddManga( MangaValues entry )
        {
            NameValueCollection post = new NameValueCollection();

            post.Add("id", entry.id.ToString());
            post.Add("data", EncodeMangaUpdateEntry(entry).OuterXml);

            string str = GetWebData("https://myanimelist.net/api/mangalist/add/" + entry.id.ToString() + ".xml", post);
            return str == "Created";
        }
        public bool UpdateManga( MangaValues entry )
        {
            NameValueCollection post = new NameValueCollection();

            post.Add("id", entry.id.ToString());
            post.Add("data", EncodeMangaUpdateEntry(entry).OuterXml);

            string str = GetWebData("https://myanimelist.net/api/mangalist/update/" + entry.id.ToString() + ".xml", post);
            return str == "Updated";
        }
        public bool DeleteManga( int id )
        {
            NameValueCollection post = new NameValueCollection();

            post.Add("id", id.ToString());

            string str = GetWebData("https://myanimelist.net/api/mangalist/delete/" + id.ToString() + ".xml", post);
            return str == "Deleted";
        }

        //Meta
        public override string ToString()
        {
            return "MyAnimeList (unofficial) REST API C# wrapper";
        }
    }
}
