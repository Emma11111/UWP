using System;
using System.Collections.ObjectModel;
using SQLitePCL;
using Windows.UI.Notifications;

namespace HereYou {
  class TodoItemViewModel {
    private ObservableCollection<TodoItem> items = new ObservableCollection<TodoItem>();
    public ObservableCollection<TodoItem> getItems { get { return this.items; } }

    private TodoItem selectedItem;
    public TodoItem SelectedItem {
      get { return selectedItem; }
      set { selectedItem = value; }
    }

    public TodoItemViewModel() {
      try {
        var sql = "SELECT * FROM Todo";
        var conn = App.conn;
        int totalNum = 0;
        using (var statement = conn.Prepare(sql)) {
          while (SQLiteResult.ROW == statement.Step()) {
            var s = statement[1].ToString();
            s = s.Substring(0, s.IndexOf(' '));
            long ID = (long)statement[0];
            DateTime date = new DateTime(int.Parse(s.Split('/')[0]), int.Parse(s.Split('/')[1]), int.Parse(s.Split('/')[2]));
            string imgname = (string)statement[2];
            string title = (string)statement[3];
            string detail = (string)statement[4];
            bool finish = Boolean.Parse(statement[5] as string);
            if (finish != true) {
              ++totalNum;
            }
            this.AddItem(ID, date, imgname, title, detail, finish);
          }
          if (totalNum > 0) {
            var template = ToastTemplateType.ToastText01;
            var xml = ToastNotificationManager.GetTemplateContent(template);
            xml.DocumentElement.SetAttribute("launch", "Args");

            string content = "您还有" + totalNum.ToString() + "个待办事项未完成";

            var text = xml.CreateTextNode(content);
            var elements = xml.GetElementsByTagName("text");
            elements[0].AppendChild(text);

            // show toast
            var toast = new ToastNotification(xml);
            ToastNotificationManager.CreateToastNotifier().Show(toast);
          }
        }
      } catch {
      }
    }

    public void AddItem(long ID, DateTime date, string imgname, string title, string detail, bool? finish) {
      items.Insert(0, new TodoItem(ID, date, imgname, title, detail, finish));
    }
    public void UpdateItem(DateTime date, string imgname, string title, string detail) {
      selectedItem.date = date;
      selectedItem.title = title;
      selectedItem.detail = detail;
      if (selectedItem.imgname != imgname) {
        selectedItem.imgname = imgname;
        selectedItem.setImg();
      }
    }
    public void RemoveItem() {
      items.Remove(selectedItem);
      SelectedItem = null;
    }
  }
}
