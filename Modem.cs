using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZTEModem
{
    class Modem
    {
        private HttpClient client = null;
        private string router = string.Empty;

        public class DeviceInfo
        {
            public string Model { get; set; }
            public string SerialNumber { get; set; }
            public string HardwareVersion { get; set; }
            public string SoftwareVersion { get; set; }
            public string BootloaderVersion { get; set; }
            public string PonSerialNumber { get; set; }
            public string BatchNumber { get; set; }
        }

        public Modem(string router)
        {
            client = new HttpClient();
            this.router = ("http://" + router).TrimEnd('/');
        }

        public void Close()
        {
            client?.Dispose();
        }

        public async Task<DeviceInfo> GetDeviceInfo()
        {
            var response = (await client.GetAsync(router + "/template.gch")).EnsureSuccessStatusCode();
            var sources = await response.Content.ReadAsStringAsync();
            return new DeviceInfo()
            {
                Model = sources.Find("Frm_ModelName.+?>(.+?)</", "Unknown").HtmlDecode(),
                SerialNumber = sources.Find("Frm_SerialNumber.+?>\n{0,}(.+?)\n{0,}</", "Unknown").HtmlDecode(),
                HardwareVersion = sources.Find("Frm_HardwareVer.+?>\n{0,}(.+?)\n{0,}</", "Unknown").HtmlDecode(),
                SoftwareVersion = sources.Find("Frm_SoftwareVer.+?>\n{0,}(.+?)\n{0,}</", "Unknown").HtmlDecode(),
                BootloaderVersion = sources.Find("Frm_BootVer.+?>\n{0,}(.+?)\n{0,}</", "Unknown").HtmlDecode(),
                PonSerialNumber = sources.Find("var sn = \"(.+?)\"", "Unknown"),
                BatchNumber = sources.Find("Frm_SoftwareVerExtent.+?>\n{0,}(.+?)\n{0,}</", "Unknown").HtmlDecode()
            };
        }

        public async Task<bool> isLogin()
        {
            var response = await client.GetAsync(router + "/template.gch");
            return response.StatusCode == HttpStatusCode.OK;
        }

        public async Task<bool> Login(string username, string password)
        {
            // prepare login
            var response = (await client.GetAsync(router)).EnsureSuccessStatusCode();
            var sources = await response.Content.ReadAsStringAsync();

            // hidden form values
            var login_token = sources.Find("Frm_Logintoken.*?\"(\\d+)\"", "0");
            var check_token = sources.Find("Frm_Loginchecktoken.*?\"(\\d+)\"", "0");
            var random_num = new Random().Next(10000000, 99999999).ToString();
            var encoded_pass = (password + random_num).Sha256();

            Console.WriteLine($"  Login Token  : {login_token}\n" +
                              $"  Random Num   : {random_num}\n" +
                              $"  Check Token  : {check_token}");
            //return false;

            // login form
            var content = new StringContent(
                $"action=login&Username={username}&Password={encoded_pass}&Frm_Logintoken={login_token}&UserRandomNum={random_num}&Frm_Loginchecktoken={check_token}",
                Encoding.UTF8, "application/x-www-form-urlencoded");
            (await client.PostAsync(router, content)).EnsureSuccessStatusCode();

            // final login check
            return await isLogin();
        }

        public async Task Reboot()
        {
            var response = (await client.GetAsync(router + "/getpage.gch?pid=1002&nextpage=manager_dev_conf_t.gch"))
                .EnsureSuccessStatusCode();
            var sources = await response.Content.ReadAsStringAsync();
            var session_token = sources.Find("session_token.*?\"(\\d+)\"", "0");
            Console.WriteLine("  Session Token : " + session_token);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(1));

            try
            {
                var content = new StringContent(
                    "IF_ACTION=devrestart&IF_ERRORSTR=SUCC&IF_ERRORPARAM=SUCC&IF_ERRORTYPE=-1&flag=1&_SESSION_TOKEN=" + session_token,
                    Encoding.UTF8, "application/x-www-form-urlencoded");
                await client.PostAsync(router + "/getpage.gch?pid=1002&nextpage=manager_dev_conf_t.gch", content, cts.Token);
            }
            catch (Exception) { }
            finally { cts.Dispose(); }
        }
    }
}
