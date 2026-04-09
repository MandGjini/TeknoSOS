package app.teknosos.mobile

import android.app.Application
import android.app.NotificationChannel
import android.app.NotificationManager
import android.os.Build
import dagger.hilt.android.HiltAndroidApp

/**
 * TeknoSOS Application class
 * Initializes Hilt dependency injection and notification channels
 */
@HiltAndroidApp
class TeknoSOSApplication : Application() {

    override fun onCreate() {
        super.onCreate()
        createNotificationChannels()
    }

    private fun createNotificationChannels() {
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.O) {
            val notificationManager = getSystemService(NotificationManager::class.java)

            // Main notification channel
            val mainChannel = NotificationChannel(
                CHANNEL_NOTIFICATIONS,
                "Njoftimet",
                NotificationManager.IMPORTANCE_HIGH
            ).apply {
                description = "Njoftimet e TeknoSOS"
                enableVibration(true)
                enableLights(true)
            }

            // Chat messages channel
            val chatChannel = NotificationChannel(
                CHANNEL_CHAT,
                "Mesazhet e Chat",
                NotificationManager.IMPORTANCE_HIGH
            ).apply {
                description = "Mesazhet e bisedës me teknikët"
                enableVibration(true)
            }

            // Updates channel
            val updatesChannel = NotificationChannel(
                CHANNEL_UPDATES,
                "Përditësimet",
                NotificationManager.IMPORTANCE_DEFAULT
            ).apply {
                description = "Përditësime rreth defekteve tuaja"
            }

            notificationManager.createNotificationChannels(
                listOf(mainChannel, chatChannel, updatesChannel)
            )
        }
    }

    companion object {
        const val CHANNEL_NOTIFICATIONS = "teknosos_notifications"
        const val CHANNEL_CHAT = "teknosos_chat"
        const val CHANNEL_UPDATES = "teknosos_updates"
    }
}
