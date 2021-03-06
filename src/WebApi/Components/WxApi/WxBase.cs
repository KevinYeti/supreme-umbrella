﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using SystemCommonLibrary.Json;
using WebApi.Components.Manager;
using WebApi.Models;

namespace WebApi.Components.WxApi
{
    public static class WxBase
    {
        public static string BaseUrl = "https://api.weixin.qq.com/";
        private static string _tokenUrl = "cgi-bin/token?grant_type=client_credential&appid={0}&secret={1}";
        private static string _token = "talkischeap";
        private static string _infoUrl = "cgi-bin/user/info?access_token={0}&openid={1}";
        private static string _menuUrl = "cgi-bin/menu/create?access_token={0}";

        private static string _accessToken = string.Empty;
        private static DateTime _expireTime = DateTime.Now.AddHours(-2);

        public static async Task<string> GetAccessToken()
        {
            if (DateTime.Now >= _expireTime)
            {
                try
                {
                    var client = new HttpClient() { BaseAddress = new Uri(BaseUrl) };
                    var response = await client.GetAsync(string.Format(_tokenUrl, HelperConfig.Current.WxAppid, HelperConfig.Current.WxSecret));
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        var result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(result);
                        var json = DynamicJson.Parse(result);
                        var access = json.Deserialize<WxAccessToken>();
                        _accessToken = access.access_token;
                        _expireTime = DateTime.Now.AddSeconds(access.expires_in);

                        return _accessToken;
                    }
                    else
                        throw new Exception("微信服务器错误");
                }
                catch (Exception ex)
                {
                    throw ex;
                }

            }
            else
                return _accessToken;
        }

        /// <summary>
        /// 验证微信签名
        /// * 将token、timestamp、nonce三个参数进行字典序排序
        /// * 将三个参数字符串拼接成一个字符串进行sha1加密
        /// * 开发者获得加密后的字符串可与signature对比，标识该请求来源于微信。
        /// </summary>
        /// <returns>bool</returns>
        public static bool CheckSignature(string signature, string timestamp, string nonce)
        {
            string[] arr = { _token, timestamp, nonce };
            Array.Sort(arr);     //字典排序
            string str = string.Join("", arr);
            var hash = SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(str));
            StringBuilder sign = new StringBuilder();
            for (var i = 0; i < hash.Length; i++)
                sign.Append(hash[i].ToString("X2"));

            return sign.ToString().ToLower() == signature.ToLower();
        }

        public static async Task<WxUserInfo> GetUserInfo(string openId)
        {
            try
            {
                var client = new HttpClient() { BaseAddress = new Uri(BaseUrl) };
                var response = await client.GetAsync(string.Format(_infoUrl, await GetAccessToken(), openId));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("GetUserInfo success:" + result);
                    var json = DynamicJson.Parse(result);
                    
                    var info = json.Deserialize<WxUserInfo>();
                    //info.openid = json.openid;
                    //info.sex = json.sex;
                    //info.nickname = json.nickname;
                    //info.headimgurl = json.headimgurl;

                    return info;
                }
                else
                    throw new Exception("微信服务器错误");
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<bool> InitMenu()
        {
            if (GlobalVarsManager.IsWxMenuInited)
            {
                return true;
            }
            try
            {
                var client = new HttpClient() { BaseAddress = new Uri(BaseUrl) };
                var response = await client.PostAsync(string.Format(_menuUrl, await GetAccessToken()), 
                                                        new StringContent(WxMenu.Current));
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    GlobalVarsManager.IsWxMenuInited = true;
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
