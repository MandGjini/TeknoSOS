package app.teknosos.mobile.di

import android.content.Context
import app.teknosos.mobile.BuildConfig
import app.teknosos.mobile.data.api.TeknoSOSApi
import app.teknosos.mobile.data.local.TokenStorage
import com.google.gson.Gson
import com.google.gson.GsonBuilder
import dagger.Module
import dagger.Provides
import dagger.hilt.InstallIn
import dagger.hilt.android.qualifiers.ApplicationContext
import dagger.hilt.components.SingletonComponent
import okhttp3.Interceptor
import okhttp3.OkHttpClient
import okhttp3.logging.HttpLoggingInterceptor
import retrofit2.Retrofit
import retrofit2.converter.gson.GsonConverterFactory
import java.util.concurrent.TimeUnit
import javax.inject.Singleton

/**
 * Hilt Dependency Injection Module
 */
@Module
@InstallIn(SingletonComponent::class)
object AppModule {

    @Provides
    @Singleton
    fun provideGson(): Gson {
        return GsonBuilder()
            .setDateFormat("yyyy-MM-dd'T'HH:mm:ss")
            .create()
    }

    @Provides
    @Singleton
    fun provideTokenStorage(@ApplicationContext context: Context): TokenStorage {
        return TokenStorage(context)
    }

    @Provides
    @Singleton
    fun provideAuthInterceptor(tokenStorage: TokenStorage): Interceptor {
        return Interceptor { chain ->
            val request = chain.request().newBuilder()
            
            tokenStorage.getAccessToken()?.let { token ->
                request.addHeader("Authorization", "Bearer $token")
            }
            
            request.addHeader("User-Agent", "TeknoSOS-Android/${BuildConfig.VERSION_NAME}")
            request.addHeader("Accept", "application/json")
            request.addHeader("Content-Type", "application/json")
            
            chain.proceed(request.build())
        }
    }

    @Provides
    @Singleton
    fun provideOkHttpClient(authInterceptor: Interceptor): OkHttpClient {
        val builder = OkHttpClient.Builder()
            .connectTimeout(30, TimeUnit.SECONDS)
            .readTimeout(30, TimeUnit.SECONDS)
            .writeTimeout(30, TimeUnit.SECONDS)
            .addInterceptor(authInterceptor)

        if (BuildConfig.DEBUG) {
            val loggingInterceptor = HttpLoggingInterceptor().apply {
                level = HttpLoggingInterceptor.Level.BODY
            }
            builder.addInterceptor(loggingInterceptor)
        }

        return builder.build()
    }

    @Provides
    @Singleton
    fun provideRetrofit(okHttpClient: OkHttpClient, gson: Gson): Retrofit {
        return Retrofit.Builder()
            .baseUrl(BuildConfig.API_URL + "/")
            .client(okHttpClient)
            .addConverterFactory(GsonConverterFactory.create(gson))
            .build()
    }

    @Provides
    @Singleton
    fun provideTeknoSOSApi(retrofit: Retrofit): TeknoSOSApi {
        return retrofit.create(TeknoSOSApi::class.java)
    }
}
