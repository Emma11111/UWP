//公选评论
namespace HereYou.Model {
  class CommentItem {
        public string username;
        public string avatar;
        public string body;
        public string time;
        public string score;

        public CommentItem(string username, string avatar, string body, string time, string score) {
            this.username = username;
            this.score = score;
            this.avatar = avatar;
            this.body = body;
            this.time = time;
        }
    }
}
