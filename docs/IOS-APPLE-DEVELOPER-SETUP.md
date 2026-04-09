# iOS Apple Developer Setup

Ky konfigurim e kthen build-in e iOS nga unsigned IPA ne signed IPA qe mund te instalohet ne iPhone.

## Cfare duhet ne Apple Developer

Duhet nje llogari Apple Developer me pagese.

1. Krijo ose verifiko App ID me bundle id `app.teknosos.mobile`.
2. Regjistro iPhone-in qe do testosh duke shtuar UDID ne Apple Developer.
3. Krijo nje provisioning profile `Ad Hoc` ose `iOS App Development` per `app.teknosos.mobile`.
4. Krijo ose eksporto nje certificate per signing:
   - `Apple Distribution` per `Ad Hoc`
   - ose `Apple Development` per `Development`
5. Eksporto certifikaten si `.p12` me password.

## GitHub Secrets qe duhen

Ne repository settings te GitHub, shto keto secrets:

1. `APPLE_TEAM_ID`
2. `BUILD_CERTIFICATE_BASE64`
3. `P12_PASSWORD`
4. `BUILD_PROVISION_PROFILE_BASE64`
5. `KEYCHAIN_PASSWORD`
6. `PROVISIONING_PROFILE_SPECIFIER`
7. `APPLE_CODE_SIGN_IDENTITY`
8. `IOS_EXPORT_METHOD` (opsionale: `ad-hoc` ose `development`)

## Si i gjeneron vlerat

### 1. Gjej Apple Team ID

Nga Apple Developer Account -> Membership.

### 2. Kthe `.p12` ne Base64

Ne macOS:

```bash
base64 -i TeknoSOS-signing.p12 | pbcopy
```

Ne PowerShell:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\TeknoSOS-signing.p12"))
```

Kopjo rezultatin te `BUILD_CERTIFICATE_BASE64`.

### 3. Kthe provisioning profile ne Base64

Ne macOS:

```bash
base64 -i TeknoSOS.mobileprovision | pbcopy
```

Ne PowerShell:

```powershell
[Convert]::ToBase64String([IO.File]::ReadAllBytes("C:\path\TeknoSOS.mobileprovision"))
```

Kopjo rezultatin te `BUILD_PROVISION_PROFILE_BASE64`.

### 4. P12 password

Password-i qe vendoset kur eksportohet `.p12`.

### 5. Keychain password

Mund te jete nje string i ri, p.sh. nje fjalekalim i forte random.

### 6. Provisioning profile specifier

Vendos emrin e profile-it saktesisht si ne Apple Developer, p.sh. `TeknoSOS AdHoc`.

### 7. Apple code sign identity

Per shembull:

```text
Apple Distribution: Emri Mbiemri (TEAMID1234)
```

ose:

```text
Apple Development: Emri Mbiemri (TEAMID1234)
```

Duhet te perputhet me identitetin brenda `.p12`.

### 8. iOS export method

Vendos:

1. `ad-hoc` nese ke `Apple Distribution` certificate dhe `Ad Hoc` provisioning profile.
2. `development` nese ke `Apple Development` certificate dhe development provisioning profile.

Nese nuk e vendos, workflow perdor `ad-hoc` si default.

## Si funksionon workflow

Faili `.github/workflows/ios-build.yml` punon ne dy menyra:

1. Nese mungon ndonje secret, prodhon unsigned IPA vetem per compile test.
2. Nese te gjitha secrets ekzistojne, prodhon signed IPA ne artifact `TeknoSOS-iOS-signed-ipa`.

## Si ta instalosh ne iPhone

Nese build del me signed IPA:

1. Shkarko artifact-in nga GitHub Actions.
2. Instaloje ne iPhone me nje nga keto:
   - Apple Configurator 2 ne Mac
   - Xcode Devices and Simulators ne Mac
   - 3uTools ose Sideloadly ne Windows, nese profili dhe certifikata jane valide per pajisjen

## Kufizime

1. Pa Apple Developer credentials dhe pa provisioning profile nuk mund te prodhohet IPA instalues nga GitHub Actions.
2. Pa regjistruar UDID e telefonit ne profile, build-i `Ad Hoc` nuk instalohet ne ate pajisje.
3. TestFlight kerkon nje pipeline tjeter me App Store Connect upload dhe review te build-it.