﻿﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using RestSharp;
﻿using RestSharp.Deserializers;
﻿using SalesforceSharp.Responses;

namespace SalesforceSharp
{
    public interface ISalesforceRestService
    {
        /// <summary>
        /// Creates an object in Salesforce.
        /// http://www.salesforce.com/us/developer/docs/api_rest/Content/dome_sobject_create.htm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">Salesforce object to be created.</param>
        /// <returns></returns>
        AddResponse Add<T>(object t) where T : new();

        /// <summary>
        /// Get an Salesforce object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The id of the Salesforce object to be retrieved.</param>
        /// <returns></returns>
        SalesforceResponse<T> Get<T>(string id) where T : new();

        /// <summary>
        /// Updates an object in Salesforce.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">Salesforce object to be updated.</param>
        /// <param name="id"></param>
        /// <returns></returns>
        SalesforceResponse Update<T>(object t, string id);

        /// <summary>
        /// Deletes an Salesforce object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The id of the Salesforce object to be deleted.</param>
        /// <returns></returns>
        SalesforceResponse Delete<T>(string id) where T : new();

        /// <summary>
        /// Execute a SOQL Query
        /// </summary>
        /// <param name="query">SOQL query</param>
        /// <returns></returns>
        string Query(string query);

        /// <summary>
        /// Execute a SOQL Query
        /// </summary>
        /// <param name="query">SOQL query</param>
        /// <returns></returns>
        QueryResponse<T> Query<T>(string query) where T : new();

        /// <summary>
        /// Gets available API the versions.
        /// </summary>
        /// <returns></returns>
        SalesforceResponse<List<ApiVersion>> GetVersions();

        /// <summary>
        /// Completely describes the individual metadata at all levels for the specified object. 
        /// For example, this can be used to retrieve the fields, URLs, and child relationships for the Account object.
        /// </summary>
        /// <param name="name">The Salesforce object name.</param>
        /// <returns></returns>
        string DescribeJson(string name);

        /// <summary>
        /// Completely describes the individual metadata at all levels for the specified object. 
        /// For example, this can be used to retrieve the fields, URLs, and child relationships for the Account object.
        /// </summary>
        /// <param name="name">The Salesforce object name.</param>
        /// <returns></returns>
        DescribeResponse Describe(string name);

        /// <summary>
        /// Lists the available objects and their metadata for your organization's data. 
        /// In addition, it provides the organization encoding, as well as maximum batch size permitted in queries
        /// </summary>
        /// <returns></returns>
        DescribeGlobalResponse DescribeGlobal();
    }

    public class SalesforceRestService : ISalesforceRestService
    {
        protected const string DefaultBaseUrl = "https://login.salesforce.com";
        protected const string DefaultVersion = "v29.0";

        public string AccessToken { get; private set; }
        public string InstanceUrl { get; private set; }
        public string Version { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SalesforceRestService" /> class using OAuth Refresh Token.
        /// </summary>
        /// <param name="consumerKey">The consumer key.</param>
        /// <param name="consumerSecret">The consumer secret.</param>
        /// <param name="refreshToken">The OAuth refresh token.</param>
        /// <param name="version">API version to be used.</param>
        /// <param name="baseUrl">The Salesforce base URL.</param>
        public SalesforceRestService(string consumerKey, string consumerSecret, string refreshToken, string version = DefaultVersion, string baseUrl = DefaultBaseUrl)
        {
            // Create the RefreshToken request.
            IRestRequest request = new RestRequest
                                   {
                                       Resource = "/services/oauth2/token",
                                       Method = Method.POST
                                   };

            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("client_id", consumerKey);
            request.AddParameter("client_secret", consumerSecret);
            request.AddParameter("refresh_token", refreshToken);

            IRestClient client = new RestClient();
            client.BaseUrl = baseUrl;
            var response = client.Execute<RefreshTokenResponse>(request);

            if (response.ErrorException != null)
            {
                Debug.WriteLine(string.Format("StatusCode={0}; Message={1}; AccessToken=null", response.StatusCode, response.ErrorMessage));
                return;
            }

            response.Data.StatusCode = response.StatusCode;
            Debug.WriteLine(response.Data.ToString());

            AccessToken = response.Data.AccessToken;
            InstanceUrl = response.Data.InstanceUrl;
            Version = version;
        }

        /// <summary>
        /// Creates an object in Salesforce.
        /// http://www.salesforce.com/us/developer/docs/api_rest/Content/dome_sobject_create.htm
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">Salesforce object to be created.</param>
        /// <returns></returns>
        public AddResponse Add<T>(object t) where T : new()
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/sobjects/{1}", Version, typeof(T).Name),
                Method = Method.POST,
                RequestFormat = DataFormat.Json
            };
            request.AddBody(t);
            var response = ExecuteRequest<AddResponse>(request);
            return response.Data;
        }

