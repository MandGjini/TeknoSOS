//
//  AppState.swift
//  TeknoSOS
//
//  Global UI state — selected tab, error alerts
//

import Foundation

@MainActor
class AppState: ObservableObject {

    // MARK: - Tabs
    enum Tab {
        case home
        case defects
        case technicians
        case chat
        case profile
    }

    @Published var selectedTab: Tab = .home

    // MARK: - Error Alert
    @Published var showingError = false
    @Published var errorMessage = ""

    func showError(_ message: String) {
        errorMessage = message
        showingError = true
    }
}
