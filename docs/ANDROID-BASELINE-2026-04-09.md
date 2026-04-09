# Android Baseline - 09/04/2026

## Çfarë u verifikua

- Backend `dotnet build TeknoSOS.WebApp.slnx` është në gjendje build-i.
- Struktura Android ekziston me Compose, Hilt, Retrofit dhe DataStore.
- Dokumentimi mobile dhe API spec ekzistojnë.

## Çfarë u rregullua sot

### Build blockers në Android project
- U hoq `com.google.gms.google-services` plugin nga build-i Android sepse mungon `google-services.json` dhe kjo do ta bllokonte build-in lokal.
- U shtua `proguard-rules.pro` që release config të mos referojë file që mungon.
- U shtuan skedarët që mungonin në manifest:
  - `services/TeknoSOSMessagingService.kt`
  - `res/xml/data_extraction_rules.xml`
  - `res/xml/backup_rules.xml`
  - `res/drawable/ic_notification.xml`

### Security config
- U pastrua `network_security_config.xml` nga certificate pin placeholders të pavlefshme.
- U hoq domain-i CIDR jo-valid dhe u la konfigurim më i sigurt për debug/prod.

### Backend compatibility me Android
- U shtua endpoint-i `api/v1/technicians` që Android e pret tashmë.
- U shtua edhe `api/v1/technicians/{id}`.
- U shtuan endpoint-et `api/v1/messages` (conversation + send).
- U shtuan endpoint-et `api/v1/notifications` (list + register + mark-read).

### Gradle wrapper baseline
- U shtuan `gradlew`, `gradlew.bat` dhe `gradle/wrapper/*` me konfigurim për Gradle 8.7.

## Çfarë është gati për fillimin e Android development

- Auth mobile me JWT.
- Defects API mobile (`api/v1/defects`).
- Technicians API mobile (`api/v1/technicians`).
- Bazë Compose + MVVM + Hilt + Retrofit.

## Çfarë mungon ende

### 1. Gradle Wrapper
- Wrapper tani ekziston në repo.
- Bllokuesi aktual në këtë ambient është mungesa e Java/JDK (`JAVA_HOME`), ndaj build-i Android nuk u verifikua lokalisht nga terminali.

### 2. Firebase config
- `google-services.json` mungon.
- Për push notifications reale duhet shtuar më vonë nga Firebase Console.

### 3. API parity e plotë
- Messages dhe notifications në `api/v1` u mbuluan.
- Mbetet të verifikohet parity e modeleve/field mapping (p.sh. defects/business payload shape) gjatë testit real në app.

## Hapi i rekomanduar pasues

1. Hap `Mobile/TeknoSOS.Android` në Android Studio dhe gjenero Gradle wrapper.
2. Vendos një JDK (17+) dhe konfiguro `JAVA_HOME` që wrapper të ekzekutohet.
3. Bëj sync projektin dhe kap çdo compile/runtime issue Android.
4. Verifiko payload mappings gjatë testit real mobile (defects/business).
5. Shto `google-services.json` vetëm kur të fillojë puna reale me Firebase/FCM.