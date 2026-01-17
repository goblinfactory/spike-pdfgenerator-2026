# Security Analysis: /data/{fileName} Endpoint

## Question
Can `GET /data/{fileName}` be hacked to access files outside the PDF directory?

## Answer
**No.** The endpoint has multiple layers of protection against path traversal attacks.

## Security Measures Implemented

### 1. Path Component Stripping
```csharp
var safeName = Path.GetFileName(fileName);
```
- Strips all directory separators and path components
- `../../etc/passwd` becomes just `passwd`
- `subdir/file.pdf` becomes just `file.pdf`

### 2. Exact Match Validation
```csharp
if (!string.Equals(safeName, fileName, StringComparison.Ordinal))
{
    return Results.BadRequest("Invalid file name.");
}
```
- Rejects if the cleaned filename differs from the input
- Blocks: `../file.pdf`, `subdir/file.pdf`, etc.

### 3. File Extension Whitelist
```csharp
if (!safeName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
{
    return Results.BadRequest("Only PDF files are allowed.");
}
```
- Only allows `.pdf` files
- Prevents access to config files, source code, etc.
- Blocks: `appsettings.json`, `Program.cs`, `.env`

### 4. Directory Boundary Check
```csharp
var filePath = Path.Combine(outputRoot, safeName);
var fullPath = Path.GetFullPath(filePath);
var fullOutputRoot = Path.GetFullPath(outputRoot);

if (!fullPath.StartsWith(fullOutputRoot, StringComparison.OrdinalIgnoreCase))
{
    return Results.BadRequest("Invalid file path.");
}
```
- Resolves both paths to absolute paths
- Ensures the final path is within `outputRoot`
- Defense-in-depth: catches symbolic link attacks or edge cases

## Attack Vectors Tested

| Attack | Result | HTTP Status |
|--------|--------|-------------|
| `../../../etc/passwd` | ❌ Blocked | 404 Not Found |
| `..%2F..%2Fetc%2Fpasswd` (URL-encoded) | ❌ Blocked | 404 Not Found |
| `etc/passwd` | ❌ Blocked | 404 Not Found |
| `../Program.cs` | ❌ Blocked | 404 Not Found |
| `something.txt` | ❌ Blocked | 400 Bad Request ("Only PDF files are allowed.") |
| `invoice-...pdf` (legitimate) | ✅ Allowed | 200 OK |

## Additional Security Considerations

### What's Protected
✅ System files (`/etc/passwd`, `/etc/hosts`)  
✅ Application code (`Program.cs`, `.csproj`)  
✅ Configuration files (`appsettings.json`, `.env`)  
✅ Other directories (`/templates`, `/app`)  
✅ Non-PDF files in the output directory  

### Container Isolation
Even if path traversal succeeded, the Docker container provides additional isolation:
- Container runs as non-root user (ASP.NET runtime image)
- Limited file system access
- No sensitive data stored in container
- Output directory (`/data`) is explicitly mounted and writable

### Best Practices Applied
1. **Whitelist over Blacklist** - Only allows `.pdf` files
2. **Defense in Depth** - Multiple validation layers
3. **Fail Secure** - Returns generic errors, doesn't leak path info
4. **Principle of Least Privilege** - Only accesses one specific directory

## Recommendations for Production

If deploying to production, consider:

1. **Add authentication** - Require API keys or JWT tokens
2. **Add rate limiting** - Prevent brute-force filename guessing
3. **Add audit logging** - Log all file access attempts
4. **Use UUIDs only** - Current filenames include timestamps that are guessable
5. **Consider signed URLs** - Generate time-limited download tokens
6. **Add file size limits** - Prevent storage exhaustion
7. **Implement file expiry** - Automatically delete old PDFs

## Conclusion

The current implementation is **secure against common path traversal attacks**. The multi-layered validation ensures that only PDF files in the designated output directory can be accessed.
