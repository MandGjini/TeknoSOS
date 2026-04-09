package app.teknosos.mobile.ui.navigation

import androidx.compose.foundation.layout.padding
import androidx.compose.material.icons.Icons
import androidx.compose.material.icons.filled.Chat
import androidx.compose.material.icons.filled.Home
import androidx.compose.material.icons.filled.Person
import androidx.compose.material.icons.filled.Build
import androidx.compose.material.icons.filled.People
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.runtime.collectAsState
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.vector.ImageVector
import androidx.hilt.navigation.compose.hiltViewModel
import androidx.navigation.NavDestination.Companion.hierarchy
import androidx.navigation.NavGraph.Companion.findStartDestination
import androidx.navigation.compose.NavHost
import androidx.navigation.compose.composable
import androidx.navigation.compose.currentBackStackEntryAsState
import androidx.navigation.compose.rememberNavController
import app.teknosos.mobile.ui.screens.auth.AuthScreen
import app.teknosos.mobile.ui.screens.chat.ChatListScreen
import app.teknosos.mobile.ui.screens.defects.DefectsScreen
import app.teknosos.mobile.ui.screens.home.HomeScreen
import app.teknosos.mobile.ui.screens.profile.PublicProfileScreen
import app.teknosos.mobile.ui.screens.profile.ProfileScreen
import app.teknosos.mobile.ui.screens.technicians.TechniciansScreen
import app.teknosos.mobile.ui.viewmodels.AuthViewModel

/**
 * Navigation routes
 */
sealed class Screen(val route: String, val title: String, val icon: ImageVector) {
    object Home : Screen("home", "Kryefaqja", Icons.Default.Home)
    object Defects : Screen("defects", "Defektet", Icons.Default.Build)
    object Technicians : Screen("technicians", "Teknikë", Icons.Default.People)
    object Chat : Screen("chat", "Chat", Icons.Default.Chat)
    object Profile : Screen("profile", "Profili", Icons.Default.Person)
    object Auth : Screen("auth", "Hyr", Icons.Default.Person)
    data object PublicProfile : Screen("publicProfile/{userId}", "Profili", Icons.Default.Person) {
        fun createRoute(userId: String) = "publicProfile/$userId"
    }
}

val bottomNavItems = listOf(
    Screen.Home,
    Screen.Defects,
    Screen.Technicians,
    Screen.Chat,
    Screen.Profile
)

/**
 * Main Navigation Host with bottom navigation
 */
@OptIn(ExperimentalMaterial3Api::class)
@Composable
fun TeknoSOSNavHost(
    authViewModel: AuthViewModel = hiltViewModel()
) {
    val navController = rememberNavController()
    val isAuthenticated by authViewModel.isAuthenticated.collectAsState()
    
    // If not authenticated, show auth screen
    if (!isAuthenticated) {
        AuthScreen(
            onLoginSuccess = {
                // Will automatically update isAuthenticated
            }
        )
        return
    }
    
    Scaffold(
        bottomBar = {
            NavigationBar {
                val navBackStackEntry by navController.currentBackStackEntryAsState()
                val currentDestination = navBackStackEntry?.destination
                
                bottomNavItems.forEach { screen ->
                    NavigationBarItem(
                        icon = { Icon(screen.icon, contentDescription = screen.title) },
                        label = { Text(screen.title) },
                        selected = currentDestination?.hierarchy?.any { it.route == screen.route } == true,
                        onClick = {
                            navController.navigate(screen.route) {
                                popUpTo(navController.graph.findStartDestination().id) {
                                    saveState = true
                                }
                                launchSingleTop = true
                                restoreState = true
                            }
                        }
                    )
                }
            }
        }
    ) { innerPadding ->
        NavHost(
            navController = navController,
            startDestination = Screen.Home.route,
            modifier = Modifier.padding(innerPadding)
        ) {
            composable(Screen.Home.route) {
                HomeScreen(
                    onNavigateToDefects = { navController.navigate(Screen.Defects.route) },
                    onNavigateToTechnicians = { navController.navigate(Screen.Technicians.route) }
                )
            }
            composable(Screen.Defects.route) {
                DefectsScreen()
            }
            composable(Screen.Technicians.route) {
                TechniciansScreen(
                    onOpenProfile = { userId -> navController.navigate(Screen.PublicProfile.createRoute(userId)) }
                )
            }
            composable(Screen.Chat.route) {
                ChatListScreen()
            }
            composable(Screen.Profile.route) {
                ProfileScreen(
                    onOpenPublicProfile = { userId -> navController.navigate(Screen.PublicProfile.createRoute(userId)) },
                    onLogout = { authViewModel.logout() }
                )
            }
            composable(Screen.PublicProfile.route) { backStackEntry ->
                val userId = backStackEntry.arguments?.getString("userId").orEmpty()
                PublicProfileScreen(
                    userId = userId,
                    onBack = { navController.popBackStack() }
                )
            }
        }
    }
}
