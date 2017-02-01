//课程分数详情
namespace HereYou.Model {
  public class ScoreItem {
    public string kcmc { get; set; }
    public string kclb { get; set; }
    public string xf { get; set; }
    public string zzcj { get; set; }
    public string jd { get; set; }
    public string jxbpm { get; set; }
    public string jsxm { get; set; }
    public ScoreItem(string kcmc, string kclb, string jsxm, string xf, string zzcj, string jd, string jxbpm) {
      this.kcmc = kcmc;
      this.xf = xf;
      this.zzcj = zzcj;
      this.jd = jd;
      this.jxbpm = jxbpm;
      this.jsxm = jsxm != ""? jsxm : "";
      switch (kclb) {
        case "10":
          this.kclb = "公必";
          break;
        case "11":
          this.kclb = "专必";
          break;
        case "21":
          this.kclb = "专选";
          break;
        case "30":
          this.kclb = "公选";
          break;
      }
    }
  }
}
