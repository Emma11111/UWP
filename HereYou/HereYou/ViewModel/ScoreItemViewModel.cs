using System.Collections.ObjectModel;
using HereYou.Model;

namespace HereYou.ViewModel {
  class ScoreItemViewModel : ObservableCollection<ScoreItem> {
    public ObservableCollection<ScoreItem> items = new ObservableCollection<ScoreItem>();
    public ScoreItemViewModel() {}
  }
}
