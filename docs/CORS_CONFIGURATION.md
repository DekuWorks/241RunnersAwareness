# CORS Configuration for 241 Runners Awareness API

## Required CORS Settings

The API must be configured to allow requests from both www and non-www origins:

### Allowed Origins
```
https://241runnersawareness.org
https://www.241runnersawareness.org
```

### Allowed Methods
```
GET, POST, PUT, DELETE, OPTIONS
```

### Allowed Headers
```
Authorization
Content-Type
X-CSRF-Token
X-Client
Cache-Control
Pragma
Expires
```

### Credentials
```
AllowCredentials = true
```

## API Configuration (Program.cs)

Add this to your Program.cs:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins(
                "https://241runnersawareness.org",
                "https://www.241runnersawareness.org"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Use the policy
app.UseCors("AllowSpecificOrigins");
```

## Important Notes

1. **Never use `*` for origins when using `AllowCredentials = true`**
2. **Include both www and non-www versions**
3. **Ensure redirects also include CORS headers**
4. **Test with both origins to ensure compatibility**

## Testing CORS

Use this curl command to test CORS:

```bash
curl -H "Origin: https://241runnersawareness.org" \
     -H "Access-Control-Request-Method: POST" \
     -H "Access-Control-Request-Headers: Authorization,Content-Type" \
     -X OPTIONS \
     https://241runners-api.azurewebsites.net/api/Auth/login
```

Expected response headers:
```
Access-Control-Allow-Origin: https://241runnersawareness.org
Access-Control-Allow-Methods: GET,POST,PUT,DELETE,OPTIONS
Access-Control-Allow-Headers: Authorization,Content-Type,X-CSRF-Token,X-Client
Access-Control-Allow-Credentials: true
```
