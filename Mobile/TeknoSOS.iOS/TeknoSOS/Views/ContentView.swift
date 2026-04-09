//
//  ContentView.swift
//  TeknoSOS
//
//  Main navigation container
//

import SwiftUI

struct ContentView: View {
    @EnvironmentObject var appState: AppState
    @EnvironmentObject var authManager: AuthenticationManager
    
    var body: some View {
        Group {
            if authManager.isAuthenticated {
                MainTabView()
            } else {
                AuthenticationView()
            }
        }
        .alert("Gabim", isPresented: $appState.showingError) {
            Button("OK", role: .cancel) { }
        } message: {
            Text(appState.errorMessage)
        }
    }
}

// MARK: - Main Tab View
struct MainTabView: View {
    @EnvironmentObject var appState: AppState
    
    var body: some View {
        TabView(selection: $appState.selectedTab) {
            HomeView()
                .tabItem {
                    Label("Kryefaqja", systemImage: "house.fill")
                }
                .tag(AppState.Tab.home)
            
            DefectsListView()
                .tabItem {
                    Label("Defektet", systemImage: "wrench.and.screwdriver.fill")
                }
                .tag(AppState.Tab.defects)
            
            TechniciansView()
                .tabItem {
                    Label("Teknikë", systemImage: "person.2.fill")
                }
                .tag(AppState.Tab.technicians)
            
            ChatListView()
                .tabItem {
                    Label("Chat", systemImage: "message.fill")
                }
                .tag(AppState.Tab.chat)
            
            ProfileView()
                .tabItem {
                    Label("Profili", systemImage: "person.circle.fill")
                }
                .tag(AppState.Tab.profile)
        }
        .tint(.blue)
    }
}

// MARK: - Placeholder Views
struct HomeView: View {
    var body: some View {
        NavigationStack {
            ScrollView {
                VStack(spacing: 20) {
                    // Hero section
                    VStack(spacing: 12) {
                        Text("TeknoSOS")
                            .font(.largeTitle)
                            .fontWeight(.bold)
                        
                        Text("Lidhu me teknikë të certifikuar")
                            .font(.subheadline)
                            .foregroundColor(.secondary)
                    }
                    .padding(.top, 20)
                    
                    // Quick actions
                    LazyVGrid(columns: [
                        GridItem(.flexible()),
                        GridItem(.flexible())
                    ], spacing: 16) {
                        QuickActionCard(
                            title: "Raporto Defekt",
                            icon: "plus.circle.fill",
                            color: .blue
                        )
                        
                        QuickActionCard(
                            title: "Gjej Teknikë",
                            icon: "magnifyingglass",
                            color: .green
                        )
                        
                        QuickActionCard(
                            title: "Defektet e Mia",
                            icon: "list.bullet.clipboard",
                            color: .orange
                        )
                        
                        QuickActionCard(
                            title: "Urgjentë",
                            icon: "exclamationmark.triangle.fill",
                            color: .red
                        )
                    }
                    .padding(.horizontal)
                }
            }
            .navigationTitle("Kryefaqja")
        }
    }
}

struct QuickActionCard: View {
    let title: String
    let icon: String
    let color: Color
    
    var body: some View {
        VStack(spacing: 12) {
            Image(systemName: icon)
                .font(.system(size: 32))
                .foregroundColor(color)
            
            Text(title)
                .font(.subheadline)
                .fontWeight(.medium)
                .multilineTextAlignment(.center)
        }
        .frame(maxWidth: .infinity)
        .padding()
        .background(color.opacity(0.1))
        .cornerRadius(16)
    }
}

struct DefectsListView: View {
    var body: some View {
        NavigationStack {
            List {
                Text("Defektet do të shfaqen këtu")
                    .foregroundColor(.secondary)
            }
            .navigationTitle("Defektet e Mia")
            .toolbar {
                ToolbarItem(placement: .primaryAction) {
                    NavigationLink(destination: ReportDefectView()) {
                        Image(systemName: "plus")
                    }
                }
            }
        }
    }
}

struct TechniciansView: View {
    @State private var technicians: [Technician] = []
    @State private var isLoading = false
    @State private var errorMessage: String?

