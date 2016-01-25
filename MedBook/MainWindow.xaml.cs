using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using MedBookLib.DataTypes;
using MedBookLib.Parsing;
using xNet.Net;
using Application = System.Windows.Application;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using HttpRequest = xNet.Net.HttpRequest;
using MessageBox = System.Windows.MessageBox;
using WebBrowser = System.Windows.Forms.WebBrowser;

namespace MedBook
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        [DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool InternetSetCookie(string lpszUrlName, string lbszCookieName, string lpszCookieData);

        public ObservableCollection<AptekaStruct> AptekaCollection { get; set; }
        public ObservableCollection<InfoStruct> InfoCollection { get; set; }
        public readonly WebBrowser Web = new WebBrowser();

        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();

            Web.ScriptErrorsSuppressed = true;
            AptekaCollection = new ObservableCollection<AptekaStruct>();
            InfoCollection = new ObservableCollection<InfoStruct>();
            DataGridInfo.ItemsSource = InfoCollection;
            DataGridApteka.ItemsSource = AptekaCollection;
        }

        private async void Compleated(object s, WebBrowserDocumentCompletedEventArgs e)
        {
            var source = Web.DocumentText;
            if (source.Contains("Начало основного контента"))
            {
                Web.DocumentCompleted -= Compleated;
                var list = await ParsingAptek.Parse(source);
                await AddCollection(AptekaCollection, list);
            }
        }

        private async Task WebCookies(string url, CookieDictionary cookie)
        {
            await Task.Run(() =>
            {
                var i = 0;
                foreach (var pair in cookie.Select(t => cookie.ElementAt(i++)))
                    InternetSetCookie(url, pair.Key, pair.Value);
                Web.Navigate(url);
            });
        }

        private async void ButtonSearch_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(tbSerch.Text))
            {
                MessageBox.Show("Введіть назву ліків!");
                return;
            }

            Search.IsEnabled = false;
            var search = tbSerch.Text;
            await Task.Run(async () =>
            {
                Web.DocumentCompleted += Compleated;

                var infoList = await GetInfo(search);
                await AddCollection(InfoCollection, infoList);

                await GetApteka(search);
            });
            Search.IsEnabled = true;
        }

        private static async Task AddCollection<T>(ICollection<T> collection, IEnumerable<T> list)
        {
            await Task.Run(async () =>
            {
                try
                {
                    await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            collection.Clear();
                        }
                        catch (Exception)
                        {
                        }

                        foreach (var y in list)
                            collection.Add(y);
                    }));
                }
                catch (Exception)
                {
                }
            });
        }

        private static async Task<List<InfoStruct>> GetInfo(string search)
        {
            return await Task.Run(async () =>
            {
                try
                {
                    var doc = new HtmlDocument();
                    var req = new HttpRequest
                    {
                        EnableAdditionalHeaders = true,
                        UserAgent = HttpHelper.ChromeUserAgent(),
                        Cookies = new CookieDictionary(),
                        //Proxy = new HttpProxyClient("127.0.0.1", 8888),
                        ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8"
                    };

                    var newSearch =
                        HttpUtility.UrlEncode(search, Encoding.GetEncoding("windows-1251"));

                    var respBytes = req.Get("http://mozdocs.kiev.ua/liki.php").ToBytes();
                    var resp = Encoding.GetEncoding("windows-1251").GetString(respBytes);

                    if (resp.Contains("http://mozdocs.kiev.ua/oops/?hash"))
                    {
                        resp = Encoding.UTF8.GetString(respBytes);
                        doc.LoadHtml(resp);
                        var postUrl = doc.DocumentNode.Descendants("form")
                            .First()
                            .GetAttributeValue("action", string.Empty);
                        var imgUrl = doc.DocumentNode.Descendants("img").First().GetAttributeValue("src", string.Empty);
                        var imgBytes = req.Get(imgUrl).ToBytes();

                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var captcha = new Captcha(imgBytes);
                            if (captcha.ShowDialog() == true)
                            {
                                req.AddParam("code", captcha.TextResult);

                                respBytes = req.Post(postUrl).ToBytes();
                                resp = Encoding.GetEncoding("windows-1251").GetString(respBytes);
                            }
                        }));
                    }

                    respBytes =
                        req.Get(
                            $"http://mozdocs.kiev.ua/liki.php?name={newSearch}&lang=1&manufacturer=&category=0&likform=0&pokaz=&atcode=&go=%CF%EE%F8%F3%EA&nav=1&hf=1&am=")
                            .ToBytes();
                    resp = Encoding.GetEncoding("windows-1251").GetString(respBytes);

                    var url = await ParsingInfo.GetUrl(resp, search);
                    if (string.IsNullOrEmpty(url))
                        throw new Exception();

                    respBytes = req.Get(url).ToBytes();
                    resp = Encoding.GetEncoding("windows-1251").GetString(respBytes);

                    if (resp.Contains("http://mozdocs.kiev.ua/oops/?hash"))
                    {
                        resp = Encoding.UTF8.GetString(respBytes);
                        doc.LoadHtml(resp);
                        var postUrl = doc.DocumentNode.Descendants("form")
                            .First()
                            .GetAttributeValue("action", string.Empty);
                        var imgUrl = doc.DocumentNode.Descendants("img").First().GetAttributeValue("src", string.Empty);
                        var imgBytes = req.Get(imgUrl).ToBytes();

                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var captcha = new Captcha(imgBytes);
                            if (captcha.ShowDialog() == true)
                            {
                                req.AddParam("code", captcha.TextResult);

                                respBytes = req.Post(postUrl).ToBytes();
                                resp = Encoding.GetEncoding("windows-1251").GetString(respBytes);
                            }
                        }));
                    }

                    return await ParsingInfo.GetResult(resp);
                }
                catch (Exception)
                {
                    return new List<InfoStruct> {new InfoStruct {FarmVlast = "Нічого не знайдено."}};
                }
            });
        }

        private async Task GetApteka(string search)
        {
            await Task.Run(async () =>
            {
                try
                {
                    var collection = new CookieDictionary();
                    var doc = new HtmlDocument();
                    var req = new HttpRequest
                    {
                        EnableAdditionalHeaders = true,
                        UserAgent = HttpHelper.ChromeUserAgent(),
                        Cookies = collection,
                        ["Accept"] = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                        AllowAutoRedirect = true,
                        EnableEncodingContent = true,
                        CharacterSet = Encoding.UTF8,
                        //Proxy = new HttpProxyClient("127.0.0.1", 8888),
                    };

                    req.AddParam("searchstr", search.ToLower());
                    req.AddParam("go", "Найти!");
                    req.AddParam("city", "Киев");

                    var resp = req.Post("http://medbrowse.com.ua/search").ToString();
                    if (resp.Contains("http://medbrowse.com.ua/oops/?hash"))
                    {
                        doc.LoadHtml(resp);
                        var postUrl = doc.DocumentNode.Descendants("form")
                            .First()
                            .GetAttributeValue("action", string.Empty);
                        var imgUrl = doc.DocumentNode.Descendants("img").First().GetAttributeValue("src", string.Empty);
                        var imgBytes = req.Get(imgUrl).ToBytes();

                        await Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            var captcha = new Captcha(imgBytes);
                            if (captcha.ShowDialog() == true)
                            {
                                req.AddParam("code", captcha.TextResult);
                                resp = req.Post(postUrl).ToString();
                            }
                        }));
                    }

                    await WebCookies(req.Address.AbsoluteUri, collection);

                    return new List<AptekaStruct>();
                }
                catch (Exception)
                {
                    return new List<AptekaStruct>();
                }
            });
        }

        private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var textBlock = (TextBlock) sender;
                if (textBlock == null)
                    throw new Exception();

                tbSerch.Text = textBlock.Text;
                ButtonSearch_Click(null, null);
            }
            catch (Exception)
            {

            }
        }

        private void LaunchMedBookOnGitHub(object sender, RoutedEventArgs e)
        {
            Process.Start("https://github.com/mazanuj/MedBook");
        }
    }
}