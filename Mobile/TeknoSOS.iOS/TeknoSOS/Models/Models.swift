//
//  Models.swift
//  TeknoSOS
//
//  Shared data models for the API
//

import Foundation

// MARK: - API Response Wrapper
struct APIResponse<T: Decodable>: Decodable {
    let success: Bool
    let message: String?
    let data: T?
}

// MARK: - Pagination
struct PagedResult<T: Decodable>: Decodable {
    let items: [T]
    let page: Int
    let pageSize: Int
    let totalCount: Int
    let totalPages: Int
}

// MARK: - Auth Models
struct LoginRequest: Encodable {
    let email: String
    let password: String
    let deviceToken: String?
    let devicePlatform: String?
}

struct RegisterRequest: Encodable {
    let email: String
    let password: String
    let firstName: String?
    let lastName: String?
    let phoneNumber: String?
    let city: String?
    let country: String?
    let isProfessional: Bool
}

struct RefreshTokenRequest: Encodable {
    let accessToken: String
    let refreshToken: String
}

struct AuthResponse: Decodable {
    let accessToken: String
    let refreshToken: String
    let expiresIn: Int
    let user: User
}

// MARK: - User Model
struct User: Decodable, Identifiable {
    let id: String
    let email: String
    let firstName: String
    let lastName: String
    let displayUsername: String
    let phoneNumber: String?
    let city: String?
    let country: String?
    let profileImageUrl: String?
    let isVerified: Bool
    let role: String
    let registrationDate: Date
    
    var fullName: String {
        "\(firstName) \(lastName)"
    }
    
    var initials: String {
        let f = firstName.prefix(1)
        let l = lastName.prefix(1)
        return "\(f)\(l)".uppercased()
    }
}

struct PublicUserProfile: Decodable, Identifiable {
    let id: String
    let email: String
    let firstName: String
    let lastName: String
    let displayUsername: String
    let phoneNumber: String?
    let city: String?
    let country: String?
    let address: String?
    let postalCode: String?
    let bio: String?
    let preferredLanguage: String
    let profileImageUrl: String?
    let isVerified: Bool
    let isActive: Bool
    let role: String
    let registrationDate: Date
    let createdRequestsCount: Int
    let completedRequestsCount: Int
    let writtenReviewsCount: Int

    var fullName: String {
        "\(firstName) \(lastName)"
    }

    var initials: String {
        let f = firstName.prefix(1)
        let l = lastName.prefix(1)
        return "\(f)\(l)".uppercased()
    }
}

// MARK: - Defect Models
struct DefectSummary: Decodable, Identifiable {
    let id: Int
    let trackingCode: String
    let title: String
    let category: String
    let status: String
    let priority: String
    let city: String?
    let photoUrl: String?
    let createdAt: Date
    let hasUnreadMessages: Bool
    let distance: Double?
}

struct Defect: Decodable, Identifiable {
    let id: Int
    let trackingCode: String
    let title: String?
    let description: String?
    let category: String
    let status: String
    let priority: String?
    let city: String?
    let address: String?
    let latitude: Double?
    let longitude: Double?
    let photoUrls: [String]?
    let createdAt: Date
    let citizen: UserSummary?
    let assignedTechnician: UserSummary?
    let interestCount: Int
}

struct UserSummary: Decodable, Identifiable {
    let id: String
    let displayName: String
    let profileImageUrl: String?
}

struct CreateDefectRequest: Encodable {
    let title: String
    let description: String?
    let category: String
    let priority: String?
    let city: String?
    let address: String?
    let latitude: Double?
    let longitude: Double?
    let photoUrls: [String]?
}

// MARK: - Technician Models
struct Technician: Decodable, Identifiable {
    let id: String
    let displayName: String
    let profileImageUrl: String?
    let specialties: [String]
    let rating: Double
    let reviewCount: Int
    let city: String?
    let isAvailable: Bool
    let isVerified: Bool
    let distance: Double?
}

// MARK: - Message Models
struct Message: Decodable, Identifiable {
    let id: Int
    let content: String
    let senderId: String
    let senderName: String
    let receiverId: String
    let createdAt: Date
    let isRead: Bool
    let attachmentUrl: String?
}

// MARK: - Notification Models
struct AppNotification: Decodable, Identifiable {
    let id: Int
    let title: String
    let message: String
    let type: String
    let isRead: Bool
    let createdAt: Date
    let linkUrl: String?
}

// MARK: - Enums
enum ServiceCategory: String, CaseIterable {
    case electrical = "Electrical"
    case plumbing = "Plumbing"
    case hvac = "HVAC"
    case carpentry = "Carpentry"
    case it = "IT"
    case devices = "Devices"
    case gypsum = "Gypsum"
    case tiles = "Tiles"
    case parquet = "Parquet"
    case terraceInsulation = "TerraceInsulation"
    case architect = "Architect"
    case engineer = "Engineer"
    case general = "General"
    case evCharger = "EVCharger"
    case security = "Security"
    
    var localizedName: String {
        switch self {
        case .electrical: return "Elektrike"
        case .plumbing: return "Hidraulike"
        case .hvac: return "Ngrohje/Ftohje"
        case .carpentry: return "Zdrukthëtari"
        case .it: return "IT/Kompjuter"
        case .devices: return "Pajisje"
        case .gypsum: return "Gips"
        case .tiles: return "Pllaka"
        case .parquet: return "Parket"
        case .terraceInsulation: return "Izolim Tarace"
        case .architect: return "Arkitekt"
        case .engineer: return "Inxhinier"
        case .general: return "Të përgjithshme"
        case .evCharger: return "Karikues EV"
        case .security: return "Siguri"
        }
    }
    
    var iconName: String {
        switch self {
        case .electrical: return "bolt.fill"
        case .plumbing: return "drop.fill"
        case .hvac: return "thermometer"
        case .carpentry: return "hammer.fill"
        case .it: return "desktopcomputer"
        case .devices: return "washer.fill"
        case .gypsum: return "square.split.2x2.fill"
        case .tiles: return "square.grid.3x3.fill"
        case .parquet: return "rectangle.grid.2x2.fill"
        case .terraceInsulation: return "house.fill"
        case .architect: return "pencil.and.ruler.fill"
        case .engineer: return "gearshape.2.fill"
        case .general: return "wrench.fill"
        case .evCharger: return "bolt.car.fill"
        case .security: return "lock.shield.fill"
        }
    }
}

enum DefectStatus: String, CaseIterable {
    case created = "Created"
    case matched = "Matched"
    case inProgress = "InProgress"
    case completed = "Completed"
    case cancelled = "Cancelled"
    
    var localizedName: String {
        switch self {
        case .created: return "Krijuar"
        case .matched: return "Përputhur"
        case .inProgress: return "Në Progres"
        case .completed: return "Përfunduar"
        case .cancelled: return "Anuluar"
        }
    }
    
    var color: String {
        switch self {
        case .created: return "blue"
        case .matched: return "orange"
        case .inProgress: return "purple"
        case .completed: return "green"
        case .cancelled: return "red"
        }
    }
}

enum Priority: String, CaseIterable {
    case normal = "Normal"
    case high = "High"
    case emergency = "Emergency"
    
    var localizedName: String {
        switch self {
        case .normal: return "Normal"
        case .high: return "I Lartë"
        case .emergency: return "Urgjent"
        }
    }
}
