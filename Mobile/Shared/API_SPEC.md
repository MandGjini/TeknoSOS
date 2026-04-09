# TeknoSOS Mobile API Specification

## Base URLs

| Environment | URL |
|-------------|-----|
| Development | `http://localhost:5050/api/v1` |
| Production | `https://teknosos.app/api/v1` |

## Authentication

All authenticated endpoints require a JWT token in the `Authorization` header:

```
Authorization: Bearer <access_token>
```

### Token Refresh

Access tokens expire after 60 minutes. Use the refresh endpoint to get new tokens:

```http
POST /auth/refresh
Content-Type: application/json

{
  "accessToken": "expired_access_token",
  "refreshToken": "valid_refresh_token"
}
```

## API Response Format

All API responses follow this structure:

```json
{
  "success": true,
  "message": "Optional message",
  "data": { /* response data */ }
}
```

## Endpoints

### Authentication

#### Login
```http
POST /auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "deviceToken": "optional_fcm_token"
}
```

Response:
```json
{
  "success": true,
  "data": {
    "accessToken": "jwt_token",
    "refreshToken": "refresh_token",
    "expiresIn": 3600,
    "user": {
      "id": "user_id",
      "email": "user@example.com",
      "firstName": "John",
      "lastName": "Doe",
      "displayUsername": "johndoe",
      "role": "Citizen",
      "isVerified": false
    }
  }
}
```

#### Register
```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123",
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+355691234567",
  "city": "Tiranë",
  "country": "AL",
  "isProfessional": false
}
```

#### Get Profile
```http
GET /auth/profile
Authorization: Bearer <token>
```

#### Update Profile
```http
PUT /auth/profile
Authorization: Bearer <token>
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "phoneNumber": "+355691234567",
  "city": "Tiranë",
  "address": "Rruga Example 123",
  "bio": "About me text"
}
```

#### Logout
```http
POST /auth/logout
Authorization: Bearer <token>
```

#### Change Password
```http
POST /auth/change-password
Authorization: Bearer <token>
Content-Type: application/json

{
  "currentPassword": "old_password",
  "newPassword": "new_password"
}
```

---

### Defects (Service Requests)

#### Get My Defects
```http
GET /defects?page=1&pageSize=20&status=Created
Authorization: Bearer <token>
```

Query Parameters:
- `page` (int): Page number (default: 1)
- `pageSize` (int): Items per page (default: 20, max: 100)
- `status` (string): Filter by status (Created, Matched, InProgress, Completed, Cancelled)

Response:
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "trackingCode": "DEF-260307-A1B2C3",
        "title": "Electrical issue",
        "category": "Electrical",
        "status": "Created",
        "priority": "Normal",
        "city": "Tiranë",
        "photoUrl": "https://...",
        "createdAt": "2026-03-07T10:30:00Z",
        "hasUnreadMessages": false,
        "distance": null
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 1,
    "totalPages": 1
  }
}
```

#### Get Defect Details
```http
GET /defects/{id}
Authorization: Bearer <token>
```

#### Create Defect
```http
POST /defects
Authorization: Bearer <token>
Content-Type: application/json

{
  "title": "Broken electrical outlet",
  "description": "The outlet in the living room is not working",
  "category": "Electrical",
  "priority": "Normal",
  "city": "Tiranë",
  "address": "Rruga Example 123",
  "latitude": 41.3275,
  "longitude": 19.8187,
  "photoUrls": ["https://..."]
}
```

#### Get Nearby Defects (Technicians)
```http
GET /defects/nearby?lat=41.3275&lng=19.8187&radiusKm=10&category=Electrical
Authorization: Bearer <token>
```

---

### Technicians

#### Get Technicians
```http
GET /technicians?category=Electrical&lat=41.3275&lng=19.8187&radiusKm=20
Authorization: Bearer <token>
```

Response:
```json
{
  "success": true,
  "data": [
    {
      "id": "tech_id",
      "displayName": "Elektrik Pro",
      "profileImageUrl": "https://...",
      "specialties": ["Electrical", "Security"],
      "rating": 4.8,
      "reviewCount": 45,
      "city": "Tiranë",
      "isAvailable": true,
      "isVerified": true,
      "distance": 2.5
    }
  ]
}
```

---

### Messages

#### Get Conversation
```http
GET /messages/conversation/{userId}/{serviceRequestId}
Authorization: Bearer <token>
```

#### Send Message
```http
POST /messages
Authorization: Bearer <token>
Content-Type: application/json

{
  "receiverId": "user_id",
  "serviceRequestId": 1,
  "content": "Hello, I need help with...",
  "attachmentUrl": null
}
```

---

### Notifications

#### Get Notifications
```http
GET /notifications?page=1&unreadOnly=false
Authorization: Bearer <token>
```

#### Register Device for Push
```http
POST /notifications/register
Authorization: Bearer <token>
Content-Type: application/json

{
  "token": "fcm_or_apns_token",
  "platform": "Android"  // or "iOS"
}
```

#### Mark as Read
```http
PUT /notifications/{id}/read
Authorization: Bearer <token>
```

---

## Enums

### ServiceCategory
- `Electrical` - Elektrike
- `Plumbing` - Hidraulike
- `HVAC` - Ngrohje/Ftohje
- `Carpentry` - Zdrukthëtari
- `IT` - IT/Kompjuter
- `Devices` - Pajisje
- `General` - Të përgjithshme
- `EVCharger` - Karikues EV
- `Security` - Siguri

### DefectStatus
- `Created` - Krijuar
- `Matched` - Përputhur
- `InProgress` - Në Progres
- `Completed` - Përfunduar
- `Cancelled` - Anuluar

### Priority
- `Normal` - Normal
- `High` - I Lartë
- `Emergency` - Urgjent

### UserRole
- `Admin`
- `Professional`
- `Citizen`

---

## Error Codes

| HTTP Code | Description |
|-----------|-------------|
| 200 | Success |
| 201 | Created |
| 400 | Bad Request - Invalid input |
| 401 | Unauthorized - Invalid/expired token |
| 403 | Forbidden - No permission |
| 404 | Not Found |
| 409 | Conflict - Resource already exists |
| 500 | Server Error |

---

## WebSocket (SignalR)

### Chat Hub
```
URL: wss://teknosos.app/chathub?access_token={jwt_token}
```

Events:
- `ReceiveMessage(message)` - New message received
- `UserTyping(userId, serviceRequestId)` - User is typing

Methods:
- `JoinConversation(serviceRequestId)` - Join a conversation room
- `LeaveConversation(serviceRequestId)` - Leave a conversation room
- `SendMessage(receiverId, serviceRequestId, content)` - Send message

### Notification Hub
```
URL: wss://teknosos.app/notificationhub?access_token={jwt_token}
```

Events:
- `ReceiveNotification(notification)` - New notification
- `NotificationRead(notificationId)` - Notification marked as read

---

## Rate Limiting

- 100 requests per minute per user
- 1000 requests per hour per IP

---

## Swagger Documentation

Interactive API documentation available at:
- Development: `http://localhost:5050/api-docs`
- Production: `https://teknosos.app/api-docs`
