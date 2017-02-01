using System;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using Newtonsoft.Json;

namespace Todos {
   class TodoItemViewModel {
    private ObservableCollection<TodoItem> items = new ObservableCollection<TodoItem>();
    public ObservableCollection<TodoItem> getItems { get { return this.items; } }

    private TodoItem selectedItem;
    public TodoItem SelectedItem {
      get { return selectedItem; }
      set { selectedItem = value; }
    }

    public TodoItemViewModel() {
      items.Insert(0, new TodoItem(DateTime.Today, "", "pre", "English presentation", true));
      items.Insert(0, new TodoItem(DateTime.Today, "", "python", "python homework"));
    }

    public void AddItem(DateTime date, string imgname, string title, string detail) {
      items.Insert(0, new TodoItem(date, imgname, title, detail));
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

    //#region Methods for handling the apps permanent data
    //public void LoadData() {
    //  if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("Item")) {
    //    selectedItem = JsonConvert.DeserializeObject<TodoItem>(
    //        (string)ApplicationData.Current.RoamingSettings.Values["Item"]);
    //  } else {
    //    // New start, initialize the data
    //    selectedItem = null;
    //  }
    //  if (ApplicationData.Current.RoamingSettings.Values.ContainsKey("ViewModel")) {
    //    Common.ViewModel = JsonConvert.DeserializeObject<TodoItemViewModel>(
    //        (string)ApplicationData.Current.RoamingSettings.Values["ViewModel"]);
    //  } else {
    //    // New start, initialize the data
    //  }
    //}

    public void SaveData() {
      ApplicationData.Current.RoamingSettings.Values["Item"] = JsonConvert.SerializeObject(selectedItem);
    }
      //  ApplicationData.Current.RoamingSettings.Values["ViewModel"] = JsonConvert.SerializeObject(this);
      //}
      //#endregion
    }
}
