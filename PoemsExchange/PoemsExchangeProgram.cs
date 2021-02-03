﻿////////////////////////////////////////////////////////////////////
//                          _ooOoo_                               //
//                         o8888888o                              //
//                         88" . "88                              //
//                         (| ^_^ |)                              //
//                         O\  =  /O                              //
//                      ____/`---'\____                           //
//                    .'  \\|     |//  `.                         //
//                   /  \\|||  :  |||//  \                        //
//                  /  _||||| -:- |||||-  \                       //
//                  |   | \\\  -  /// |   |                       //
//                  | \_|  ''\---/''  |   |                       //
//                  \  .-\__  `-`  ___/-. /                       //
//                ___`. .'  /--.--\  `. . ___                     //
//              ."" '<  `.___\_<|>_/___.'  >'"".                  //
//            | | :  `- \`.;`\ _ /`;.`/ - ` : | |                 //
//            \  \ `-.   \_ __\ /__ _/   .-` /  /                 //
//      ========`-.____`-.___\_____/___.-`____.-'========         //
//                           `=---='                              //
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^        //
//         佛祖保佑       永无BUG     永不修改                       //
////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PoemsExchange
{
    public class ConfigLst
    {
        public List<Config> 配置列表 { get; set; }
    }

    public class Config
    {
        public string 根地址 { get; set; }
        public Dictionary<string,string> 替换参数 { get; set; }
        public List<CustomerRequest> 请求列表 { get; set; }
    }

    public class 替换参数
    {
        public string 辉立key{ get; set; }
        public string 用户名{ get; set; }
        public string 密码{ get; set; }
    }

    public class CustomerRequest
    {
        public string 相对地址{ get; set; }
        public string 发送数据{ get; set; }
        public HttpMethod 发送方法{ get; set; }
    }

    static class PoemsExchangeProgram
    {
        static void PrepareConfig(bool overwrite = false)
        {
            const string 帐号配置模板 = "帐号配置模板.json";
            if (!overwrite && File.Exists(帐号配置模板))
            {
                return;
            }
            var lst = new ConfigLst
            {
                配置列表 = new List<Config>
                {
                    new Config
                    {
                        根地址 = "https://trading.poems.com.hk",
                        替换参数 = new Dictionary<string, string>
                        {
                        ["{"+nameof(替换参数.用户名)+"}"] = "",
                        ["{"+nameof(替换参数.密码)+"}"] = "",
                        ["{"+nameof(替换参数.辉立key)+"}"] = ""
                        },
                        请求列表 = new List<CustomerRequest>
                        {
                            new CustomerRequest
                            {
                                发送方法 = HttpMethod.Post,
                                相对地址 ="/Poems2/LoginAction.asp",
                                发送数据 = "{'func': 'Login', 'Language': 'ZH', 'IPO': '','iFormType': '','Accode': {"+nameof(替换参数.用户名)+"},'Password': {"+nameof(替换参数.密码)+"}}"
                            },
                            new CustomerRequest
                            {
                                发送方法 = HttpMethod.Post,
                                相对地址="/Poems2/LoginAction.asp",
                                发送数据 = "{'iFunc': 'LOGIN', 'Language': '', 'IPO': '', 'pErrMsg': '', 'iFormType': '', 'pLoginId': {"+nameof(替换参数.用户名)+"},'iOTP': {"+nameof(替换参数.辉立key)+"}}"
                            }
                        }
                    }
                }
            };
            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            var jsonSer = JsonSerializer.Create(options);
            using (var createStream = File.CreateText(帐号配置模板))
            {
                jsonSer.Serialize(createStream,lst);
            }
            // var reader = new JsonTextReader(File.OpenText(帐号配置模板));
            // var result = jsonSer.Deserialize<ConfigLst>(reader);
            Console.WriteLine($"{帐号配置模板} 已创建，放在当前目录 {Environment.CurrentDirectory}");
        }
        
        static void Main(string[] args)
        {
            PrepareConfig();
            var programName = Environment.GetCommandLineArgs()[0];
            programName = Path.GetFileNameWithoutExtension(programName);
            Console.WriteLine(@$"使用方法：{programName} 配置文件名字
如果配置文件名字没有填写，则默认为：辉立帐号配置.json");
            var cfgFileName = "辉立帐号配置.json";
            if (args.Length > 0 && File.Exists(args[0]))
            {
                if (Directory.Exists(args[0]))
                {
                    Console.WriteLine($"警告，{args[0]}是一个目录，寻找默认配置文件：辉立帐号配置.json");
                }
                else
                {
                    cfgFileName = args[0];
                }
            }

            if (!File.Exists(cfgFileName))
            {
                Console.WriteLine($"当前目录 {Environment.CurrentDirectory} 找不到配置文件：辉立帐号配置.json，退出");
                return;
            }

            var options = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented
            };
            var jsonSer = JsonSerializer.Create(options);
            ConfigLst cfgLst;
            try
            {
                cfgLst = jsonSer.Deserialize<ConfigLst>(new JsonTextReader(File.OpenText(cfgFileName)));
            }
            catch (Exception e)
            {
                Console.WriteLine($"退出，读取配置文件出错: {e.Message}");
                return;
            }
            if (cfgLst == null)
            {
                Console.WriteLine("退出，无法读取配置文件");
                return;
            }
            if (cfgLst.配置列表.Count <= 0)
            {
                Console.WriteLine("退出，配置数量为0");
                return;
            }

            Parallel.ForEach(cfgLst.配置列表, cfg =>
            {
                Console.WriteLine("运行配置");
                jsonSer.Serialize(Console.Out, cfg);
                Console.WriteLine();
                ProcessExchange(cfg).Wait();
            });
        }

        private static async Task ProcessExchange(Config cfg)
        {
            if (cfg.请求列表 == null || cfg.请求列表.Count == 0)
            {
                return;
            }
            var baseAddr = cfg.根地址;
            var baseAddress = new Uri(baseAddr);
            var cookieContainer = new CookieContainer();
            using var handler = new HttpClientHandler() {CookieContainer = cookieContainer};
            using var client = new HttpClient(handler) {BaseAddress = baseAddress};

            var replaceDict = cfg.替换参数;
            try
            {
                foreach (var req in cfg.请求列表)
                {
                    Console.WriteLine($"请求 {req.相对地址}");
                    if (req.发送方法 == HttpMethod.Post)
                    {
                        var jsonData = ReplaceWith(req.发送数据, replaceDict);
                        var content = new StringContent(jsonData,Encoding.UTF8, "application/json");
                        var result = await client.PostAsync(req.相对地址, content);
                        result.EnsureSuccessStatusCode();
                        Console.WriteLine("返回结果");
                        Console.WriteLine(result);
                    }
                    else if (req.发送方法 == HttpMethod.Get)
                    {
                        var uri = ReplaceWith(req.相对地址, replaceDict);
                        var result = await client.GetAsync(uri);
                        result.EnsureSuccessStatusCode();
                        Console.WriteLine("返回结果");
                        Console.WriteLine(result);
                    }
                    else
                    {
                        Console.WriteLine($"这个Http方法暂时不支持：{req.发送方法}");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"请求异常：{e.Message}");
            }   
        }

        private static string ReplaceWith(string src, Dictionary<string, string> dict)
        {
            var tmp = src;
            foreach (var kv in dict)
            {
                tmp = tmp.Replace(kv.Key, kv.Value);
            }
            Console.Write("替换: ");
            Console.WriteLine(src);
            Console.Write("为: ");
            Console.WriteLine(tmp);
            return tmp;
        }
    }
}