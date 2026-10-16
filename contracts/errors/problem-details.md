# Problem Details (RFC 7807)

All Identity, Feedback, Reporting, and Gateway error responses use this envelope. Do not return `{ "message": "..." }`.

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more fields are invalid.",
  "traceId": "00-...",
  "errors": {
    "overall": ["Must be between 1 and 5."]
  }
}
```

| Status | `title` |
| --- | --- |
| 400 | Validation failed |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not found |
| 409 | Conflict |
| 500 | An error occurred |

`errors` is omitted when there are no field-level messages. `traceId` is the ASP.NET Core/Serilog correlation id.
