using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AZM.Abyan.Identity.Domain.Enums
{
    public static class ResultStatus
    {
        public const int Success = 200;          // 200 OK
        public const int Created = 201;          // 201 Created
        public const int Updated = 200;          // 200 OK (or 204 No Content)
        public const int Deleted = 200;          // 200 OK (or 204 No Content)
        public const int NotFound = 404;         // 404 Not Found
        public const int ValidationFailed = 400; // 400 Bad Request
        public const int Unauthorized = 401;     // 401 Unauthorized
        public const int Conflict = 409;         // 409 Conflict
        public const int InvalidOperation = 400; // 400 Bad Request
    }
}
