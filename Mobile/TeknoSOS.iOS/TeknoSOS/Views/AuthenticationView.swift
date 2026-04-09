//
//  AuthenticationView.swift
//  TeknoSOS
//
//  Login and Registration screens
//

import SwiftUI

struct AuthenticationView: View {
    @State private var isLoginMode = true
    
    var body: some View {
        NavigationStack {
            ScrollView {
                VStack(spacing: 30) {
                    // Logo & Title
                    VStack(spacing: 16) {
                        Image(systemName: "wrench.and.screwdriver.fill")
                            .font(.system(size: 60))
                            .foregroundStyle(
                                LinearGradient(
                                    colors: [.blue, .purple],
                                    startPoint: .topLeading,
                                    endPoint: .bottomTrailing
                                )
                            )
                        
                        Text("TeknoSOS")
                            .font(.largeTitle)
                            .fontWeight(.bold)
                        
                        Text("Lidhu me teknikë të certifikuar")
                            .font(.subheadline)
                            .foregroundColor(.secondary)
                    }
                    .padding(.top, 40)
                    
                    // Tab Picker
                    Picker("Mode", selection: $isLoginMode) {
                        Text("Hyr").tag(true)
                        Text("Regjistrohu").tag(false)
                    }
                    .pickerStyle(.segmented)
                    .padding(.horizontal)
                    
                    // Forms
                    if isLoginMode {
                        LoginForm()
                    } else {
                        RegisterForm()
                    }
                }
                .padding()
            }
            .background(Color(.systemGroupedBackground))
        }
    }
}

// MARK: - Login Form
struct LoginForm: View {
    @EnvironmentObject var authManager: AuthenticationManager
    @State private var email = ""
    @State private var password = ""
    @State private var isLoading = false
    @State private var errorMessage: String?
    
    var body: some View {
        VStack(spacing: 20) {
            VStack(spacing: 16) {
                TextField("Email", text: $email)
                    .textFieldStyle(.roundedBorder)
                    .textContentType(.emailAddress)
                    .keyboardType(.emailAddress)
                    .autocapitalization(.none)
                    .autocorrectionDisabled()
                
                SecureField("Fjalëkalimi", text: $password)
                    .textFieldStyle(.roundedBorder)
                    .textContentType(.password)
            }
            
            if let error = errorMessage {
                Text(error)
                    .font(.caption)
                    .foregroundColor(.red)
                    .multilineTextAlignment(.center)
            }
            
            Button {
                Task { await login() }
            } label: {
                if isLoading {
                    ProgressView()
                        .tint(.white)
                } else {
                    Text("Hyr")
                        .fontWeight(.semibold)
                }
            }
            .frame(maxWidth: .infinity)
            .frame(height: 50)
            .background(
                LinearGradient(
                    colors: [.blue, .purple],
                    startPoint: .leading,
                    endPoint: .trailing
                )
            )
            .foregroundColor(.white)
            .cornerRadius(12)
            .disabled(isLoading || email.isEmpty || password.isEmpty)
            
            Button("Harrove fjalëkalimin?") {
                // TODO: Forgot password flow
            }
            .font(.subheadline)
            .foregroundColor(.blue)
        }
        .padding()
        .background(Color(.secondarySystemGroupedBackground))
        .cornerRadius(16)
        .padding(.horizontal)
    }
    
    private func login() async {
        isLoading = true
        errorMessage = nil
        
        do {
            try await authManager.login(email: email, password: password)
        } catch {
            errorMessage = error.localizedDescription
        }
        
        isLoading = false
    }
}

// MARK: - Register Form
struct RegisterForm: View {
    @EnvironmentObject var authManager: AuthenticationManager
    @State private var firstName = ""
    @State private var lastName = ""
    @State private var email = ""
    @State private var password = ""
    @State private var confirmPassword = ""
    @State private var phoneNumber = ""
    @State private var isProfessional = false
    @State private var isLoading = false
    @State private var errorMessage: String?
    
    var body: some View {
        VStack(spacing: 20) {
            VStack(spacing: 16) {
                HStack(spacing: 12) {
                    TextField("Emri", text: $firstName)
                        .textFieldStyle(.roundedBorder)
                        .textContentType(.givenName)
                    
                    TextField("Mbiemri", text: $lastName)
                        .textFieldStyle(.roundedBorder)
                        .textContentType(.familyName)
                }
                
                TextField("Email", text: $email)
                    .textFieldStyle(.roundedBorder)
                    .textContentType(.emailAddress)
                    .keyboardType(.emailAddress)
                    .autocapitalization(.none)
                    .autocorrectionDisabled()
                
                TextField("Numri i telefonit", text: $phoneNumber)
                    .textFieldStyle(.roundedBorder)
                    .textContentType(.telephoneNumber)
                    .keyboardType(.phonePad)
                
                SecureField("Fjalëkalimi", text: $password)
                    .textFieldStyle(.roundedBorder)
                    .textContentType(.newPassword)
                
                SecureField("Konfirmo fjalëkalimin", text: $confirmPassword)
                    .textFieldStyle(.roundedBorder)
                    .textContentType(.newPassword)
                
                Toggle(isOn: $isProfessional) {
                    VStack(alignment: .leading) {
                        Text("Jam profesionist/teknik")
                            .fontWeight(.medium)
                        Text("Regjistrohuni si teknik për të ofruar shërbimet tuaja")
                            .font(.caption)
                            .foregroundColor(.secondary)
                    }
                }
                .padding(.vertical, 8)
            }
            
            if let error = errorMessage {
                Text(error)
                    .font(.caption)
                    .foregroundColor(.red)
                    .multilineTextAlignment(.center)
            }
            
            Button {
                Task { await register() }
            } label: {
                if isLoading {
                    ProgressView()
                        .tint(.white)
                } else {
                    Text("Regjistrohu")
                        .fontWeight(.semibold)
                }
            }
            .frame(maxWidth: .infinity)
            .frame(height: 50)
            .background(
                LinearGradient(
                    colors: [.blue, .purple],
                    startPoint: .leading,
                    endPoint: .trailing
                )
            )
            .foregroundColor(.white)
            .cornerRadius(12)
            .disabled(isLoading || !isFormValid)
            
            Text("Duke u regjistruar, pranoni Kushtet e Përdorimit dhe Politikën e Privatësisë")
                .font(.caption)
                .foregroundColor(.secondary)
                .multilineTextAlignment(.center)
        }
        .padding()
        .background(Color(.secondarySystemGroupedBackground))
        .cornerRadius(16)
        .padding(.horizontal)
    }
    
    private var isFormValid: Bool {
        !firstName.isEmpty &&
        !lastName.isEmpty &&
        !email.isEmpty &&
        !password.isEmpty &&
        password == confirmPassword &&
        password.count >= 6
    }
    
    private func register() async {
        guard password == confirmPassword else {
            errorMessage = "Fjalëkalimet nuk përputhen"
            return
        }
        
        isLoading = true
        errorMessage = nil
        
        do {
            try await authManager.register(
                email: email,
                password: password,
                firstName: firstName,
                lastName: lastName,
                phoneNumber: phoneNumber.isEmpty ? nil : phoneNumber,
                isProfessional: isProfessional
            )
        } catch {
            errorMessage = error.localizedDescription
        }
        
        isLoading = false
    }
}

#Preview {
    AuthenticationView()
        .environmentObject(AuthenticationManager.shared)
}
