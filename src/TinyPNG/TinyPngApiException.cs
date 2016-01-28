using System;

namespace TinyPng
{
    public class TinyPngApiException : Exception
    {
        public int StatusCode { get; private set; }
        public string StatusReasonPhrase { get; private set; }
        public string ErrorTitle { get; private set; }
        public string ErrorMessage { get; private set; }


        public TinyPngApiException(int statusCode, string statusReasonPhrase, string errorTitle, string errorMessage)
        {
            ErrorTitle = errorTitle;
            ErrorMessage = errorMessage;
            StatusCode = statusCode;
            StatusReasonPhrase = statusReasonPhrase;

            Data.Add(nameof(ErrorTitle), ErrorTitle);
            Data.Add(nameof(ErrorMessage), ErrorMessage);
            Data.Add(nameof(StatusCode), StatusCode);
            Data.Add(nameof(StatusReasonPhrase), StatusReasonPhrase);
        }

        public override string Message
        {
            get
            {
                return $"Api Service returned a non-success status code when attempting an operation on an image: {StatusCode} - {StatusReasonPhrase}. {ErrorTitle}, {ErrorMessage} ";
            }
        }

    }
}
