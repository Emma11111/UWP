using System;
using System.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Hw1 {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class MainPage : Page {
        private delegate void AnimalSaying(object sender);//声明一个委托
        private event AnimalSaying Say;//委托声明一个事件
        private int num;
    public MainPage() {
            this.InitializeComponent();
            string auth = "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("api" + ":" + "key-eb3829cd732d929feb06556c063010d11"));
            Debug.WriteLine(auth);
        }

        interface Animal {
            void saying(object sender);
        }

        class cat : Animal {
            TextBlock word;

            public cat(TextBlock w) {
                word = w;
            }
            public void saying(object sender) {
                word.Text += "cat: I'm a cat!\n";
            }
        }

        class dog : Animal {
            TextBlock word;

            public dog(TextBlock w) {
                word = w;
            }
            public void saying(object sender) {
                word.Text += "dog: I'm a dog!\n";
            }
        }

        class pig : Animal {
            TextBlock word;

            public pig(TextBlock w) {
                word = w;
            }
            public void saying(object sender) {
                word.Text += "pig: I'm a pig!\n";
            }
        }

        private cat c;
        private dog d;
        private pig p;

        private void btn_say_Click(object sender, RoutedEventArgs e) {
            num = new Random().Next() % 3;
            if (num == 0) {
                c = new cat(words);
                Say = new AnimalSaying(c.saying);
            } else if (num == 1) {
                d = new dog(words);
                Say = new AnimalSaying(d.saying);
            } else if (num == 2) {
                p = new pig(words);
                Say = new AnimalSaying(p.saying);
            }
            //执行事件
            Say(this);
            //滚动到底部
            scroll.ChangeView(null, scroll.ScrollableHeight, null);
        }

        private void button_sure_Click(object sender, RoutedEventArgs e) {
            string s = who.Text;
            if (s == "cat") {
                c = new cat(words);
                Say = new AnimalSaying(c.saying);
            } else if (s == "dog") {
                d = new dog(words);
                Say = new AnimalSaying(d.saying);
            } else if (s == "pig") {
                p = new pig(words);
                Say = new AnimalSaying(p.saying);
            } else {
                who.Text = "";
                return;
            }
            Say(this);
            scroll.ChangeView(null, scroll.ScrollableHeight, null);
            who.Text = "";
        }
    }
}