    var body: some View {
        NavigationStack {
            Group {
                if isLoading && technicians.isEmpty {
                    ProgressView("Duke ngarkuar teknikët...")
                } else if let errorMessage, technicians.isEmpty {
                    ContentUnavailableView("Gabim", systemImage: "exclamationmark.triangle", description: Text(errorMessage))
                } else {
                    List(technicians) { technician in
                        NavigationLink(destination: PublicProfileView(userId: technician.id)) {
                            HStack(spacing: 14) {
                                AvatarView(imageUrl: technician.profileImageUrl, fallback: String(technician.displayName.prefix(2)))
                                VStack(alignment: .leading, spacing: 4) {
                                    Text(technician.displayName)
                                        .font(.headline)
                                    Text(technician.city ?? "Pa qytet")
                                        .font(.subheadline)
                                        .foregroundColor(.secondary)
                                    Text("\(technician.reviewCount) review • \(String(format: "%.1f", technician.rating))")
                                        .font(.caption)
                                        .foregroundColor(.secondary)
                                }
                            }
                            .padding(.vertical, 4)
                        }
                    }
                }
            }
            .navigationTitle("Teknikë")
            .task {
                await loadTechnicians()
            }
        }
    }

    private func loadTechnicians() async {
        guard technicians.isEmpty else { return }
        isLoading = true
        defer { isLoading = false }

        do {
            let response = try await APIService.shared.getTechnicians()
            technicians = response.data ?? []
            errorMessage = response.success ? nil : response.message
        } catch {
            errorMessage = error.localizedDescription
        }
    }
}

struct ChatListView: View {
    var body: some View {
        NavigationStack {
            List {
                Text("Bisedat do të shfaqen këtu")
                    .foregroundColor(.secondary)
            }
            .navigationTitle("Chat")
        }
    }
}

struct ProfileView: View {
    @EnvironmentObject var authManager: AuthenticationManager
    
    var body: some View {
        NavigationStack {
            List {
                Section {
                    if let user = authManager.currentUser {
                        HStack(spacing: 16) {
                            AvatarView(imageUrl: user.profileImageUrl, fallback: user.initials, size: 60)
                            
                            VStack(alignment: .leading) {
                                Text("\(user.firstName) \(user.lastName)")
                                    .font(.headline)
                                Text(user.email)
                                    .font(.subheadline)
                                    .foregroundColor(.secondary)
                            }
                        }
                        .padding(.vertical, 8)

                        NavigationLink(destination: PublicProfileView(userId: user.id)) {
                            Label("Shiko profilin publik", systemImage: "person.text.rectangle")
                        }
                    }
                }
                
                Section("Cilësimet") {
                    NavigationLink(destination: Text("Njoftime")) {
                        Label("Njoftimet", systemImage: "bell")
                    }
                    NavigationLink(destination: Text("Gjuha")) {
                        Label("Gjuha", systemImage: "globe")
                    }
                    NavigationLink(destination: Text("Privatësia")) {
                        Label("Privatësia", systemImage: "lock")
                    }
                }
                
                Section {
                    Button(role: .destructive) {
                        Task {
                            await authManager.logout()
                        }
                    } label: {
                        Label("Dil", systemImage: "rectangle.portrait.and.arrow.right")
                    }
                }
            }
            .navigationTitle("Profili")
        }
    }
}

struct PublicProfileView: View {
    let userId: String
    @State private var profile: PublicUserProfile?
    @State private var isLoading = false
    @State private var errorMessage: String?

