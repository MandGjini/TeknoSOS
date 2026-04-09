package app.teknosos.mobile.data.repository

import app.teknosos.mobile.data.api.TeknoSOSApi
import app.teknosos.mobile.data.models.LoginRequest
import app.teknosos.mobile.data.models.RegisterRequest
import app.teknosos.mobile.data.models.User
import app.teknosos.mobile.data.local.TokenStorage
import javax.inject.Inject
import javax.inject.Singleton

/**
 * Authentication Repository
 */
@Singleton
class AuthRepository @Inject constructor(
    private val api: TeknoSOSApi,
    private val tokenStorage: TokenStorage
) {
    private var currentUser: User? = null

    suspend fun checkAuthState(): Boolean {
        val accessToken = tokenStorage.getAccessToken() ?: return false
        val refreshToken = tokenStorage.getRefreshToken() ?: return false
        
        return try {
            val response = api.getProfile()
            if (response.success && response.data != null) {
                currentUser = response.data
                true
            } else {
                false
            }
        } catch (e: Exception) {
            // Try to refresh token
            try {
                val refreshResponse = api.refreshToken(
                    app.teknosos.mobile.data.models.RefreshTokenRequest(accessToken, refreshToken)
                )
                if (refreshResponse.success && refreshResponse.data != null) {
                    tokenStorage.saveTokens(
                        refreshResponse.data.accessToken,
                        refreshResponse.data.refreshToken
                    )
                    currentUser = refreshResponse.data.user
                    true
                } else {
                    tokenStorage.clearTokens()
                    false
                }
            } catch (e: Exception) {
                tokenStorage.clearTokens()
                false
            }
        }
    }

    suspend fun login(email: String, password: String): Result<User> {
        return try {
            val response = api.login(LoginRequest(email, password))
            if (response.success && response.data != null) {
                tokenStorage.saveTokens(response.data.accessToken, response.data.refreshToken)
                currentUser = response.data.user
                Result.success(response.data.user)
            } else {
                Result.failure(Exception(response.message ?: "Kredencialet e gabuara"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun register(
        email: String,
        password: String,
        firstName: String,
        lastName: String,
        phoneNumber: String?,
        isProfessional: Boolean
    ): Result<User> {
        return try {
            val response = api.register(
                RegisterRequest(
                    email = email,
                    password = password,
                    firstName = firstName,
                    lastName = lastName,
                    phoneNumber = phoneNumber,
                    city = null,
                    country = "AL",
                    isProfessional = isProfessional
                )
            )
            if (response.success && response.data != null) {
                tokenStorage.saveTokens(response.data.accessToken, response.data.refreshToken)
                currentUser = response.data.user
                Result.success(response.data.user)
            } else {
                Result.failure(Exception(response.message ?: "Regjistrimi dështoi"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun logout() {
        try {
            api.logout()
        } catch (e: Exception) {
            // Ignore logout errors
        }
        tokenStorage.clearTokens()
        currentUser = null
    }

    fun getCurrentUser(): User? = currentUser
}
