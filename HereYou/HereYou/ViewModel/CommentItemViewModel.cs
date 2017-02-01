using System.Collections.ObjectModel;

namespace HereYou.ViewModel {
  class CommentItemViewModel {
        private ObservableCollection<Model.CommentItem> allItems = new ObservableCollection<Model.CommentItem>();
        public ObservableCollection<Model.CommentItem> AllItems { get { return this.allItems; } }

        public void AddCommentItem(string username, string avatar, string body, string time, string score) {
            var newItem = new Model.CommentItem(username, avatar, body, time, score);
            this.allItems.Add(newItem);
        }

        public void ClearAllItems() {
            allItems.Clear();
        }
    }
}
