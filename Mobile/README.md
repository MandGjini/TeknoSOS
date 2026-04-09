# TeknoSOS Mobile Apps

Projektet mobile native për TeknoSOS platformën.

---

## 📁 STRUKTURA E PROJEKTEVE

```
TeknoSOS.WebApp/
│
├── 🌐 WEB APP (ASP.NET Core)
│   ├── Controllers/       # API Controllers
│   ├── Pages/             # Razor Pages
│   ├── Views/             # MVC Views
│   ├── Services/          # Business Logic
│   └── wwwroot/           # Static files
│
└── Mobile/
    │
    ├── 🤖 TeknoSOS.Android/    # ANDROID APP (Kotlin)
    │   ├── app/src/main/
    │   │   ├── java/app/teknosos/mobile/
    │   │   │   ├── ui/         # Compose Screens
    │   │   │   ├── data/       # API & Repository
    │   │   │   └── di/         # Dependency Injection
    │   │   └── res/            # Resources
    │   └── build.gradle.kts
    │
    ├── 🍎 TeknoSOS.iOS/        # iOS APP (SwiftUI)
    │   ├── TeknoSOS/
    │   │   ├── Views/          # SwiftUI Views
    │   │   ├── Models/         # Data Models
    │   │   └── Services/       # API Services
    │   └── TeknoSOS.xcodeproj
    │
    └── 📚 Shared/              # Shared API documentation
        └── API_SPEC.md
```

---

## 🔧 SI TË EDITONI ÇDO PLATFORMË

| Platformë | Si ta hapësh | IDE |
|-----------|--------------|-----|
| **Web** | Hap `TeknoSOS.WebApp.slnx` | Visual Studio / VS Code |
| **Android** | Hap folder `Mobile/TeknoSOS.Android/` | Android Studio |
| **iOS** | Hap `Mobile/TeknoSOS.iOS/TeknoSOS.xcodeproj` | Xcode |

---

## iOS App

### Requirements
- Xcode 15+
- iOS 16.0+ deployment target
- Swift 5.9+

### Setup
```bash
cd TeknoSOS.iOS
open TeknoSOS.xcodeproj
```

### Features
- SwiftUI + MVVM Architecture
- Combine for reactive programming
- URLSession for networking
- CoreLocation for GPS
- Push notifications via APNs
- Keychain for secure token storage

## Android App

### Requirements
- Android Studio Hedgehog (2023.1.1)+
- Kotlin 1.9+
- Min SDK 26 (Android 8.0)
- Target SDK 34 (Android 14)

### Setup
```bash
cd TeknoSOS.Android
./gradlew build
```

### Features
- Jetpack Compose UI
- MVVM + Clean Architecture
- Retrofit + OkHttp for networking
- Hilt for dependency injection
- Room for local database
- Firebase Cloud Messaging (FCM)

## API Configuration

Both apps connect to the same backend API:

**Development:**
- Base URL: `http://localhost:5050/api/v1`
- WebSocket: `ws://localhost:5050/chathub`

**Production:**
- Base URL: `https://teknosos.app/api/v1`
- WebSocket: `wss://teknosos.app/chathub`

## Authentication Flow

1. User registers/logs in via `/api/v1/auth/login`
2. Server returns JWT `accessToken` + `refreshToken`
3. Store tokens securely (Keychain/EncryptedSharedPrefs)
4. Include `Authorization: Bearer {token}` in all API calls
5. Refresh token before expiry via `/api/v1/auth/refresh`

## Push Notifications

### iOS (APNs)
1. Enable Push Notifications capability in Xcode
2. Configure APNs in Apple Developer Console
3. Send device token to backend: `POST /api/v1/notifications/register`

### Android (FCM)
1. Add `google-services.json` from Firebase Console
2. Configure FCM in Firebase Console
3. Send FCM token to backend: `POST /api/v1/notifications/register`

## Building for Release

### iOS
```bash
xcodebuild -scheme TeknoSOS -configuration Release archive
```

Per signed IPA ne iPhone me Apple Developer, shih `docs/IOS-APPLE-DEVELOPER-SETUP.md`.

### Android
```bash
./gradlew assembleRelease
```

## Versioning

Keep version numbers in sync across platforms:
- iOS: Info.plist - CFBundleShortVersionString
- Android: build.gradle - versionName

Current version: **1.0.0**
