using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SegoeMDL2IconViewer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : TianXiaTech.BlurWindow
    {
        private const string DocUrl = "https://learn.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font";
        private const string DocUrlPrefix = "https://learn.microsoft.com/en-us/windows/apps/design/style/";

        private ObservableCollection<Icon> displayIcons = new ObservableCollection<Icon>();
        private List<Icon> rawIcons = new List<Icon>();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadIconsAsync();
        }

        private async Task<string> GetHtmlSourceAsync(string url)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.6261.95 Safari/537.36");
            httpClient.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");

            var html = await httpClient.GetStringAsync(url);
            return html;
        }

        private async void LoadIconsAsync()
        {
            var html = await GetHtmlSourceAsync(DocUrl);
            rawIcons = ParseHtml(html);
            displayIcons = new ObservableCollection<Icon>(rawIcons);
            this.listbox.ItemsSource = displayIcons;
        }

        private List<Icon> ParseHtml(string html)
        {
            List<Icon> list = new List<Icon>();
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(html);
            var tbodyList = doc.DocumentNode.SelectNodes("//tbody");

            foreach (var tbody in tbodyList)
            {
                var trElementList = tbody.Elements("tr");
                for (int i = 1;i< trElementList.Count();i++)
                {
                    var trElement = trElementList.ElementAt(i);
                    Icon icon = new Icon();

                    var tdElementList = trElement.Elements("td");

                    icon.Glyph = DocUrlPrefix + tdElementList.ElementAt(0).Element("img").Attributes["src"].Value;
                    icon.Code = tdElementList.ElementAt(1).InnerText;
                    icon.Description = tdElementList.ElementAt(2).InnerText;
                    list.Add(icon);
                }
            }
            return list;
        }

        private void listbox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.listbox.SelectedIndex == -1)
                return;

            var icon = this.listbox.SelectedItem as Icon;
            System.Windows.Forms.Clipboard.SetText(icon.Code);
        }

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            this.listbox.ItemsSource = null;

            if (string.IsNullOrEmpty(this.tbox_Keyword.Text))
            {
                displayIcons = new ObservableCollection<Icon>(rawIcons);
            }
            else
            {
                var list = rawIcons.Where(x => x.Description.ToUpper().Contains(this.tbox_Keyword.Text.ToUpper()));
                displayIcons = new ObservableCollection<Icon>(list);
            }

            this.listbox.ItemsSource = displayIcons;
        }
    }
}