    var body: some View {
        ScrollView {
            if isLoading && profile == nil {
                ProgressView("Duke ngarkuar profilin...")
                    .padding(.top, 80)
            } else if let errorMessage, profile == nil {
                ContentUnavailableView("Gabim", systemImage: "exclamationmark.triangle", description: Text(errorMessage))
                    .padding(.top, 80)
            } else if let profile {
                VStack(spacing: 16) {
                    VStack(spacing: 12) {
                        AvatarView(imageUrl: profile.profileImageUrl, fallback: profile.initials, size: 110)
                        Text(profile.fullName)
                            .font(.title2)
                            .fontWeight(.bold)
                        if !profile.displayUsername.isEmpty {
                            Text("@\(profile.displayUsername)")
                                .foregroundColor(.secondary)
                        }
                        HStack(spacing: 8) {
                            ChipView(text: profile.role)
                            if profile.isVerified {
                                ChipView(text: "I verifikuar", systemImage: "checkmark.seal.fill")
                            }
                        }
                    }

                    LazyVGrid(columns: [GridItem(.flexible()), GridItem(.flexible())], spacing: 12) {
                        ProfileInfoCard(title: "Email", value: profile.email)
                        ProfileInfoCard(title: "Telefoni", value: profile.phoneNumber ?? "Nuk është shtuar")
                        ProfileInfoCard(title: "Qyteti", value: profile.city ?? "Nuk është shtuar")
                        ProfileInfoCard(title: "Adresa", value: profile.address ?? "Nuk është shtuar")
                        ProfileInfoCard(title: "Gjuha", value: profile.preferredLanguage)
                        ProfileInfoCard(title: "Anëtar që nga", value: profile.registrationDate.formatted(date: .abbreviated, time: .omitted))
                        ProfileInfoCard(title: "Kërkesa", value: "\(profile.createdRequestsCount)")
                        ProfileInfoCard(title: "Të mbyllura", value: "\(profile.completedRequestsCount)")
                        ProfileInfoCard(title: "Review", value: "\(profile.writtenReviewsCount)")
                    }

                    if let bio = profile.bio, !bio.isEmpty {
                        VStack(alignment: .leading, spacing: 8) {
                            Text("Rreth përdoruesit")
                                .font(.headline)
                            Text(bio)
                                .foregroundColor(.secondary)
                                .frame(maxWidth: .infinity, alignment: .leading)
                        }
                        .frame(maxWidth: .infinity)
                        .padding()
                        .background(Color(.secondarySystemBackground))
                        .clipShape(RoundedRectangle(cornerRadius: 16))
                    }
                }
                .padding()
            }
        }
        .navigationTitle("Profili")
        .task {
            await loadProfile()
        }
    }

    private func loadProfile() async {
        guard profile == nil else { return }
        isLoading = true
        defer { isLoading = false }

        do {
            let response = try await APIService.shared.getPublicProfile(id: userId)
            profile = response.data
            errorMessage = response.success ? nil : response.message
        } catch {
            errorMessage = error.localizedDescription
        }
    }
}

private struct AvatarView: View {
    let imageUrl: String?
    let fallback: String
    var size: CGFloat = 60

    var body: some View {
        Group {
            if let imageUrl, let url = URL(string: imageUrl), !imageUrl.isEmpty {
                AsyncImage(url: url) { image in
                    image
                        .resizable()
                        .scaledToFill()
                } placeholder: {
                    ProgressView()
                }
            } else {
                ZStack {
                    Circle().fill(Color.blue.opacity(0.2))
                    Text(fallback)
                        .font(.title3)
                        .fontWeight(.bold)
                        .foregroundColor(.blue)
                }
            }
        }
        .frame(width: size, height: size)
        .clipShape(Circle())
    }
}

private struct ChipView: View {
    let text: String
    var systemImage: String? = nil

    var body: some View {
        HStack(spacing: 6) {
            if let systemImage {
                Image(systemName: systemImage)
            }
            Text(text)
        }
        .font(.caption)
        .padding(.horizontal, 10)
        .padding(.vertical, 6)
        .background(Color.blue.opacity(0.12))
        .foregroundColor(.blue)
        .clipShape(Capsule())
    }
}

private struct ProfileInfoCard: View {
    let title: String
    let value: String

    var body: some View {
        VStack(alignment: .leading, spacing: 6) {
            Text(title)
                .font(.caption)
                .foregroundColor(.secondary)
            Text(value)
                .font(.subheadline)
                .fontWeight(.semibold)
        }
        .frame(maxWidth: .infinity, alignment: .leading)
        .padding()
        .background(Color(.secondarySystemBackground))
        .clipShape(RoundedRectangle(cornerRadius: 14))
    }
}

struct ReportDefectView: View {
    var body: some View {
        Text("Raporto Defekt - Coming Soon")
            .navigationTitle("Raporto Defekt")
    }
}

#Preview {
    ContentView()
        .environmentObject(AppState())
        .environmentObject(AuthenticationManager.shared)
}
