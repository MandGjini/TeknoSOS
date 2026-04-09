package app.teknosos.mobile.data.api

import app.teknosos.mobile.data.models.*
import retrofit2.http.*

/**
 * TeknoSOS REST API Interface
 */
interface TeknoSOSApi {
    
    // ============== Auth Endpoints ==============
    
    @POST("auth/login")
    suspend fun login(@Body request: LoginRequest): ApiResponse<AuthResponse>
    
    @POST("auth/register")
    suspend fun register(@Body request: RegisterRequest): ApiResponse<AuthResponse>
    
    @POST("auth/refresh")
    suspend fun refreshToken(@Body request: RefreshTokenRequest): ApiResponse<AuthResponse>
    
    @GET("auth/profile")
    suspend fun getProfile(): ApiResponse<User>

    @GET("auth/profile/{id}")
    suspend fun getPublicProfile(@Path("id") id: String): ApiResponse<PublicUserProfile>
    
    @PUT("auth/profile")
    suspend fun updateProfile(@Body request: UpdateProfileRequest): ApiResponse<User>
    
    @POST("auth/logout")
    suspend fun logout(): ApiResponse<Unit>
    
    @POST("auth/change-password")
    suspend fun changePassword(@Body request: ChangePasswordRequest): ApiResponse<Unit>
    
    // ============== Defects Endpoints ==============
    
    @GET("defects")
    suspend fun getMyDefects(
        @Query("page") page: Int = 1,
        @Query("pageSize") pageSize: Int = 20,
        @Query("status") status: String? = null
    ): ApiResponse<PagedResult<DefectSummary>>
    
    @GET("defects/{id}")
    suspend fun getDefect(@Path("id") id: Int): ApiResponse<Defect>
    
    @POST("defects")
    suspend fun createDefect(@Body request: CreateDefectRequest): ApiResponse<Defect>
    
    @GET("defects/nearby")
    suspend fun getNearbyDefects(
        @Query("lat") lat: Double,
        @Query("lng") lng: Double,
        @Query("radiusKm") radiusKm: Double = 10.0,
        @Query("category") category: String? = null
    ): ApiResponse<List<DefectSummary>>
    
    // ============== Technicians Endpoints ==============
    
    @GET("technicians")
    suspend fun getTechnicians(
        @Query("category") category: String? = null,
        @Query("lat") lat: Double? = null,
        @Query("lng") lng: Double? = null,
        @Query("radiusKm") radiusKm: Double? = null
    ): ApiResponse<List<Technician>>
    
    @GET("technicians/{id}")
    suspend fun getTechnician(@Path("id") id: String): ApiResponse<Technician>
    
    // ============== Messages Endpoints ==============
    
    @GET("messages/conversation/{userId}/{serviceRequestId}")
    suspend fun getConversation(
        @Path("userId") userId: String,
        @Path("serviceRequestId") serviceRequestId: Int
    ): ApiResponse<List<Message>>
    
    @POST("messages")
    suspend fun sendMessage(@Body request: SendMessageRequest): ApiResponse<Message>
    
    // ============== Notifications Endpoints ==============
    
    @GET("notifications")
    suspend fun getNotifications(
        @Query("page") page: Int = 1,
        @Query("unreadOnly") unreadOnly: Boolean = false
    ): ApiResponse<PagedResult<AppNotification>>
    
    @POST("notifications/register")
    suspend fun registerDevice(@Body request: RegisterDeviceRequest): ApiResponse<Unit>
    
    @PUT("notifications/{id}/read")
    suspend fun markNotificationRead(@Path("id") id: Int): ApiResponse<Unit>
}
