package app.teknosos.mobile.data.repository

import app.teknosos.mobile.data.api.TeknoSOSApi
import app.teknosos.mobile.data.models.PublicUserProfile
import app.teknosos.mobile.data.models.Technician
import javax.inject.Inject
import javax.inject.Singleton

@Singleton
class UserRepository @Inject constructor(
    private val api: TeknoSOSApi
) {
    suspend fun getPublicProfile(id: String): Result<PublicUserProfile> {
        return try {
            val response = api.getPublicProfile(id)
            if (response.success && response.data != null) {
                Result.success(response.data)
            } else {
                Result.failure(Exception(response.message ?: "Profili nuk u gjet"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }

    suspend fun getTechnicians(): Result<List<Technician>> {
        return try {
            val response = api.getTechnicians()
            if (response.success && response.data != null) {
                Result.success(response.data)
            } else {
                Result.failure(Exception(response.message ?: "Teknikët nuk u ngarkuan"))
            }
        } catch (e: Exception) {
            Result.failure(e)
        }
    }
}