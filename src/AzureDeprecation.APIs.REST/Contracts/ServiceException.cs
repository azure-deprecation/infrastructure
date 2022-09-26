using System.Net;

namespace AzureDeprecation.APIs.REST.Contracts;

public class ServiceException : Exception
{
    public HttpStatusCode HttpStatusCode { get; set; }
}