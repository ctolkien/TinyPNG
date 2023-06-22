using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TinyPng.Tests;

public class FakeResponseHandler : DelegatingHandler
{
    private readonly Dictionary<Uri, HttpResponseMessage> _FakeGetResponses = new();
    private readonly Dictionary<Uri, HttpResponseMessage> _FakePostResponses = new();


    public void AddFakeGetResponse(Uri uri, HttpResponseMessage responseMessage)
    {
        _FakeGetResponses.Add(uri, responseMessage);
    }
    public void AddFakePostResponse(Uri uri, HttpResponseMessage responseMessage)
    {
        _FakePostResponses.Add(uri, responseMessage);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        var result = request.Method == HttpMethod.Get && _FakeGetResponses.ContainsKey(request.RequestUri)
            ? _FakeGetResponses[request.RequestUri]
            : request.Method == HttpMethod.Post && _FakePostResponses.ContainsKey(request.RequestUri)
            ? _FakePostResponses[request.RequestUri]
            : new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };

        return Task.FromResult(result);
    }
}
