﻿using atprotosharp.Exceptions;
using atprotosharp.Models;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace atprotosharp;
public class ATProtoClient
{
    private readonly string _serverUrl;
    private readonly HttpClient _httpClient;
    private Session? _session;

    #region Constructor

    /// <summary>
    /// Instantiate the API, connected to the server of your choice. By default connects to the BlueSky server
    /// </summary>
    /// <param name="httpClient">Use your own custom HTTPRest Client... if you wish... dunno why you'd do that tho...</param>
    /// <param name="serverUrl">The base url of the server you wish to connect to. Default: https://bsky.social</param>
    public ATProtoClient(IHttpClientFactory httpClientFactory, string serverUrl = Constants.DefaultServerUrl)
    {
        _httpClient = httpClientFactory.CreateClient();
        _serverUrl = serverUrl;
    }

    #endregion

    /// <summary>
    /// (Unauthenticated) Get the configuration of the server you're connecting with
    /// </summary>
    /// <returns>ServerConfig object with the information for that server.</returns>
    public async Task<ServerDescription?> GetServerParameters()
    {
        var result = await _httpClient.HttpGetAsync(_serverUrl + Constants.Endpoints.DescribeServer);
        if (!result.success)
            return null;

        try
        {
            result = JObject.Parse(result.responseBody);
            result.serverUrl = _serverUrl;
            result.success = true;
            return result;
        }
        catch (Exception ex)
        {
            throw new ATProtocolException("Unable to parse DescribeServer response", ex);
        }
    }

    /// <summary>
    /// Connects you to the server using your account details
    /// </summary>
    /// <param name="username">your username</param>
    /// <param name="password">your password</param>
    public async Task<bool> LoginAsync(string username, string password)
    {
        _session = await CreateSession(username, password);
        if (_session is null)
            return false;

        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _session?.AccessJwt);

