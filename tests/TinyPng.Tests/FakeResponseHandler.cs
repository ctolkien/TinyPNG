using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace TinyPng.Tests;

public class FakeResponseHandler : DelegatingHandler
{
    private readonly Dictionary<Uri, HttpResponseMessage> _fakeGetResponses = new();
    private readonly Dictionary<Uri, HttpResponseMessage> _fakePostResponses = new();


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
        var result = request.Method == HttpMethod.Get && _fakeGetResponses.ContainsKey(request.RequestUri)
            ? _fakeGetResponses[request.RequestUri]
            : request.Method == HttpMethod.Post && _fakePostResponses.ContainsKey(request.RequestUri)
            ? _fakePostResponses[request.RequestUri]
            : new HttpResponseMessage(HttpStatusCode.NotFound) { RequestMessage = request };

        return Task.FromResult(result);
    }
}
