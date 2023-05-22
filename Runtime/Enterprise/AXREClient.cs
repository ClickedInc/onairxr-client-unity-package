using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace onAirXR.Client {
    public class AXREClient {
        private string _endpointWithoutPath;

        public void SetAddress(string ipaddr, int port) {
            _endpointWithoutPath = $"http://{ipaddr}:{port}";
        }

        public async Task<AXREContent[]> GetContents() {
            checkEndpoint();

            using (var request = AXREApi.GetContentsRequest(_endpointWithoutPath)) {
                var data = await send(request);
                return JsonArrayParser<AXREContent>.Parse(data.text);
            }
        }

        public async Task<Texture2D> GetThumbnail(AXREContent content) {
            checkEndpoint();
            if (content?.thumbnail == null) { return null; }

            using (var request = AXREApi.GetTextureRequest(_endpointWithoutPath, content.thumbnail)) {
                var data = await send(request);
                return ((DownloadHandlerTexture)data).texture;
            }
        }

        public async Task<AXREGroup> GetRunningGroup() {
            checkEndpoint();

            using (var request = AXREApi.GetGroupsRequest(_endpointWithoutPath)) {
                try {
                    var data = await send(request);
                    return JsonUtility.FromJson<AXREGroup>(data.text);
                }
                catch (AXREException e) {
                    if (e.code != AXREException.Code.HTTP) { throw e; }

                    switch (e.responseCode) {
                        case 404:
                            return null;
                        case 503:
                            throw new AXREException(AXREException.Code.Busy, e.responseCode, e.reason);
                        default:
                            throw e;
                    }
                }
            }
        }

        public async Task<AXREGroup> CreateGroup(string contentId) {
            checkEndpoint();

            using (var request = AXREApi.CreateGroupRequest(_endpointWithoutPath, contentId)) {
                var data = await send(request);
                return JsonUtility.FromJson<AXREGroup>(data.text);
            }
        }

        public async Task DeleteGroup() {
            checkEndpoint();

            using (var request = AXREApi.DeleteAllGroupsRequest(_endpointWithoutPath)) {
                try {
                    await send(request);
                }
                catch (AXREException e) {
                    if (e.code != AXREException.Code.HTTP) { throw e; }

                    switch (e.responseCode) {
                        case 404:
                            throw new AXREException(AXREException.Code.GroupNotFound, 0, "no group to delete");
                        default:
                            throw e;
                    }
                }
            }
        }

        public async Task PutGroupCommand(string command) {
            checkEndpoint();

            using (var request = AXREApi.PutGroupCommandRequest(_endpointWithoutPath, command)) {
                await send(request);
            }
        }

        public async Task<AXRELinkage> GetLinkage() {
            // TODO just for ux development
            await Task.Delay(3000);

            return new AXRELinkage() { address = "0.0.0.0:56723" };

            /* checkEndpoint();

            using (var request = AXREApi.GetLinkageRequest(_endpointWithoutPath)) {
                try {
                    var data = await send(request);
                    return JsonUtility.FromJson<AXRELinkage>(data.text);
                }
                catch (AXREException e) {
                    if (e.code != AXREException.Code.HTTP) { throw e; }

                    switch (e.responseCode) {
                        case 404:
                            return null;
                        case 503:
                            throw new AXREException(AXREException.Code.Busy, e.responseCode, e.reason);
                        default:
                            throw e;
                    }
                }
            } */
        }

        private void checkEndpoint() {
            if (string.IsNullOrWhiteSpace(_endpointWithoutPath)) {
                throw new AXREException(AXREException.Code.InvalidEndpoint, 0, "endpoint is not set");
            }
        }

        private async Task<DownloadHandler> send(UnityWebRequest request) {
            var asyncop = request.SendWebRequest();
            while (asyncop.isDone == false) { await Task.Yield(); }

            if (string.IsNullOrEmpty(request.error)) {
                return request.downloadHandler;
            }

            switch (request.result) {
                case UnityWebRequest.Result.ProtocolError:
                    throw new AXREException(AXREException.Code.HTTP, request.responseCode, request.error);
                case UnityWebRequest.Result.ConnectionError:
                    throw new AXREException(AXREException.Code.Network, 0, request.error);
                default:
                    throw new AXREException(AXREException.Code.Unknown, request.responseCode, request.error);
            }
        }

        [Serializable]
        private class GetGroupsParser {
            public string[] groups = null;
        }

        [Serializable]
        private class JsonArrayParser<T> {
            public static T[] Parse(string json) {
                return JsonUtility.FromJson<JsonArrayParser<T>>($"{{\"array\":{json}}}").array;
            }

            public T[] array = null;
        }
    }
}
