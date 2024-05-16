using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TinyPng.Tests;

public class FakeResponseHandler : DelegatingHandler
{
    private readonly Dictionary<Uri, HttpResponseMessage> _fakeGetResponses = [];
    private readonly Dictionary<Uri, HttpResponseMessage> _fakePostResponses = [];


    public void AddFakeGetResponse(Uri uri, HttpResponseMessage responseMessage)
    {
        _fakeGetResponses.Add(uri, responseMessage);
    }
    public void AddFakePostResponse(Uri uri, HttpResponseMessage responseMessage)
    {
        _fakePostResponses.Add(uri, responseMessage);
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, System.Threading.CancellationToken cancellationToken)
    {
        if (request.Method == HttpMethod.Get && _fakeGetResponses.TryGetValue(request.RequestUri, out var getMessage)) { return Task.FromResult(getMessage); }
        else if (request.Method == HttpMethod.Post && _fakePostResponses.TryGetValue(request.RequestUri, out var postMessage)) { return Task.FromResult(postMessage); }
        else { return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request }); }
    }
}
