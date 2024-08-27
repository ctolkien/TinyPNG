using System.Net.Http;
using System.Text;

namespace TinyPng;
internal class JsonContent(string content) :
    StringContent(content, Encoding.UTF8, "application/json")
{
}