        return true;
    }

    /// <summary>
    /// Returns the session object for your provided account after authentication. If no authentication this should throw an error
    /// </summary>
    /// <returns>AuthenticationResponse object with the information for your particular session</returns>
    public async Task<Session?> GetSession(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(_serverUrl + Constants.Endpoints.GetSession, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<Session>(stream, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            throw new ATProtocolException("Unable to parse GetSession response", ex);
        }
    }

    /// <summary>
    /// Get a user account by did
    /// </summary>
    /// <param name="did">the did for the profile you want to get. ex did:pfc:whatever43</param>
    /// <returns>UserProfile model contain the details for that user</returns>
    public async Task<Profile?> GetProfileByDid(string did, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(_serverUrl + Constants.Endpoints.GetProfile + "?actor=" + did, cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        try
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<Profile>(stream, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex)
        {
            throw new ATProtocolException("Unable to parse GetSession response", ex);
        }
    }

    /// <summary>
    /// Gets the timeline of the logged in user.
    /// </summary>
    /// <param name="algorithm">The order of the timeline. Possible: reverse-chronological</param>
    /// <param name="limit">How many posts to get. Default limit is 30</param>
    /// <returns>A dynamic ExpandoObject with whatever the server gives us.</returns>
    public async Task<Timeline?> GetTimeline(string algorithm, int limit = 30, CancellationToken cancellationToken = default)
    {
        var requestUrl = new StringBuilder($"{_serverUrl}{Constants.Endpoints.GetTimeline}?");
        if (!string.IsNullOrEmpty(algorithm))
            requestUrl.Append($"algorithm={algorithm}&");

        requestUrl.Append($"limit={limit}");

        using var response = await  _httpClient.GetAsync(requestUrl.ToString(), cancellationToken);
        if (response.IsSuccessStatusCode)
            return null;

        try 
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<Timeline>(stream, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex) 
        {
            throw new ATProtocolException("Unable to parse GetSession response", ex);
        }
    }

    /// <summary>
    /// Creates a new post to be inserted into the user's timeline
    /// </summary>
    /// <param name="postText">The text of the post</param>
    /// <param name="filePaths">The paths of the files you want to upload on your filesystem</param>
    /// <returns>A dynamic ExpandoObject with whatever the server gives us.</returns>    
    public async Task<dynamic> CreatePost(string postText, params string[] filePaths)
    {
        throw new NotImplementedException();

        /*var result = GenerateResult(false, "You need to be logged in in order to conduct this operation. Type \"login\" in the terminal");

        if (AutorizationHeader() == null)
            return result;

        List<dynamic> uploadedFiles = new List<dynamic>();
        if (filePaths.Length > 0)
        {
            foreach (var filePath in filePaths)
            {
                var uploadMediaResult = await UploadMedia(filePath);
                if ((bool)uploadMediaResult.success)
                {
                    uploadedFiles.Add(uploadMediaResult);
                }
                else
                {
                    result.success = uploadMediaResult.success;
                    result.error = $"Could not upload file: {filePath} ERROR: " + uploadMediaResult.error;
                    return result;
                }
            }
        }

        dynamic payload = new JObject(
            new JProperty("collection", "app.bsky.feed.post"),
            new JProperty("repo", _session?.did),
            new JProperty("record", new JObject(
                new JProperty("text", postText),
                new JProperty("createdAt", DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")),
                new JProperty("$type", "app.bsky.feed.post")
            ))
        );

        if (uploadedFiles.Count > 0)
        {
            JArray imagesArray = new JArray();

            foreach (var image in uploadedFiles)
            {
                JObject imageObject = new JObject(
                    new JProperty("image", new JObject(
                        new JProperty("$type", "blob"),
                        new JProperty("ref", new JObject(
                            new JProperty("$link", image.blob["ref"]["$link"])
                        )),
                        new JProperty("mimeType", image.blob["mimeType"]),
                        new JProperty("size", image.blob["size"])
                    )),
                    new JProperty("alt", "")
                );

                imagesArray.Add(imageObject);
            }

            JObject embed = new JObject(
           new JProperty("$type", "app.bsky.embed.images"),
           new JProperty("images", imagesArray)
           );

            payload.record["embed"] = embed;
        }

        result = await _httpClient.HttpPostAsync(
            _serverUrl + Constants.Endpoints.CreateRecord,
            new StringContent(payload.ToString(), Encoding.UTF8, "application/json"),
            AutorizationHeader()
            );

        if (!result.success)
            return result;
        result = JObject.Parse(result.responseBody);
        result.success = true;
        return result;*/

        /* FOR REPLIES LATER
        if (your condition for adding reply)
        {
            JObject reply = new JObject(
                new JProperty("root", new JObject(
                    new JProperty("uri", "at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujolygxxv2s"),
                    new JProperty("cid", "bafyreiaxmyczlndo2ftzutecptuxkvg3v6lokrl2bseo4vpdvxvptxejk4")
                )),
                new JProperty("parent", new JObject(
                    new JProperty("uri", "at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujolygxxv2s"),
                    new JProperty("cid", "bafyreiaxmyczlndo2ftzutecptuxkvg3v6lokrl2bseo4vpdvxvptxejk4")
                ))
            );

            obj.record["reply"] = reply;
        } */

        // Creating a record with no picture and it's not a reply to antyhing.
        // Request URL: https://bsky.social/xrpc/com.atproto.repo.createRecord
        // Request Method: POST
        // WITH AUTHORIZATION
        // PAYLOAD: {"collection":"app.bsky.feed.post","repo":"did:plc:w7l7x7fvogbwyeplxx4duu4s","record":{"text":"Test. Please do your utmost to ignore. I'm just trying to see how the API works for posting stuff.","createdAt":"2023-04-29T17:44:22.566Z","$type":"app.bsky.feed.post"}}
        // REPLIES WITH - basically the URL to the post you've made. WOO!
        // {"uri":"at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujolygxxv2s","cid":"bafyreiaxmyczlndo2ftzutecptuxkvg3v6lokrl2bseo4vpdvxvptxejk4"}

        // Then, for picture, first it uploads the image as a blobady blob
        // Request URL: https://bsky.social/xrpc/com.atproto.repo.uploadBlob
        // Request Method: POST
        // WITH AUTHORIZATION
        // IMPORTANT! Content-Type: image/jpeg
        // And the payload is literally the image, no surprise there.
        // REPLIES WITH - so you know where to get the image from
        // {"blob":{"$type":"blob","ref":{"$link":"bafkreiaxzorv673roo6e2na25v6uo5mjoixlhmqs5kbos65h6hbqqg72te"},"mimeType":"image/jpeg","size":323356}}
        // This endpoint is interesting as it should allow video uploads

        // Creating a record with the picture and it's a reply to the previous post
        // Request URL: https://bsky.social/xrpc/com.atproto.repo.createRecord
        // Request Method: POST
        // WITH AUTHORIZATION
        // PAYLOAD: {"collection":"app.bsky.feed.post","repo":"did:plc:w7l7x7fvogbwyeplxx4duu4s","record":{"text":"Whops, should have created one with a picture too, and a reply! SO MUCH STUFF!","reply":{"root":{"uri":"at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujolygxxv2s","cid":"bafyreiaxmyczlndo2ftzutecptuxkvg3v6lokrl2bseo4vpdvxvptxejk4"},"parent":{"uri":"at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujolygxxv2s","cid":"bafyreiaxmyczlndo2ftzutecptuxkvg3v6lokrl2bseo4vpdvxvptxejk4"}},"embed":{"$type":"app.bsky.embed.images","images":[{"image":{"$type":"blob","ref":{"$link":"bafkreiaxzorv673roo6e2na25v6uo5mjoixlhmqs5kbos65h6hbqqg72te"},"mimeType":"image/jpeg","size":323356},"alt":""}]},"createdAt":"2023-04-29T17:45:37.651Z","$type":"app.bsky.feed.post"}}
        // REPLIES WITH - basically the URL to the post you've made. WOO!
        // {"uri":"at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujooa3v622h","cid":"bafyreidlgwtbp57vg752qqdtvnj4a6izauf4fk7piv6nofu7jionfjiauy"}

        // Creating a third record to see the difference between root and parrent in the JSON payload
        // {"collection":"app.bsky.feed.post","repo":"did:plc:w7l7x7fvogbwyeplxx4duu4s","record":{"text":"Okay one more attempt to quadriple check something","reply":{"root":{"cid":"bafyreiaxmyczlndo2ftzutecptuxkvg3v6lokrl2bseo4vpdvxvptxejk4","uri":"at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujolygxxv2s"},"parent":{"uri":"at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujooa3v622h","cid":"bafyreidlgwtbp57vg752qqdtvnj4a6izauf4fk7piv6nofu7jionfjiauy"}},"createdAt":"2023-04-29T17:54:04.051Z","$type":"app.bsky.feed.post"}}
        // Yep the difference is obvious. The root is the very first post. The parrent is the immediatly previous post that we're linking this post to

        // When uploading multiple files, the embeded section for images contains an array of items all images
        // {"collection":"app.bsky.feed.post","repo":"did:plc:w7l7x7fvogbwyeplxx4duu4s","record":{"text":"Ignore the teeest. It is... JUST A TEEEEST","reply":{"root":{"cid":"bafyreiaxmyczlndo2ftzutecptuxkvg3v6lokrl2bseo4vpdvxvptxejk4","uri":"at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujolygxxv2s"},"parent":{"uri":"at://did:plc:w7l7x7fvogbwyeplxx4duu4s/app.bsky.feed.post/3jujp5cwwr22h","cid":"bafyreiffiwuwk2sn77uiuzv7avtlwovttadcwbqzzbgynwwkb6m6jtye3i"}},"embed":{"$type":"app.bsky.embed.images","images":[{"image":{"$type":"blob","ref":{"$link":"bafkreid3ktlmryarvwsoq2domfzo74xzcm6eotdu6xqfgvsas4524kwj4m"},"mimeType":"image/jpeg","size":57667},"alt":""},{"image":{"$type":"blob","ref":{"$link":"bafkreibs355urdaau5xhd7gxsf4o2tuc5lv3unpegcb4vqmwtednwzpn7i"},"mimeType":"image/jpeg","size":84135},"alt":""},{"image":{"$type":"blob","ref":{"$link":"bafkreihokjpzo76tl4wkufrfjiwpcip6pd6f5mlxrrojwnphckjy2rw4e4"},"mimeType":"image/jpeg","size":25104},"alt":""}]},"createdAt":"2023-04-30T02:42:04.404Z","$type":"app.bsky.feed.post"}}   
    }

    /// <summary>
    /// Uploads a file to the Bsky server
    /// </summary>
    /// <param name="filePath">The path to the file to upload</param>
    /// <returns>A body with the ID / path of the file in the storage cont</returns>
    public async Task<dynamic?> UploadMedia(string filePath, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(filePath))
            throw new ATProtocolException("Unable to parse GetSession response");

        try
        {
            using var file = File.OpenRead(filePath);
            using var content = new StreamContent(file);
            content.Headers.ContentType = new MediaTypeHeaderValue(MimeTypes.GetMimeType(filePath));
            using var response = await _httpClient.PostAsync(_serverUrl + Constants.Endpoints.UploadBlob, content, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var result = await JsonSerializer.DeserializeAsync<dynamic>(stream, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex) 
        {
            throw new ATProtocolException("Unable to parse GetSession response", ex);
        }
    }

    /// <summary>
    /// Gets all the invite codes availabile to the signed-in account
    /// </summary>
    /// <returns>AccountInviteCodesResult mode containing all the invite codes (used, unused, and if used by which did)</returns>
    public async Task<List<InviteCode>?> GetAccountInviteCodes(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync(_serverUrl + Constants.Endpoints.GetAccountInviteCodes, cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        try 
        {
            await using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<List<InviteCode>>(stream, cancellationToken: cancellationToken);
            return result;
        } 
        catch (Exception ex) 
        {
            throw new ATProtocolException("Unable to parse GetSession response", ex);
        }
    }

    /// <summary>
    /// Gets the handle of the account you're signed in as.
    /// </summary>
    /// <returns>The handle of the account you're signed in as. Returns null if not signed in.</returns>
    public string? GetMyHandle()
    {
        return _session?.Handle;
    }

    #region Privates
    /// <summary>
    /// Creates a new session with the server so you can do things.
    /// </summary>
    /// <param name="identifier">your username</param>
    /// <param name="password">your password</param>
    /// <returns>The response to your authentication attempt</returns>
    private async Task<Session?> CreateSession(string identifier, string password, CancellationToken cancellationToken = default)
    {
        var requestPayload = JsonSerializer.Serialize(new AuthenticationRequest()
        {
            Identifier = identifier,
            Password = password
        });

        using var response = await _httpClient.PostAsync(_serverUrl + Constants.Endpoints.CreateSession, new StringContent(requestPayload, Encoding.UTF8, "application/json"), cancellationToken: cancellationToken);
        if (!response.IsSuccessStatusCode)
            return null;

        try 
        {
            await using var stream = await response.Content.ReadAsStreamAsync();
            var result = await JsonSerializer.DeserializeAsync<Session>(stream, cancellationToken: cancellationToken);
            return result;
        }
        catch (Exception ex) 
        {
            throw new ATProtocolException("Unable to parse GetSession response", ex);
        }
    }

    /// <summary>
    /// Create a dynamic result object to provide cleaner code in the methods
    /// </summary>
    private dynamic GenerateResult(bool success, string? error = null)
    {
        dynamic result = new ExpandoObject();
        result.success = success;
        if (error != null)
            result.error = error;
        return result;
    }

    #endregion
}

