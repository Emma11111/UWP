using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;

namespace Todos.ViewModels {
  class TodoItemViewModel {
    private ObservableCollection<Models.TodoItem> items = new ObservableCollection<Models.TodoItem>();

    private Models.TodoItem selectedItem;
    public Models.TodoItem SelectedItem {
      get { return selectedItem; }
      set { this.selectedItem = value; }
    }
    public TodoItemViewModel() {
      this.SelectedItem = null;
      this.items.Add(new Models.TodoItem(DateTime.Today, null, "pre", "English presentation", true));
      this.items.Add(new Models.TodoItem(DateTime.Today, null, "python", "python homework"));
    }

    public ObservableCollection<Models.TodoItem> getItems { get { return this.items; } }

    public void AddItem(DateTime date, ImageSource img, string title, string detail) {
      items.Add(new Models.TodoItem(date, img, title, detail));
    }
  }
}
