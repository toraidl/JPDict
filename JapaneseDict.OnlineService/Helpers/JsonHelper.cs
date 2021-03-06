﻿using JapaneseDict.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Web.Http;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Security.Cryptography.Core;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Web.Http.Filters;
using JapaneseDict.OnlineService.Helpers;
using Windows.UI.Xaml.Media;

namespace JapaneseDict.OnlineService.Helpers
{
    public static class JsonHelper
    {
      
        private static async Task<string> GetJsonString(string uri)
        {
            try
            {
                var httpClient = new HttpClient();
                var response = await httpClient.GetAsync(new Uri(uri));
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();
                return responseText; 
            }
            catch
            {
                return "";
            }
        }
        private static async Task<string> GetJsonStringForTranslate(string uri,string text)
        {
            try
            {
                OAuthBase oAuthBase = new OAuthBase();
                var timestamp = oAuthBase.GenerateTimeStamp();
                var nonce = oAuthBase.GenerateNonce();
                var httpClient = new HttpClient();
                var request = new HttpRequestMessage();
                request.Method = HttpMethod.Post;
                Dictionary<string, string> pairs = new Dictionary<string, string>();
                pairs.Add("name", "kevingao");
                pairs.Add("key", TRANS_KEY);
                pairs.Add("text", text);
                pairs.Add("split", "1");
                pairs.Add("oauth_consumer_key", TRANS_KEY);
                pairs.Add("oauth_token", TRANS_SECRET);
                pairs.Add("oauth_timestamp", timestamp);
                pairs.Add("oauth_nonce", nonce);
                pairs.Add("oauth_version", "1.0");
                pairs.Add("oauth_signature", TRANS_SECRET+"&");
                pairs.Add("oauth_signature_method", "PLAINTEXT");

                var formContent = new HttpFormUrlEncodedContent(pairs);
                var response = await httpClient.PostAsync(new Uri(uri),formContent);
                response.EnsureSuccessStatusCode();
                var responseText = await response.Content.ReadAsStringAsync();
                return responseText;
            }
            catch
            {
                return "ERROR";
            }
        }
        const string appid = "20160211000011632";
        const string key = "NvduVsfjpNEclI03Sbei";
        const string TRANS_KEY = "7bec186b22bcf144ae4ca04545127be205aff7765";
        const string TRANS_SECRET = "bcf335e7f7358a2cf27ffae7a23b02d0";
        const string TRANS_URL_JPTOCN = "https://mt-auto-minhon-mlt.ucri.jgn-x.jp/api/mt/generalN_ja_zh-CN/";
        const string TRANS_URL_CNTOJP = "https://mt-auto-minhon-mlt.ucri.jgn-x.jp/api/mt/generalN_zh-CN_ja/";
        public static async Task<String> GetTranslateResult(string input, string sourcelang, string targetlang)
        {
            try
            {
                var random = new Random(1000000000).Next();
                var sign = Md5Helper.Encode(appid + input + random + key);
                string jsonStr = await GetJsonString("https://fanyi-api.baidu.com/api/trans/vip/translate?q=" + $"{Uri.EscapeDataString(input)}&from={sourcelang}&to={targetlang}&appid={appid}&salt={random}&sign={sign}");
                JsonObject jsonobj = JsonObject.Parse(jsonStr);
                return jsonobj["trans_result"].GetArray().GetObjectAt(0).GetObject()["dst"].GetString(); 
            }
            catch
            {
                return "出现错误";
            }
        }
        public static async Task<String> GetJpToCnTranslationResult(string input)
        {
            string jsonStr = await GetJsonStringForTranslate($"{TRANS_URL_JPTOCN}",input);
            JsonObject jsonobj = JsonObject.Parse(jsonStr);
            return jsonobj["resultset"].GetObject()["result"].GetObject()["text"].GetString();
        }
        public static async Task<String> GetCnToJpTranslationResult(string input)
        {
            string jsonStr = await GetJsonStringForTranslate($"{TRANS_URL_CNTOJP}", input);
            JsonObject jsonobj = JsonObject.Parse(jsonStr);
            return jsonobj["resultset"].GetObject()["result"].GetObject()["text"].GetString();
        }
        public static async Task<EverydaySentence> GetEverydaySentence(int index)
        {
            try
            {
                //string jsonStr = await GetJsonString("http://skylarkwsp-services.azurewebsites.net/api/EverydayJapanese?index=" +index);
                JsonObject resultobj = JsonObject.Parse(await GetJsonString("http://api.skylark-workshop.xyz/api/GetDailySentence?code=fi6c4bz3w5LkUnl8hGT0V4n/PoKBq7KH3Ly8za8HC4b/r8QRfj/zzw==&index=" + index));
                return new EverydaySentence() { JpText = resultobj["sentence"].GetString(), CnText = resultobj["trans"].GetString(), AudioUri = new Uri(resultobj["audio"].GetString()), NotesOnText = resultobj["sentencePoint"].GetString(), Author = resultobj["creator"].GetString(), BackgroundImage = new BitmapImage(new Uri($"ms-appx:///Assets/EverydaySentenceBackground/{index}.jpg", UriKind.RelativeOrAbsolute)) };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new EverydaySentence() { JpText = "出现错误", CnText = "请检查网络连接", BackgroundImage = new BitmapImage(new Uri($"ms-appx:///Assets/EverydaySentenceBackground/{index}.jpg", UriKind.RelativeOrAbsolute)) };
            }
        }
        public static async Task<NHKNews> GetNHKNews(int index)
        {
            try
            {
                string jsonStr = await GetJsonString("http://api.skylark-workshop.xyz/api/GetNHKNews?code=cElmudLe2wJ8tOXumYBog85EiqHN/76341GVoB5Ogtltdxrr/xlGmQ==");
                JsonObject jsonobj = JsonObject.Parse(jsonStr);
                var resultarritem = jsonobj["data"].GetObject()["item"].GetArray()[index];
                NHKNews res = new NHKNews() {Title=resultarritem.GetObject()["title"].GetString(),Link= new Uri(resultarritem.GetObject()["link"].GetString()), OriginalLink = new Uri(resultarritem.GetObject()["link"].GetString()), IconPath=new Uri(resultarritem.GetObject()["iconPath"].GetString()),VideoPath=new Uri(resultarritem.GetObject()["videoPath"].GetString()) };
                return res;
            }
            catch (Exception)
            {
                return new NHKNews() { Title = "出现连接错误",IconPath=new Uri("ms-appx:///Assets/connectionerr.png",UriKind.RelativeOrAbsolute)};
            }
        }
        public static async Task<List<NHKNews>> GetNHKNews()
        {
            try
            {
                string jsonStr = await GetJsonString("http://api.skylark-workshop.xyz/api/GetNHKNews?code=cElmudLe2wJ8tOXumYBog85EiqHN/76341GVoB5Ogtltdxrr/xlGmQ==");
                JsonObject jsonobj = JsonObject.Parse(jsonStr);
                var resultarritem = jsonobj["data"].GetObject()["item"].GetArray();
                List<NHKNews> res = new List<NHKNews>();
                foreach(var i in resultarritem)
                {
                    res.Add(new NHKNews() { Title = i.GetObject()["title"].GetString(), Link = new Uri(i.GetObject()["link"].GetString()), OriginalLink = new Uri(i.GetObject()["link"].GetString()), IconPath = new Uri(i.GetObject()["iconPath"].GetString()), VideoPath = new Uri(i.GetObject()["videoPath"].GetString()) });
                }
                return res;
            }
            catch
            {
                var err = new List<NHKNews>();
                for(int i=0;i<10;i++)
                {
                    err.Add(new NHKNews() { Title = "エラー", IconPath = new Uri("ms-appx:///Assets/connectionerr.png", UriKind.RelativeOrAbsolute) });
                }
                return err;
            }
        }
        public static async Task<List<NHKNews>> GetNHKEasyNews()
        {
            try
            {
                string jsonStr = await GetJsonString("https://slwspfunc.azurewebsites.net/api/GetNHKEasyNews?code=85lAgvwtrq9DfbDWpH6krQtNL8UkRtTsIjFuE7uyXVGfZLCrFt6VTw==");
                var jsonobj = JsonArray.Parse(jsonStr);
                List<NHKNews> res = new List<NHKNews>();
                foreach (var i in jsonobj)
                {
                    string id = i.GetObject()["newsId"].GetString();
                    string img = i.GetObject()["imageUri"].GetString();
                    res.Add(new NHKNews() { Title = i.GetObject()["title"].GetString(), Link= new Uri($"https://slwspfunc.azurewebsites.net/api/GetNewsWithRuby?code=Pwa68S3jYwHC80/aLHqDGLqFSEBfULUvNQEjzEymaipZwZxZTGg8zQ==&id={id}&img={img}"), OriginalLink= new Uri($"https://www3.nhk.or.jp/news/easy/{id}/{id}.html"), IconPath = new Uri(i.GetObject()["imageUri"].GetString()), IsEasy = true });
                }
                return res;
            }
            catch
            {
                var err = new List<NHKNews>();
                for (int i = 0; i < 10; i++)
                {
                    err.Add(new NHKNews() { Title = "エラー", IconPath = new Uri("ms-appx:///Assets/connectionerr.png", UriKind.RelativeOrAbsolute) });
                }
                return err;
            }
        }
        public static async Task<NHKRadios> GetNHKRadios(int index, string speed)
        {
            try
            {
                string jsonStr = await GetJsonString($"http://api.skylark-workshop.xyz/api/GetNHKRadio?code=NjIR9q6QzPOo29fPMCIZhuyie35aFxAgikYYwV5oFw5QzMVaUtSo6A==&speed={speed}&index={index}");
                JsonObject jsonobj = JsonObject.Parse(jsonStr);
                NHKRadios res = new NHKRadios() { Title = jsonobj["title"].GetString(), StartDate = jsonobj["startdate"].GetString(), EndDate = jsonobj["enddate"].GetString(), SoundUrl = new Uri(jsonobj["soundurl"].GetString()) };
                return res;
            }
            catch
            {
                return new NHKRadios() { Title = "出现连接错误", StartDate = "请检查网络连接" };
            }
        }
        public static async Task<int> GetNHKRadiosItemsCount()
        {
            try
            {
                return Int32.Parse(await GetJsonString("http://api.skylark-workshop.xyz/api/GetNHKRadio?code=NjIR9q6QzPOo29fPMCIZhuyie35aFxAgikYYwV5oFw5QzMVaUtSo6A==&getItemsCount=true"));

            }
            catch
            {
                return 1;
            }
        }
        public static async Task<FormattedNews> GetFormattedNews(string url)
        {
            string jsonStr = await GetJsonString($"http://api.skylark-workshop.xyz/api/GetFormattedNews?code=m3Bk0nEYpaEACAD9BHVsdtgLhJAkT3Hhr4kFPEfpfQgllIsok9rPEw==&url={url}");
            JsonObject jsonobj = JsonObject.Parse(jsonStr);
            return new FormattedNews() { Title = jsonobj["title"].GetString(), Content = jsonobj["content"].GetString(), Image = new Uri(jsonobj["image"].GetString()) };
        }
        /// <summary>
        /// Get UTC+8
        /// </summary>
        /// <returns></returns>
        public static async Task<DateTime> GetDate()
        {
            try
            {
                string datejson = await GetJsonString("http://skylarkwsp-services.azurewebsites.net/api/DateTime");
                return (DateTime.Parse(JsonObject.Parse(datejson)["datetime"].GetString()));
            }
            catch
            {
                return DateTime.Now;
            }
        }

        public static async Task<List<string>> GetLemmatized(string sentence)
        {
            try
            {
                string jsonStr = await GetJsonString($"https://jpdict-lemmatizer.azurewebsites.net/api/lemmatized?sentence={sentence}");
                JsonArray jsonArr = JsonArray.Parse(jsonStr);
                List<string> result = new List<string>();
                foreach(var i in jsonArr)
                {
                    result.Add(i.GetString());
                }
                return result;
            }
            catch
            {
                List<string> result = new List<string>();
                result.Add(sentence);
                return result;
            }
        }
    }
}
