using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PRN232.LMS.API.Swagger;

public sealed class SwaggerOperationNotesFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var route = context.ApiDescription.RelativePath ?? string.Empty;
        var httpMethod = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? string.Empty;
        var version = context.ApiDescription.GroupName ?? string.Empty;
        var authorization = GetAuthorizationInfo(context, route, version);

        operation.Description = BuildDescription(operation.Description, authorization, httpMethod, route, context);
        operation.Responses ??= [];

        if (authorization.RequiresJwt)
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", null!, null),
                    []
                }
            });

            operation.Responses.TryAdd("401", new OpenApiResponse
            {
                Description = "Chưa đăng nhập, token sai hoặc token hết hạn."
            });
        }

        if (authorization.IsAdminOnly)
        {
            operation.Responses.TryAdd("403", new OpenApiResponse
            {
                Description = "Đã đăng nhập nhưng role không đủ quyền. Dùng user để demo lỗi này."
            });
        }
    }

    private static AuthorizationInfo GetAuthorizationInfo(
        OperationFilterContext context,
        string route,
        string version)
    {
        if (route.Contains("/auth/", StringComparison.OrdinalIgnoreCase))
        {
            return new AuthorizationInfo(
                "Public",
                "Không cần access token. Login dùng username/password; refresh/revoke dùng refresh token trong body.",
                false,
                false);
        }

        if (string.Equals(version, "v1", StringComparison.OrdinalIgnoreCase))
        {
            return new AuthorizationInfo(
                "Public - Lab 1",
                "Không cần JWT để giữ đúng mục tiêu Lab 1: tập trung REST, query, response format và routing.",
                false,
                false);
        }

        var authorizeAttributes = GetAuthorizeAttributes(context);
        var isAdminOnly = authorizeAttributes.Any(attribute =>
            string.Equals(attribute.Policy, "Lab2Admin", StringComparison.OrdinalIgnoreCase) ||
            ContainsRole(attribute.Roles, "Admin"));

        if (isAdminOnly)
        {
            return new AuthorizationInfo(
                "Admin",
                "Cần JWT hợp lệ có role Admin. Dùng user gọi endpoint này để test lỗi 403 Forbidden.",
                true,
                true);
        }

        if (authorizeAttributes.Count > 0)
        {
            return new AuthorizationInfo(
                "User hoặc Admin",
                "Cần JWT hợp lệ. Role User đủ quyền đọc dữ liệu, role Admin cũng gọi được.",
                true,
                false);
        }

        return new AuthorizationInfo("Public", "Không yêu cầu đăng nhập.", false, false);
    }

    private static List<AuthorizeAttribute> GetAuthorizeAttributes(OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor descriptor)
        {
            return [];
        }

        var controllerAttributes = descriptor.ControllerTypeInfo
            .GetCustomAttributes(inherit: true)
            .OfType<AuthorizeAttribute>();

        var actionAttributes = descriptor.MethodInfo
            .GetCustomAttributes(inherit: true)
            .OfType<AuthorizeAttribute>();

        return controllerAttributes.Concat(actionAttributes).ToList();
    }

    private static string BuildDescription(
        string? currentDescription,
        AuthorizationInfo authorization,
        string httpMethod,
        string route,
        OperationFilterContext context)
    {
        var notes = new List<string>
        {
            "**Ghi chú demo**",
            $"- Quyền truy cập: **{authorization.Label}**.",
            $"- Ý nghĩa: {authorization.Explanation}"
        };

        if (authorization.RequiresJwt)
        {
            notes.Add("- Trên Swagger, bấm **Authorize** rồi chỉ dán `accessToken`; không nhập thêm chữ `Bearer`.");
        }

        if (httpMethod is "POST" or "PUT" or "DELETE")
        {
            notes.Add("- Đây là thao tác thay đổi dữ liệu, nên trong Lab 2 được dùng để minh họa authorization và lỗi 403.");
        }

        if (route.Contains("X-Request-Id", StringComparison.OrdinalIgnoreCase) ||
            context.ApiDescription.ParameterDescriptions.Any(parameter =>
                string.Equals(parameter.Name, "X-Request-Id", StringComparison.OrdinalIgnoreCase)))
        {
            notes.Add("- Header `X-Request-Id` là mã theo dõi request: gửi lên để API echo lại, giúp liên hệ request của client với log server.");
        }

        var noteBlock = string.Join(Environment.NewLine, notes);

        return string.IsNullOrWhiteSpace(currentDescription)
            ? noteBlock
            : $"{currentDescription}{Environment.NewLine}{Environment.NewLine}{noteBlock}";
    }

    private static bool ContainsRole(string? roles, string expectedRole)
    {
        return roles?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(role => string.Equals(role, expectedRole, StringComparison.OrdinalIgnoreCase)) == true;
    }

    private sealed record AuthorizationInfo(
        string Label,
        string Explanation,
        bool RequiresJwt,
        bool IsAdminOnly);
}
