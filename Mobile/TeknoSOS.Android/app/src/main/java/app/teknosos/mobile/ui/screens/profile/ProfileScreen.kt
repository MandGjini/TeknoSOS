package app.teknosos.mobile.ui.screens.profile

import androidx.compose.foundation.background
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.*
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.draw.clip
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import androidx.hilt.navigation.compose.hiltViewModel
import coil.compose.AsyncImage
import app.teknosos.mobile.ui.theme.TeknoBlue
import app.teknosos.mobile.ui.viewmodels.AuthViewModel

@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun ProfileScreen(
    onOpenPublicProfile: (String) -> Unit,
    onLogout: () -> Unit,
    viewModel: AuthViewModel = hiltViewModel()
) {
    val currentUser by viewModel.currentUser.collectAsState()
    var showLogoutDialog by remember { mutableStateOf(false) }
    
    if (showLogoutDialog) {
        AlertDialog(
            onDismissRequest = { showLogoutDialog = false },
            title = { Text("Dil nga llogaria?") },
            text = { Text("Jeni i sigurt që dëshironi të dilni?") },
            confirmButton = {
                TextButton(
                    onClick = {
                        showLogoutDialog = false
                        onLogout()
                    }
                ) {
                    Text("Dil", color = MaterialTheme.colorScheme.error)
                }
            },
            dismissButton = {
                TextButton(onClick = { showLogoutDialog = false }) {
                    Text("Anulo")
                }
            }
        )
    }

    Scaffold(
        topBar = {
            TopAppBar(
                title = { Text("Profili") },
                actions = {
                    IconButton(onClick = { /* Settings */ }) {
                        Icon(Icons.Default.Settings, contentDescription = "Cilësimet")
                    }
                }
            )
        }
    ) { padding ->
        Column(
            modifier = Modifier
                .fillMaxSize()
                .padding(padding)
        ) {
            // Profile Header
            Column(
                modifier = Modifier
                    .fillMaxWidth()
                    .padding(24.dp),
                horizontalAlignment = Alignment.CenterHorizontally
            ) {
                // Avatar
                Box(
                    modifier = Modifier
                        .size(100.dp)
                        .clip(CircleShape)
                        .background(TeknoBlue.copy(alpha = 0.2f)),
                    contentAlignment = Alignment.Center
                ) {
                    if (!currentUser?.profileImageUrl.isNullOrBlank()) {
                        AsyncImage(
                            model = currentUser?.profileImageUrl,
                            contentDescription = currentUser?.fullName,
                            modifier = Modifier
                                .fillMaxSize()
                                .clip(CircleShape)
                        )
                    } else {
                        Text(
                            text = currentUser?.initials ?: "?",
                            style = MaterialTheme.typography.headlineLarge,
                            fontWeight = FontWeight.Bold,
                            color = TeknoBlue
                        )
                    }
                }
                
                Spacer(modifier = Modifier.height(16.dp))
                
                Text(
                    text = currentUser?.fullName ?: "User",
                    style = MaterialTheme.typography.headlineSmall,
                    fontWeight = FontWeight.Bold
                )
                
                Text(
                    text = currentUser?.email ?: "",
                    style = MaterialTheme.typography.bodyMedium,
                    color = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.7f)
                )
                
                currentUser?.role?.let { role ->
                    Spacer(modifier = Modifier.height(8.dp))
                    AssistChip(
                        onClick = { },
                        label = { Text(role) },
                        leadingIcon = {
                            Icon(
                                imageVector = if (role == "Professional") Icons.Default.Build else Icons.Default.Person,
                                contentDescription = null,
                                modifier = Modifier.size(18.dp)
                            )
                        }
                    )
                }

                currentUser?.id?.let { userId ->
                    Spacer(modifier = Modifier.height(12.dp))
                    OutlinedButton(onClick = { onOpenPublicProfile(userId) }) {
                        Icon(Icons.Default.Visibility, contentDescription = null)
                        Spacer(modifier = Modifier.width(8.dp))
                        Text("Shiko profilin publik")
                    }
                }
            }
            
            Spacer(modifier = Modifier.height(8.dp))
            
            // Menu Items
            Column(
                modifier = Modifier.padding(vertical = 8.dp)
            ) {
                ProfileMenuItem(
                    icon = Icons.Default.Person,
                    title = "Edito Profilin",
                    onClick = { /* TODO */ }
                )
                ProfileMenuItem(
                    icon = Icons.Default.Notifications,
                    title = "Njoftimet",
                    onClick = { /* TODO */ }
                )
                ProfileMenuItem(
                    icon = Icons.Default.Language,
                    title = "Gjuha",
                    subtitle = "Shqip",
                    onClick = { /* TODO */ }
                )
                ProfileMenuItem(
                    icon = Icons.Default.Lock,
                    title = "Privatësia & Siguria",
                    onClick = { /* TODO */ }
                )
                ProfileMenuItem(
                    icon = Icons.Default.Help,
                    title = "Ndihmë & Mbështetje",
                    onClick = { /* TODO */ }
                )
                ProfileMenuItem(
                    icon = Icons.Default.Info,
                    title = "Rreth Aplikacionit",
                    subtitle = "Version 1.0.0",
                    onClick = { /* TODO */ }
                )
                
                Spacer(modifier = Modifier
                    .fillMaxWidth()
                    .height(8.dp))
                
                ProfileMenuItem(
                    icon = Icons.Default.Logout,
                    title = "Dil",
                    tint = MaterialTheme.colorScheme.error,
                    onClick = { showLogoutDialog = true }
                )
            }
        }
    }
}

@Composable
private fun ProfileMenuItem(
    icon: ImageVector,
    title: String,
    subtitle: String? = null,
    tint: androidx.compose.ui.graphics.Color = MaterialTheme.colorScheme.onSurface,
    onClick: () -> Unit
) {
    ListItem(
        headlineContent = { 
            Text(
                text = title,
                color = tint
            ) 
        },
        supportingContent = subtitle?.let { { Text(it) } },
        leadingContent = {
            Icon(
                imageVector = icon,
                contentDescription = null,
                tint = tint
            )
        },
        trailingContent = {
            Icon(
                imageVector = Icons.Default.ChevronRight,
                contentDescription = null,
                tint = MaterialTheme.colorScheme.onSurface.copy(alpha = 0.5f)
            )
        },
        modifier = Modifier.clickable(onClick = onClick)
    )
}
