package app.teknosos.mobile.services

import android.util.Log
import com.google.firebase.messaging.FirebaseMessagingService
import com.google.firebase.messaging.RemoteMessage

class TeknoSOSMessagingService : FirebaseMessagingService() {

    override fun onNewToken(token: String) {
        super.onNewToken(token)
        Log.d("TeknoSOSMessaging", "FCM token refreshed: $token")
        // TODO: send token to backend when push notification registration flow is finalized.
    }

    override fun onMessageReceived(message: RemoteMessage) {
        super.onMessageReceived(message)
        Log.d("TeknoSOSMessaging", "Message received from: ${message.from}")
    }
}
