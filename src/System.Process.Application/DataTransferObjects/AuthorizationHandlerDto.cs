using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace System.Process.Application.DataTransferObjects
{
    public class AuthorizationHandlerDto
    {
        public string Body { get; set; }
        public QueryString Query { get; set; }
        public RouteData Route { get; set; }
    }
}
