﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Components.WxApi;
using WebApi.Models;

namespace WebApi.Controllers
{
    /// <summary>
    /// 与微信交互的接口
    /// </summary>
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class WxHelperController : ControllerBase
    {
        [HttpGet]
        [ActionName("Responser")]
        public string Get(string signature, string timestamp, string nonce, string echostr)
        {
            if (string.IsNullOrEmpty(signature) 
                || string.IsNullOrEmpty(timestamp)
                || string.IsNullOrEmpty(nonce))
            {
                return string.Empty;
            }

            if (WxBase.CheckSignature(signature, timestamp, nonce))
            {
                return echostr; //返回随机字符串则表示验证通过
            }
            else
            {
                Console.WriteLine($"failed check signature:{signature} with {timestamp},{nonce}");
                return string.Empty;
            }
        }

        [HttpPost]
        [ActionName("Responser")]
        public async Task<ContentResult> Post()
        {
            using (Stream stream = Request.Body)
            {
                byte[] buffer = new byte[Request.ContentLength.Value];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                string xml = Encoding.UTF8.GetString(buffer);
                Console.WriteLine($"收到：{xml}");
                var msg = WxTextMsg.FromXml(xml);

                if (msg.MsgType.Contains("text")
                    && msg.Content.Contains("磁芯大战"))
                {
                    string openId = msg.FromUserName;
                    //
                    var info = await WxBase.GetUserInfo(openId);
                    Console.WriteLine($"获取：{info.nickname} 信息");


                    //
                    var article = new WxArticle("MagCore - 磁芯大战", "进入房间创建游戏",
                        "https://pic2.zhimg.com/50/v2-36dfc53b61eda72a96d7165198a3fdc6_400x224.jpg",
                        "https://www.zhihu.com/question/48903769/answer/656886137");
                    var reply = new WxReplyMsg(msg.FromUserName, msg.ToUserName, msg.CreateTime,
                        "news", new WxArticle[] { article });
                    string text = reply.ToXml();
                    Console.WriteLine($"回复：{text}");

                    return new ContentResult() { StatusCode = 200, Content = text };
                }
            }

            return new ContentResult();
        }        
    }
}
