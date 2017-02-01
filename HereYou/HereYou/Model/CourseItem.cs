//公选课程
namespace HereYou.Model {
  class CourseItem {
        public string id;
        public string courseName;
        public string score;
        public string teacher;

        public CourseItem(string id, string courseName, string score, string teacher) {
            this.id = id;
            this.score = score;
            this.courseName = courseName;
            this.teacher = teacher;
        }
    }
}
