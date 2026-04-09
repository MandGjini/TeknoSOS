# WhatsApp Cloud API Setup (Meta Developer Tools)

Ky udhezues lidhe TeknoSOS me WhatsApp Cloud API qe raportet te krijohen direkt nga WhatsApp dhe sistemi te ktheje pergjigje automatike me kodin e rastit.

## 1) Krijo App ne Meta for Developers
- Hape developers.facebook.com
- Krijo app me tipin `Business`
- Shto produktin `WhatsApp`
- Hape `WhatsApp > API Setup`
- Merr keto vlera:
  - `Phone Number ID`
  - `Temporary Access Token` (per test), pastaj `Permanent System User Token`

## 2) Konfiguro Webhook
- Te Meta: `WhatsApp > Configuration > Webhook`
- Callback URL:
  - `https://<domain>/api/whatsapp/webhook`
- Verify token:
  - nje string qe e vendos te `appsettings` (`WhatsApp:VerifyToken`)
- Subscribe field: `messages`

## 3) Konfiguro TeknoSOS (appsettings)
Shto te `WhatsApp`:

```json
"WhatsApp": {
  "ApiKey": "<internal_api_key>",
  "VerifyToken": "<same_as_meta_verify_token>",
  "PhoneNumber": "+355...",
  "AllowAnonymous": true,
  "CloudApi": {
    "ApiVersion": "v21.0",
    "PhoneNumberId": "<meta_phone_number_id>",
    "AccessToken": "<meta_access_token>",
    "Enabled": true
  }
}
```

## 4) Testo
- Dergo nje mesazh ne WhatsApp te numrit te lidhur
- TeknoSOS krijon rast automatik (`DEF-XXXXXX`)
- TeknoSOS kthen reply me kodin e rastit dhe linkun

## 5) Endpointet
- Webhook: `POST /api/whatsapp/webhook`
- Manual create: `POST /api/whatsapp/create-case`
- Status: `GET /api/whatsapp/status/{trackingCode}`
- Health: `GET /api/whatsapp/health`

## 6) Kalimi ne Production
- Përdor permanent token (jo temporary)
- Vendos webhook me HTTPS publik
- Vendos `AllowAnonymous=false` per endpointet private
- Ruaj token-at ne environment variables ose secret manager
