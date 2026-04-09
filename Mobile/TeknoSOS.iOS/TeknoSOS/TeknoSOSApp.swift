//
//  TeknoSOSApp.swift
//  TeknoSOS
//
//  Main entry point for iOS application
//

import SwiftUI

@main
struct TeknoSOSApp: App {
    @StateObject private var appState = AppState()
    @StateObject private var authManager = AuthenticationManager.shared
    
    init() {
        // Configure app appearance
        configureAppearance()
    }
    
    var body: some Scene {
        WindowGroup {
            ContentView()
                .environmentObject(appState)
                .environmentObject(authManager)
                .task {
                    await authManager.checkAuthState()
                }
        }
    }
    
    private func configureAppearance() {
        // Navigation bar appearance
        let navAppearance = UINavigationBarAppearance()
        navAppearance.configureWithOpaqueBackground()
        navAppearance.backgroundColor = UIColor.systemBackground
        navAppearance.titleTextAttributes = [.foregroundColor: UIColor.label]
        navAppearance.largeTitleTextAttributes = [.foregroundColor: UIColor.label]
        
        UINavigationBar.appearance().standardAppearance = navAppearance
        UINavigationBar.appearance().scrollEdgeAppearance = navAppearance
        UINavigationBar.appearance().compactAppearance = navAppearance
        
        // Tab bar appearance
        let tabAppearance = UITabBarAppearance()
        tabAppearance.configureWithOpaqueBackground()
        UITabBar.appearance().standardAppearance = tabAppearance
        UITabBar.appearance().scrollEdgeAppearance = tabAppearance
    }
}

// MARK: - App State
class AppState: ObservableObject {
    @Published var isLoading = false
    @Published var selectedTab: Tab = .home
    @Published var showingError = false
    @Published var errorMessage = ""
    
    enum Tab {
        case home
        case defects
        case technicians
        case chat
        case profile
    }
    
    func showError(_ message: String) {
        errorMessage = message
        showingError = true
    }
}
