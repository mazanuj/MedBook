using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MedBookLib.DataTypes;

namespace MedBookLib.Parsing
{
    public static class ParsingInfo
    {
        public static async Task<string> GetUrl(string html, string search)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    var elements =
                        doc.DocumentNode.Descendants("a")
                            .Where(x => x.Attributes["href"].Value.StartsWith("likiview.php?id="));

                    var url = @"http://mozdocs.kiev.ua/" +
                              elements.First(x => Regex.IsMatch(x.InnerText.ToLower(), $@"^{search.ToLower()}[\®\s]"))
                                  .Attributes[
                                      "href"].Value;

                    return url;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            });
        }

        public static async Task<List<InfoStruct>> GetResult(string html)
        {
            return await Task.Run(() =>
            {
                try
                {
                    var info = new InfoStruct();
                    var doc = new HtmlDocument();
                    doc.LoadHtml(html);

                    try
                    {
                        var instrElem =
                            doc.DocumentNode.Descendants("div")
                                .First(
                                    x => x.Attributes.Contains("class") && x.Attributes["class"].Value == "instruction");

                        if (instrElem != null)
                        {
                            var res = instrElem.InnerText;
                            var zas = Regex.Match(res, @"(?<=Спосіб застосування та дози\.\r\n).+(?=\r\n)").Value;
                            info.Zastosuvannya = zas;

                            var pokaz = Regex.Match(res, @"(?<=[(Показання)(Показання для застосування)]\.\r\n).+(?=\r\n)").Value;
                            info.Pokazannya = pokaz;

                            var protu = Regex.Match(res, @"(?<=Протипоказання\.\r\n).+(?=\r\n)").Value;
                            info.Protupokazannya = protu;

                            var farm = Regex.Match(res, @"(?<=Фармакотерапевтична група\.).+(?=\r\n)").Value;
                            info.FarmVlast = farm;

                            var zber = Regex.Match(res, @"(?<=Умови зберігання\.).+(?=\r\n)").Value;
                            info.Zberigannya = zber;
                        }
                    }
                    catch (Exception)
                    {
                        try
                        {
                            var pokaz =
                                doc.DocumentNode.Descendants("span")
                                    .First(x => !string.IsNullOrEmpty(x.InnerText) && x.InnerText.Contains("Показання"));

                            if (pokaz != null)
                                info.Pokazannya = pokaz.ParentNode.NextSibling.InnerText;
                        }
                        catch (Exception)
                        {
                        }

                        try
                        {
                            var farm =
                                doc.DocumentNode.Descendants("span")
                                    .First(
                                        x =>
                                            !string.IsNullOrEmpty(x.InnerText) &&
                                            x.InnerText.Contains("Фармакотерапевтична група"));

                            if (farm != null)
                                info.FarmVlast = farm.ParentNode.NextSibling.InnerText;
                        }
                        catch (Exception)
                        {

                        }

                        info.Zberigannya = "Зберігати при температурі не вище 25 °С у недоступному для дітей місці.";
                    }
                    
                    return new List<InfoStruct> {info};
                }
                catch (Exception)
                {
                    return new List<InfoStruct>();

                }
            });
        }
    }
}