package app.teknosos.mobile.data.models

import com.google.gson.annotations.SerializedName
import java.util.Date

/**
 * Generic API Response wrapper
 */
data class ApiResponse<T>(
    val success: Boolean,
    val message: String?,
    val data: T?
)

/**
 * Paginated result wrapper
 */
data class PagedResult<T>(
    val items: List<T>,
    val page: Int,
    val pageSize: Int,
    val totalCount: Int,
    val totalPages: Int
)

// ============== Auth Models ==============

data class LoginRequest(
    val email: String,
    val password: String,
    val deviceToken: String? = null,
    val devicePlatform: String? = "Android"
)

data class RegisterRequest(
    val email: String,
    val password: String,
    val firstName: String?,
    val lastName: String?,
    val phoneNumber: String?,
    val city: String?,
    val country: String?,
    val isProfessional: Boolean
)

data class RefreshTokenRequest(
    val accessToken: String,
    val refreshToken: String
)

data class UpdateProfileRequest(
    val firstName: String?,
    val lastName: String?,
    val phoneNumber: String?,
    val city: String?,
    val address: String?,
    val bio: String?
)

data class ChangePasswordRequest(
    val currentPassword: String,
    val newPassword: String
)

data class AuthResponse(
    val accessToken: String,
    val refreshToken: String,
    val expiresIn: Int,
    val user: User
)

// ============== User Model ==============

data class User(
    val id: String,
    val email: String,
    val firstName: String,
    val lastName: String,
    val displayUsername: String,
    val phoneNumber: String?,
    val city: String?,
    val country: String?,
    val profileImageUrl: String?,
    val isVerified: Boolean,
    val role: String,
    val registrationDate: Date
) {
    val fullName: String get() = "$firstName $lastName"
    val initials: String get() = "${firstName.firstOrNull() ?: ""}${lastName.firstOrNull() ?: ""}".uppercase()
}

data class PublicUserProfile(
    val id: String,
    val email: String,
    val firstName: String,
    val lastName: String,
    val displayUsername: String,
    val phoneNumber: String?,
    val city: String?,
    val country: String?,
    val address: String?,
    val postalCode: String?,
    val bio: String?,
    val preferredLanguage: String,
    val profileImageUrl: String?,
    val isVerified: Boolean,
    val isActive: Boolean,
    val role: String,
    val registrationDate: Date,
    val createdRequestsCount: Int,
    val completedRequestsCount: Int,
    val writtenReviewsCount: Int
) {
    val fullName: String get() = "$firstName $lastName"
    val initials: String get() = "${firstName.firstOrNull() ?: ""}${lastName.firstOrNull() ?: ""}".uppercase()
}

// ============== Defect Models ==============

data class DefectSummary(
    val id: Int,
    val trackingCode: String,
    val title: String,
    val category: String,
    val status: String,
    val priority: String,
    val city: String?,
    val photoUrl: String?,
    val createdAt: Date,
    val hasUnreadMessages: Boolean,
    val distance: Double?
)

data class Defect(
    val id: Int,
    val trackingCode: String,
    val title: String?,
    val description: String?,
    val category: String,
    val status: String,
    val priority: String?,
    val city: String?,
    val address: String?,
    val latitude: Double?,
    val longitude: Double?,
    val photoUrls: List<String>?,
    val createdAt: Date,
    val citizen: UserSummary?,
    val assignedTechnician: UserSummary?,
    val interestCount: Int
)

data class UserSummary(
    val id: String,
    val displayName: String,
    val profileImageUrl: String?
)

data class CreateDefectRequest(
    val title: String,
    val description: String?,
    val category: String,
    val priority: String?,
    val city: String?,
    val address: String?,
    val latitude: Double?,
    val longitude: Double?,
    val photoUrls: List<String>?
)

// ============== Technician Models ==============

data class Technician(
    val id: String,
    val displayName: String,
    val profileImageUrl: String?,
    val specialties: List<String>,
    val rating: Double,
    val reviewCount: Int,
    val city: String?,
    val isAvailable: Boolean,
    val isVerified: Boolean,
    val distance: Double?
)

// ============== Message Models ==============

data class Message(
    val id: Int,
    val content: String,
    val senderId: String,
    val senderName: String,
    val receiverId: String,
    val createdAt: Date,
    val isRead: Boolean,
    val attachmentUrl: String?
)

data class SendMessageRequest(
    val receiverId: String,
    val serviceRequestId: Int,
    val content: String,
    val attachmentUrl: String?
)

// ============== Notification Models ==============

data class AppNotification(
    val id: Int,
    val title: String,
    val message: String,
    val type: String,
    val isRead: Boolean,
    val createdAt: Date,
    val linkUrl: String?
)

data class RegisterDeviceRequest(
    val token: String,
    val platform: String = "Android"
)

// ============== Enums ==============

enum class ServiceCategory(val displayName: String, val icon: String) {
    Electrical("Elektrike", "bolt"),
    Plumbing("Hidraulike", "water_drop"),
    HVAC("Ngrohje/Ftohje", "thermostat"),
    Carpentry("Zdrukthëtari", "carpenter"),
    IT("IT/Kompjuter", "computer"),
    Devices("Pajisje", "washer"),
    Gypsum("Gips", "view_in_ar"),
    Tiles("Pllaka", "grid_view"),
    Parquet("Parket", "dashboard"),
    TerraceInsulation("Izolim Tarace", "home_work"),
    Architect("Arkitekt", "architecture"),
    Engineer("Inxhinier", "engineering"),
    General("Të përgjithshme", "build"),
    EVCharger("Karikues EV", "electric_car"),
    Security("Siguri", "security")
}

enum class DefectStatus(val displayName: String) {
    Created("Krijuar"),
    Matched("Përputhur"),
    InProgress("Në Progres"),
    Completed("Përfunduar"),
    Cancelled("Anuluar")
}

enum class Priority(val displayName: String) {
    Normal("Normal"),
    High("I Lartë"),
    Emergency("Urgjent")
}
