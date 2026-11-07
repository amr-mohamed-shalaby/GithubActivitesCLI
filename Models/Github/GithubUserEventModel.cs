using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GithubActivity.Models.Github
{
    public class Actor
    {
        public int id { get; set; }
        public string login { get; set; }
        public string display_login { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string avatar_url { get; set; }
    }

    public class Org
    {
        public int id { get; set; }
        public string login { get; set; }
        public string gravatar_id { get; set; }
        public string url { get; set; }
        public string avatar_url { get; set; }
    }

    public class Payload
    {
        public string action { get; set; }
        public string @ref { get; set; }
        public string ref_type { get; set; }
        public string full_ref { get; set; }
        public string master_branch { get; set; }
        public object description { get; set; }
        public string pusher_type { get; set; }
    }

    public class Repo
    {
        public int id { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class GithubUserEventModel
    {
        public string id { get; set; }
        public string type { get; set; }
        public Actor actor { get; set; }
        public Repo repo { get; set; }
        public Payload payload { get; set; }
        public bool @public { get; set; }
        public DateTime created_at { get; set; }
        public Org org { get; set; }
    }
}