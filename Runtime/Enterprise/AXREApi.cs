using System;
using System.Net.Mime;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace onAirXR.Client {
    public static class AXREApi {
        public static UnityWebRequest GetContentsRequest(string endpointWithoutPath) {
            return UnityWebRequest.Get(endpoint(endpointWithoutPath, "contents"));
        }

        public static UnityWebRequest GetCompositeContentsRequest(string endpointWithoutPath) {
            return UnityWebRequest.Get(endpoint(endpointWithoutPath, "composite", "contents"));
        }

        public static UnityWebRequest GetGroupsRequest(string endpointWithoutPath) {
            return UnityWebRequest.Get(endpoint(endpointWithoutPath, "groups"));
        }

        public static UnityWebRequest GetGroupRequest(string endpointWithoutPath, string groupId) {
            return UnityWebRequest.Get(endpoint(endpointWithoutPath, "groups", groupId));
        }

        public static UnityWebRequest CreateGroupRequest(string endpointWithoutPath, string contentId) {
            return new UnityWebRequest(endpoint(endpointWithoutPath, "groups"), UnityWebRequest.kHttpVerbPOST) {
                uploadHandler = new UploadHandlerRaw(new UTF8Encoding().GetBytes(JsonUtility.ToJson(new CreateGroupArgs {
                    content_id = contentId,
                    instance_id = contentId
                }))) {
                    contentType = MediaTypeNames.Application.Json
                },
                downloadHandler = new DownloadHandlerBuffer()
            };
        }

        public static UnityWebRequest DeleteGroupRequest(string endpointWithoutPath, string groupId) {
            return UnityWebRequest.Delete(endpoint(endpointWithoutPath, "groups", groupId));
        }

        public static UnityWebRequest DeleteAllGroupsRequest(string endpointWithoutPath) {
            return UnityWebRequest.Delete(endpoint(endpointWithoutPath, "groups"));
        }

        public static UnityWebRequest GetLinkageRequest(string endpointWithoutPath) {
            return UnityWebRequest.Get(endpoint(endpointWithoutPath, "linkage"));
        }

        public static UnityWebRequest PutGroupCommandRequest(string endpointWithoutPath, string command) {
            return new UnityWebRequest(endpoint(endpointWithoutPath, "content"), UnityWebRequest.kHttpVerbPUT) {
                uploadHandler = new UploadHandlerRaw(new UTF8Encoding().GetBytes(JsonUtility.ToJson(new PutGroupCommandArgs {
                    Command = command
                }))) {
                    contentType = MediaTypeNames.Application.Json
                },
                downloadHandler = new DownloadHandlerBuffer()
            };
        }

        public static UnityWebRequest GetTextureRequest(string endpointWithoutPath, string path) {
            return UnityWebRequestTexture.GetTexture(endpoint(endpointWithoutPath, path));
        }

        private static string endpoint(string endpointWithoutPath, params string[] path) => $"{endpointWithoutPath}/{string.Join("/", path)}";

        [Serializable]
        private struct CreateGroupArgs {
            public string content_id;
            public string instance_id;
        }

        [Serializable]
        private struct PutGroupCommandArgs {
            public string Command;
        }
    }

    #pragma warning disable 0649

    [Serializable]
    public class AXREContent {
        [SerializeField] private Repository repository;

        public string id;
        public string title;
        public string version;
        public string author;
        public string description;
        public string[] thumbnails;
        public string[] screenshots;
        
        public string name => repository.name;
        public string thumbnail => (thumbnails?.Length ?? 0) > 0 ? thumbnails[0] : null;

        [Serializable]
        private struct Repository {
            public string name;
        }
    }

    [Serializable]
    public class AXREGroup {
        public string group_id;
        public string content_id;
        public string access_point;
    }

    [Serializable]
    public class AXRELinkage {
        public string address;

        public string ipaddr {
            get {
                if (string.IsNullOrEmpty(address)) { return null; }

                var tokens = address.Split(':');
                if (tokens.Length != 2) { return null; }

                return tokens[0];
            }
        }

        public int port {
            get {
                if (string.IsNullOrEmpty(address)) { return -1; }

                var tokens = address.Split(':');
                if (tokens.Length != 2) { return -1; }

                return int.TryParse(tokens[1], out int result) ? result : -1;
            }
        }
    }
    
    #pragma warning restore 0649
}
