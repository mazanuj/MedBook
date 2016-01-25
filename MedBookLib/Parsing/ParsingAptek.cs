using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MedBookLib.DataTypes;

namespace MedBookLib.Parsing
{
    public static class ParsingAptek
    {
        public static async Task<List<AptekaStruct>> Parse(string html)
        {
            return await Task.Run(() =>
            {
                var collection = new List<AptekaStruct>();
                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var apteki =
                        doc.DocumentNode.Descendants("li")
                            .Where(x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "apteka");

                    foreach (var z in apteki)
                    {
                        var localList = new List<AptekaStruct>();
                        try
                        {
                            var aptekaName = string.Empty;
                            var address = string.Empty;

                            try
                            {
                                //var addressElem = z.Descendants("address").First().ChildNodes.First(x => x.Name == "ol");
                                //var inner = addressElem.Descendants("li").First().InnerText;
                                var tel =
                                    Regex.Matches(z.InnerText, @"(?=Телефон:).+?(?=\s{2,}?\r?\n?(Заказать)?)")
                                        .AsEnumerable()
                                        .First(x => !x.Value.Contains("XX")).Value;
                                var street =
                                    $"Адрес: {Regex.Match(z.InnerText, @"(?<=description: \').+?(?=\')").Value}";
                                address = $"{tel}\r\n{street}";
                            }
                            catch (Exception)
                            {
                            }

                            try
                            {
                                aptekaName =
                                    z.Descendants("span")
                                        .First(x => x.Attributes["class"].Value == "caption")
                                        .Descendants("a")
                                        .First()
                                        .InnerText;
                            }
                            catch (Exception)
                            {
                            }

                            var drugs =
                                z.Descendants("li")
                                    .Where(y => y.Attributes.Contains("class") && y.Attributes["class"].Value == "drug")
                                    .ToList();

                            foreach (var h in drugs)
                            {
                                var priceStr =
                                    h.Descendants("div").First(x => x.Attributes["class"].Value == "value").InnerText;
                                priceStr = Regex.Match(priceStr, @"\d+\.\d+").Value;
                                var price = double.Parse(priceStr, new CultureInfo("En-us"));

                                var drugName =
                                    h.Descendants("span")
                                        .First(
                                            x =>
                                                x.Attributes.Contains("class") &&
                                                x.Attributes["class"].Value == "caption")
                                        .InnerText;
                                drugName = Regex.Match(drugName, @"(?=[^\r\n\s]).+(?<=[^\s])").Value;

                                localList.Add(new AptekaStruct
                                {
                                    Address = address,
                                    AptekaName = aptekaName,
                                    DrugName = drugName,
                                    Price = price
                                });
                            }
                        }
                        catch (Exception)
                        {

                        }

                        collection.AddRange(localList);
                    }
                }
                catch (Exception)
                {

                }
                return collection.Count > 0
                    ? collection
                    : new List<AptekaStruct> {new AptekaStruct {DrugName = "Нічого не знайдено"}};
            });
        }
    }

    public static class ExtensionMethods
    {
        /// <summary>
        /// Returns the input typed as a generic IEnumerable of the groups
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<System.Text.RegularExpressions.Group> AsEnumerable(
            this System.Text.RegularExpressions.GroupCollection gc)
        {
            return gc.Cast<Group>();
        }

        /// <summary>
        /// Returns the input typed as a generic IEnumerable of the matches
        /// </summary>
        /// <param name="mc"></param>
        /// <returns></returns>
        public static IEnumerable<System.Text.RegularExpressions.Match> AsEnumerable(
            this System.Text.RegularExpressions.MatchCollection mc)
        {
            return mc.Cast<Match>();
        }
    }
}