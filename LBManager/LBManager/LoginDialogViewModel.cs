using LBManager.Infrastructure.Models;
using Newtonsoft.Json;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace LBManager
{
    class LoginDialogViewModel : BindableBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set { SetProperty(ref _password, value); }
        }

        public async Task<LoginResponse> Login()
        {
            HttpClient httpClient = new HttpClient();
            LoginRequest loginRequest = new LoginRequest() { UserName = this.Name, Password = this.Password };
            string loginRequestJson = JsonConvert.SerializeObject(loginRequest);

            HttpResponseMessage response = await httpClient.PostAsync("http://lbcloud.ddt123.cn/?s=api/Manager/login", new StringContent(loginRequestJson));

            response.EnsureSuccessStatusCode();

            string content = await response.Content.ReadAsStringAsync();
            return await Task.Run(() => JsonConvert.DeserializeObject<LoginResponse>(content));
        }

    }
}
