using System.Net;
using System.Text;
using HttpServer;

namespace MediaPortal.Plugins.MP2Extended.ResourceAccess.BaseClasses
{
  class BaseHtmlHeader
  {
    internal void SendHeader(IHttpResponse response, int contentLength)
    {
      response.Status = HttpStatusCode.OK;
      response.Encoding = Encoding.UTF8;
      response.ContentType = "text/html; charset=UTF-8";
      response.ContentLength = contentLength;
      response.SendHeaders();
    }
  }
}
