//
//  AuthenticationManager.swift
//  TeknoSOS
//
//  Manages user authentication state and JWT tokens
//

import Foundation
import Security

@MainActor
class AuthenticationManager: ObservableObject {
    static let shared = AuthenticationManager()
    
    @Published var isAuthenticated = false
    @Published var currentUser: User?
    @Published var accessToken: String?
    
    private let apiService = APIService.shared
    private let keychainService = "app.teknosos.mobile"
    
    private init() {}
    
    // MARK: - Auth State
    func checkAuthState() async {
        // Try to load tokens from keychain
        if let token = loadFromKeychain(key: "accessToken"),
           let refreshToken = loadFromKeychain(key: "refreshToken") {
            accessToken = token
            
            // Verify token is still valid or refresh it
            do {
                let response = try await apiService.refreshToken(
                    accessToken: token,
                    refreshToken: refreshToken
                )
                await handleAuthSuccess(response)
            } catch {
                // Token invalid, clear auth state
                await logout()
            }
        }
    }
    
    // MARK: - Login
    func login(email: String, password: String) async throws {
        let request = LoginRequest(email: email, password: password, deviceToken: nil, devicePlatform: "iOS")
        let response = try await apiService.login(request: request)
        
        if response.success, let data = response.data {
            await handleAuthSuccess(data)
        } else {
            throw AuthError.invalidCredentials(response.message ?? "Login failed")
        }
    }
    
    // MARK: - Register
    func register(
        email: String,
        password: String,
        firstName: String,
        lastName: String,
        phoneNumber: String?,
        isProfessional: Bool
    ) async throws {
        let request = RegisterRequest(
            email: email,
            password: password,
            firstName: firstName,
            lastName: lastName,
            phoneNumber: phoneNumber,
            city: nil,
            country: "AL",
            isProfessional: isProfessional
        )
        
        let response = try await apiService.register(request: request)
        
        if response.success, let data = response.data {
            await handleAuthSuccess(data)
        } else {
            throw AuthError.registrationFailed(response.message ?? "Registration failed")
        }
    }
    
    // MARK: - Logout
    func logout() async {
        // Clear keychain
        deleteFromKeychain(key: "accessToken")
        deleteFromKeychain(key: "refreshToken")
        
        // Clear state
        accessToken = nil
        currentUser = nil
        isAuthenticated = false
        
        // Notify API to invalidate refresh token
        do {
            _ = try await apiService.logout()
        } catch {
            // Ignore logout errors
        }
    }
    
    // MARK: - Private Helpers
    private func handleAuthSuccess(_ response: AuthResponse) async {
        saveToKeychain(key: "accessToken", value: response.accessToken)
        saveToKeychain(key: "refreshToken", value: response.refreshToken)
        
        accessToken = response.accessToken
        currentUser = response.user
        isAuthenticated = true
    }
    
    // MARK: - Keychain
    private func saveToKeychain(key: String, value: String) {
        let data = value.data(using: .utf8)!
        
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: keychainService,
            kSecAttrAccount as String: key,
            kSecValueData as String: data
        ]
        
        SecItemDelete(query as CFDictionary)
        SecItemAdd(query as CFDictionary, nil)
    }
    
    private func loadFromKeychain(key: String) -> String? {
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: keychainService,
            kSecAttrAccount as String: key,
            kSecReturnData as String: true,
            kSecMatchLimit as String: kSecMatchLimitOne
        ]
        
        var result: AnyObject?
        let status = SecItemCopyMatching(query as CFDictionary, &result)
        
        guard status == errSecSuccess,
              let data = result as? Data,
              let value = String(data: data, encoding: .utf8) else {
            return nil
        }
        
        return value
    }
    
    private func deleteFromKeychain(key: String) {
        let query: [String: Any] = [
            kSecClass as String: kSecClassGenericPassword,
            kSecAttrService as String: keychainService,
            kSecAttrAccount as String: key
        ]
        SecItemDelete(query as CFDictionary)
    }
}

// MARK: - Auth Errors
enum AuthError: LocalizedError {
    case invalidCredentials(String)
    case registrationFailed(String)
    case tokenExpired
    case networkError
    
    var errorDescription: String? {
        switch self {
        case .invalidCredentials(let message):
            return message
        case .registrationFailed(let message):
            return message
        case .tokenExpired:
            return "Sesioni ka skaduar. Ju lutem hyni përsëri."
        case .networkError:
            return "Gabim në lidhje. Kontrolloni internetin."
        }
    }
}