        /// <summary>
        /// Get an Salesforce object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The id of the Salesforce object to be retrieved.</param>
        /// <returns></returns>
        public SalesforceResponse<T> Get<T>(string id) where T : new()
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/sobjects/{1}/{2}", Version, typeof(T).Name, id),
                Method = Method.GET
            };
            return ExecuteRequest<T>(request);
        }

        /// <summary>
        /// Updates an object in Salesforce.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t">Salesforce object to be updated.</param>
        /// <param name="id"></param>
        /// <returns></returns>
        public SalesforceResponse Update<T>(object t, string id)
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/sobjects/{1}/{2}", Version, typeof(T).Name, id),
                Method = Method.PATCH,
                RequestFormat = DataFormat.Json
            };
            request.AddBody(t);
            return ExecuteRequest<SalesforceResponse>(request);
        }

        /// <summary>
        /// Deletes an Salesforce object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="id">The id of the Salesforce object to be deleted.</param>
        /// <returns></returns>
        public SalesforceResponse Delete<T>(string id) where T : new()
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/sobjects/{1}/{2}", Version, typeof(T).Name, id),
                Method = Method.DELETE
            };
            return ExecuteRequest<SalesforceResponse>(request);
        }

        /// <summary>
        /// Execute a SOQL Query
        /// </summary>
        /// <param name="query">SOQL query</param>
        /// <returns></returns>
        public string Query(string query)
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/query/?q={1}", Version, query),
                Method = Method.GET
            };

            return ExecuteRequest(request);
        }

        /// <summary>
        /// Execute a SOQL Query
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="query">SOQL query</param>
        /// <returns></returns>
        public QueryResponse<T> Query<T>(string query) where T : new()
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/query/?q={1}", Version, query),
                Method = Method.GET
            };

            var response = ExecuteRequest<QueryResponse<T>>(request);
            return response.Data;
        }

        /// <summary>
        /// Gets available API the versions.
        /// </summary>
        /// <returns></returns>
        public SalesforceResponse<List<ApiVersion>> GetVersions()
        {
            IRestRequest request = new RestRequest
            {
                Resource = "/services/data/",
                Method = Method.GET
            };
            return ExecuteRequest<List<ApiVersion>>(request);
        }

        /// <summary>
        /// Completely describes the individual metadata at all levels for the specified object.
        /// For example, this can be used to retrieve the fields, URLs, and child relationships for the Account object.
        /// </summary>
        /// <param name="name">The Salesforce object name.</param>
        /// <returns></returns>
        public string DescribeJson(string name)
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/sobjects/{1}/describe/", Version, name),
                Method = Method.GET
            };

            return ExecuteRequest(request);
        }

        /// <summary>
        /// Completely describes the individual metadata at all levels for the specified object.
        /// For example, this can be used to retrieve the fields, URLs, and child relationships for the Account object.
        /// </summary>
        /// <param name="name">The Salesforce object name.</param>
        /// <returns></returns>
        public DescribeResponse Describe(string name)
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/sobjects/{1}/describe/", Version, name),
                Method = Method.GET
            };

            var response = ExecuteRequest<DescribeResponse>(request);
            return response.Data;
        }

        /// <summary>
        /// Lists the available objects and their metadata for your organization's data.
        /// In addition, it provides the organization encoding, as well as maximum batch size permitted in queries
        /// </summary>
        /// <returns></returns>
        public DescribeGlobalResponse DescribeGlobal()
        {
            IRestRequest request = new RestRequest
            {
                Resource = string.Format("/services/data/{0}/sobjects/", Version),
                Method = Method.GET
            };

            var response = ExecuteRequest<DescribeGlobalResponse>(request);
            return response.Data;
        }

        private string ExecuteRequest(IRestRequest request)
        {
            if (request == null) throw new ArgumentException("request");

            request.AddHeader("Authorization", "Bearer " + AccessToken);

            IRestClient client = new RestClient();
            client.BaseUrl = InstanceUrl;
            var response = client.Execute(request);

            if (response.ErrorException != null)
            {
                Debug.WriteLine(response.ErrorMessage);
                throw response.ErrorException;
            }

            return response.Content;
        }

        private SalesforceResponse<T> ExecuteRequest<T>(IRestRequest request) where T : new()
        {
            request.AddHeader("Authorization", "Bearer " + AccessToken);

            IRestClient client = new RestClient();
            client.BaseUrl = InstanceUrl;
            var response = client.Execute<T>(request);

            var salesforceResponse = new SalesforceResponse<T>
            {
                Data = response.Data,
                StatusCode = response.StatusCode
            };

            if (response.ErrorException != null)
            {
                // Sets the error information
                var deserializer = new JsonDeserializer();
                var errors = deserializer.Deserialize<List<SalesforceResponse>>(response);
                if (errors.Count > 0)
                {
                    salesforceResponse.ErrorCode = errors[0].ErrorCode;
                    salesforceResponse.Message = errors[0].Message;
                }

                // If data response is a SalesforceResponse type, 
                // then instantiate it so that we can set HttpStatus and errors info.
                if (typeof (T).IsSubclassOf(typeof (SalesforceResponse)))
                {
                    salesforceResponse.Data = Activator.CreateInstance<T>();
                }
            }

            // If data is a SalesforceResponse than set its error and status properties.
            var data = salesforceResponse.Data as SalesforceResponse;
            if (salesforceResponse.Data is SalesforceResponse)
            {
                data.ErrorCode = salesforceResponse.ErrorCode;
                data.Message = salesforceResponse.Message;
                data.StatusCode = salesforceResponse.StatusCode;
            }

            Debug.WriteLine(salesforceResponse);
            return salesforceResponse;
        }
    }
}
