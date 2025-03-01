using OpenFga.Sdk.Client;
using OpenFga.Sdk.Client.Model;
using System.Text.Json;

namespace DocumentManagement.Services
{
    public class OpenFgaService
    {
        private readonly OpenFgaClient _client;
        private readonly string _storeId;
        private readonly string _modelId;

        public OpenFgaService(IConfiguration configuration)
        {
            _storeId = configuration["OpenFga:StoreId"] ?? throw new ArgumentNullException("OpenFga:StoreId is missing in the configuration.");
            _modelId = configuration["OpenFga:ModelId"] ?? throw new ArgumentNullException("OpenFga:ModelId is missing in the configuration.");
            var apiUrl = configuration["OpenFga:ApiUrl"] ?? throw new ArgumentNullException("OpenFga:ApiUrl is missing in the configuration.");

            var clientConfiguration = new ClientConfiguration
            {
                ApiUrl = apiUrl,
                StoreId = _storeId,
                AuthorizationModelId = _modelId
            };

            _client = new OpenFgaClient(clientConfiguration);
        }


        public async Task InitializeAuthorizationModel()
        {
            var modelJson = "{\r\n  \"schema_version\": \"1.1\",\r\n  \"type_definitions\": [\r\n    {\r\n      \"type\": \"user\",\r\n      \"relations\": {},\r\n      \"metadata\": null\r\n    },\r\n    {\r\n      \"type\": \"document\",\r\n      \"relations\": {\r\n        \"can_share\": {\r\n          \"union\": {\r\n            \"child\": [\r\n              {\r\n                \"this\": {}\r\n              },\r\n              {\r\n                \"computedUserset\": {\r\n                  \"object\": \"\",\r\n                  \"relation\": \"owner\"\r\n                }\r\n              }\r\n            ]\r\n          }\r\n        },\r\n        \"owner\": {\r\n          \"this\": {}\r\n        },\r\n        \"reader\": {\r\n          \"union\": {\r\n            \"child\": [\r\n              {\r\n                \"this\": {}\r\n              },\r\n              {\r\n                \"computedUserset\": {\r\n                  \"object\": \"\",\r\n                  \"relation\": \"owner\"\r\n                }\r\n              }\r\n            ]\r\n          }\r\n        },\r\n        \"writer\": {\r\n          \"union\": {\r\n            \"child\": [\r\n              {\r\n                \"this\": {}\r\n              },\r\n              {\r\n                \"computedUserset\": {\r\n                  \"object\": \"\",\r\n                  \"relation\": \"owner\"\r\n                }\r\n              }\r\n            ]\r\n          }\r\n        }\r\n      },\r\n      \"metadata\": {\r\n        \"relations\": {\r\n          \"can_share\": {\r\n            \"directly_related_user_types\": [\r\n              {\r\n                \"type\": \"user\",\r\n                \"condition\": \"\"\r\n              }\r\n            ],\r\n            \"module\": \"\",\r\n            \"source_info\": null\r\n          },\r\n          \"owner\": {\r\n            \"directly_related_user_types\": [\r\n              {\r\n                \"type\": \"user\",\r\n                \"condition\": \"\"\r\n              }\r\n            ],\r\n            \"module\": \"\",\r\n            \"source_info\": null\r\n          },\r\n          \"reader\": {\r\n            \"directly_related_user_types\": [\r\n              {\r\n                \"type\": \"user\",\r\n                \"condition\": \"\"\r\n              }\r\n            ],\r\n            \"module\": \"\",\r\n            \"source_info\": null\r\n          },\r\n          \"writer\": {\r\n            \"directly_related_user_types\": [\r\n              {\r\n                \"type\": \"user\",\r\n                \"condition\": \"\"\r\n              }\r\n            ],\r\n            \"module\": \"\",\r\n            \"source_info\": null\r\n          }\r\n        },\r\n        \"module\": \"\",\r\n        \"source_info\": null\r\n      }\r\n    }\r\n  ],\r\n  \"conditions\": {}\r\n}";

            var body = JsonSerializer.Deserialize<ClientWriteAuthorizationModelRequest>(modelJson);

            await _client.WriteAuthorizationModel(body);
        }

        public async Task AddDocumentOwner(string documentId, string userId)
        {
            var writeRequest = new ClientWriteRequest()
            {
                Writes = new List<ClientTupleKey>() {
                    new() {
                            User = $"user:{userId}",
                            Relation = "owner",
                            Object = $"document:{documentId}"
                          }
                    },
            };

            await _client.Write(writeRequest);
        }

        public async Task ShareDocumentWithUser(string documentId, string userId, string permission)
        {
            var relation = permission switch
            {
                Models.Permission.Read => "reader",
                Models.Permission.Write => "writer",
                _ => throw new ArgumentException($"Unsupported permission: {permission}")
            };

            var writeRequest = new ClientWriteRequest()
            {
                Writes = new List<ClientTupleKey>() {
                    new() {
                            User = $"user:{userId}",
                            Relation = relation,
                            Object = $"document:{documentId}"
                          }
                    },
            };

            await _client.Write(writeRequest);
        }

        public async Task RevokeUserAccess(string documentId, string userId, string permission)
        {
            var relation = permission switch
            {
                Models.Permission.Read => "reader",
                Models.Permission.Write => "writer",
                _ => throw new ArgumentException($"Unsupported permission: {permission}")
            };

            var writeRequest = new ClientWriteRequest()
            {
                Deletes = new List<ClientTupleKeyWithoutCondition>() {
                new() {
                      User = $"user:{userId}",
                      Relation = relation,
                      Object = $"document:{documentId}"
                }
              },
            };

            await _client.Write(writeRequest);
        }

        public async Task<bool> CheckUserPermission(string userId, string documentId, string permission)
        {
            var relation = permission switch
            {
                Models.Permission.Read => "reader",
                Models.Permission.Write => "writer",
                Models.Permission.Delete => "owner",
                Models.Permission.Share => "owner", 
                _ => throw new ArgumentException($"Unsupported permission: {permission}")
            };
          

            var checkRequest = new ClientCheckRequest
            {
                User = $"user:{userId}",
                Relation = relation,
                Object = $"document:{documentId}"
            };

            var response = await _client.Check(checkRequest);
            return response?.Allowed ?? false;
        }
    }
}
