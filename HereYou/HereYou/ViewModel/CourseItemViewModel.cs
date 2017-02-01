using System.Collections.ObjectModel;
using HereYou.Model;
namespace HereYou.ViewModel {
  class CourseItemViewModel {
        private ObservableCollection<CourseItem> allItems = new ObservableCollection<Model.CourseItem>();
        public ObservableCollection<CourseItem> AllItems { get { return this.allItems; } }

        public ObservableCollection<CourseItem> viewItems = new ObservableCollection<CourseItem>();

        private Model.CourseItem selectedItem = default(CourseItem);
        public CourseItem SelectedItem { get { return selectedItem; } set { this.selectedItem = value; } }

        public void AddCourseItem(string id, string courseName, string score, string teacher) {
            var newItem = new CourseItem(id, courseName, score, teacher);
            this.allItems.Add(newItem);
            this.selectedItem = null;
        }

        public void AddCourseItem(CourseItem newItem) {
            this.allItems.Add(newItem);
            this.selectedItem = null;
        }

        public CourseItem FindCourseByID(string id) {
            foreach (var item in allItems) {
                if (item.id.Equals(id))
                    return item;
            }
            return null;
        }

        public void ClearAllItems() {
            allItems.Clear();
        }
    }
}
