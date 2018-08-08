﻿using lkcode.hetznercloudapi.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lkcode.hetznercloudapi.Api
{
    public class Server
    {
        /// <summary>
        /// 
        /// </summary>
        public long Id { get; set; } = 0;

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// 
        /// </summary>
        public DateTimeOffset Created { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public Network Network { get; set; }

        #region # static methods #

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static async Task<List<Server>> GetAsync()
        {
            List<Server> serverList = new List<Server>();

            string responseContent = await ApiCore.SendRequest("/servers");
            Objects.Server.Get.Response response = JsonConvert.DeserializeObject<Objects.Server.Get.Response>(responseContent);

            foreach (Objects.Server.Universal.Server responseServer in response.servers)
            {
                Server server = GetServerFromResponseData(responseServer);

                serverList.Add(server);
            }

            return serverList;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public static async Task<List<Server>> GetAsync(string filter)
        {
            if(string.IsNullOrEmpty(filter) || string.IsNullOrWhiteSpace(filter))
            {
                return await GetAsync();
            }

            List<Server> serverList = new List<Server>();

            string responseContent = await ApiCore.SendRequest(string.Format("/servers?name={0}", filter));
            Objects.Server.Get.Response response = JsonConvert.DeserializeObject<Objects.Server.Get.Response>(responseContent);

            foreach (Objects.Server.Universal.Server responseServer in response.servers)
            {
                Server server = GetServerFromResponseData(responseServer);

                serverList.Add(server);
            }

            return serverList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<Server> GetAsync(long id)
        {
            string responseContent = await ApiCore.SendRequest(string.Format("/servers/{0}", id.ToString()));
            Objects.Server.GetOne.Response response = JsonConvert.DeserializeObject<Objects.Server.GetOne.Response>(responseContent);

            Server server = GetServerFromResponseData(response.server);

            return server;
        }

        #endregion

        #region # class methods #

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ServerActionResponse> Shutdown()
        {
            string responseContent = await ApiCore.SendPostRequest(string.Format("/servers/{0}/actions/shutdown", this.Id));
            Objects.Server.PostShutdown.Response response = JsonConvert.DeserializeObject<Objects.Server.PostShutdown.Response>(responseContent);

            ServerActionResponse actionResponse = GetServerActionFromResponseData(response.action);

            return actionResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ServerActionResponse> Reset()
        {
            string responseContent = await ApiCore.SendPostRequest(string.Format("/servers/{0}/actions/reset", this.Id));
            Objects.Server.PostReset.Response response = JsonConvert.DeserializeObject<Objects.Server.PostReset.Response>(responseContent);

            ServerActionResponse actionResponse = GetServerActionFromResponseData(response.action);

            return actionResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ServerActionResponse> PowerOn()
        {
            string responseContent = await ApiCore.SendPostRequest(string.Format("/servers/{0}/actions/poweron", this.Id));
            Objects.Server.PostPoweron.Response response = JsonConvert.DeserializeObject<Objects.Server.PostPoweron.Response>(responseContent);

            ServerActionResponse actionResponse = GetServerActionFromResponseData(response.action);

            return actionResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ServerActionResponse> Reboot()
        {
            string responseContent = await ApiCore.SendPostRequest(string.Format("/servers/{0}/actions/reboot", this.Id));
            Objects.Server.PostReboot.Response response = JsonConvert.DeserializeObject<Objects.Server.PostReboot.Response>(responseContent);

            ServerActionResponse actionResponse = GetServerActionFromResponseData(response.action);

            return actionResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ServerActionResponse> ResetPassword()
        {
            string responseContent = await ApiCore.SendPostRequest(string.Format("/servers/{0}/actions/reset_password", this.Id));
            Objects.Server.ResetPassword.Response response = JsonConvert.DeserializeObject<Objects.Server.ResetPassword.Response>(responseContent);

            ServerActionResponse actionResponse = GetServerActionFromResponseData(response.action);
            actionResponse.AdditionalActionContent = response.root_password;

            return actionResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<ServerActionResponse> PowerOff()
        {
            string responseContent = await ApiCore.SendPostRequest(string.Format("/servers/{0}/actions/poweroff", this.Id));
            Objects.Server.PostPoweroff.Response response = JsonConvert.DeserializeObject<Objects.Server.PostPoweroff.Response>(responseContent);

            ServerActionResponse actionResponse = GetServerActionFromResponseData(response.action);

            return actionResponse;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public async Task<ServerActionResponse> CreateImage(string description, string type)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(description.Trim()) &&
                !string.IsNullOrWhiteSpace(description.Trim()))
            {
                arguments.Add("description", description);
            }

            if (!string.IsNullOrEmpty(type.Trim()) &&
                !string.IsNullOrWhiteSpace(type.Trim()))
            {
                arguments.Add("type", type);
            }
            
            string responseContent = await ApiCore.SendPostRequest(string.Format("/servers/{0}/actions/create_image", this.Id), arguments);
            JObject responseObject = JObject.Parse(responseContent);

            if(responseObject["error"] != null)
            {
                // error
                Objects.Server.Universal.ErrorResponse error = JsonConvert.DeserializeObject<Objects.Server.Universal.ErrorResponse>(responseContent);
                ServerActionResponse response = new ServerActionResponse();
                response.Error = GetErrorFromResponseData(error);

                return response;
            } else
            {
                // success
                Objects.Server.PostCreateImage.Response response = JsonConvert.DeserializeObject<Objects.Server.PostCreateImage.Response>(responseContent);

                ServerActionResponse actionResponse = GetServerActionFromResponseData(response.action);
                actionResponse.AdditionalActionContent = GetServerImageFromResponseData(response.image);

                return actionResponse;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public async Task<ServerActionResponse> RebuildImage(string image)
        {
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(image.Trim()) &&
                !string.IsNullOrWhiteSpace(image.Trim()))
            {
                arguments.Add("image", image);
            }

            string responseContent = await ApiCore.SendPostRequest(string.Format("/servers/{0}/actions/rebuild", this.Id), arguments);
            JObject responseObject = JObject.Parse(responseContent);

            if (responseObject["error"] != null)
            {
                // error
                Objects.Server.Universal.ErrorResponse error = JsonConvert.DeserializeObject<Objects.Server.Universal.ErrorResponse>(responseContent);
                ServerActionResponse response = new ServerActionResponse();
                response.Error = GetErrorFromResponseData(error);

                return response;
            }
            else
            {
                // success
                Objects.Server.PostRebuild.Response response = JsonConvert.DeserializeObject<Objects.Server.PostRebuild.Response>(responseContent);

                ServerActionResponse actionResponse = GetServerActionFromResponseData(response.action);
                //actionResponse.AdditionalActionContent = GetServerImageFromResponseData(response.image);

                return actionResponse;
            }
        }

        #endregion

        #region # private methods for processing #

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseServer"></param>
        /// <returns></returns>
        private static Server GetServerFromResponseData(Objects.Server.Universal.Server responseData)
        {
            Server server = new Server();

            server.Id = responseData.id;
            server.Name = responseData.name;
            server.Status = responseData.status;
            server.Created = responseData.created;
            server.Network = new Network()
            {
                Ipv4 = new AddressIpv4()
                {
                    Ip = responseData.public_net.ipv4.ip,
                    Blocked = responseData.public_net.ipv4.blocked
                },
                Ipv6 = new AddressIpv6()
                {
                    Ip = responseData.public_net.ipv6.ip,
                    Blocked = responseData.public_net.ipv6.blocked
                },
                FloatingIpIds = responseData.public_net.floating_ips
            };

            return server;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseData"></param>
        /// <returns></returns>
        private static ServerActionResponse GetServerActionFromResponseData(Objects.Server.Universal.ServerAction responseData)
        {
            ServerActionResponse serverAction = new ServerActionResponse();

            serverAction.Id = responseData.id;
            serverAction.Command = responseData.command;
            serverAction.Progress = responseData.progress;
            serverAction.Started = responseData.started;
            serverAction.Status = responseData.status;

            return serverAction;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="responseData"></param>
        /// <returns></returns>
        private static ServerImage GetServerImageFromResponseData(Objects.Server.PostCreateImage.Image responseData)
        {
            ServerImage serverImage = new ServerImage();

            serverImage.Id = responseData.id;
            serverImage.Type = responseData.type;
            serverImage.Name = responseData.name;
            serverImage.Description = responseData.description;

            return serverImage;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="errorResponse"></param>
        /// <returns></returns>
        private static Error GetErrorFromResponseData(Objects.Server.Universal.ErrorResponse errorResponse)
        {
            Error error = new Error();

            error.Message = errorResponse.error.message;
            error.Code = errorResponse.error.code;

            return error;
        }

        #endregion
    }
}