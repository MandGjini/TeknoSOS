# TeknoSOS Android release rules
# Keep Retrofit/Gson models simple during initial mobile development.
-keepattributes Signature
-keepattributes *Annotation*
-dontwarn javax.annotation.**
-dontwarn kotlin.Unit
